using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using Ustas.RimAI.Actions.Execution;
using Ustas.RimAI.Actions.Frontend;
using Ustas.RimAI.Actions.LLM;
using Ustas.RimAI.Actions.Mod;
using Ustas.RimAI.Actions.Util;
using Verse;

namespace Ustas.RimAI.Actions.Patches;

[HarmonyPatch]
public static class Patch_CreateInteraction
{
	private static PropertyInfo _idProperty;

	private static PropertyInfo _nameProperty;

	private static bool _reflectionInitialized;

	[HarmonyTargetMethod]
	public static MethodBase TargetMethod()
	{
		Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((Assembly a) => a.GetName().Name == "Ustas.RimAI.Communication");
		if (assembly == null)
		{
			EALogger.Warn("RimTalk assembly not found for CreateInteraction patch");
			return null;
		}
		Type type = assembly.GetType("Ustas.RimAI.Communication.Service.TalkService");
		if (type == null)
		{
			EALogger.Warn("TalkService type not found");
			return null;
		}
		MethodInfo methodInfo = type.GetMethod("CreateInteraction", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		if (methodInfo == null)
		{
			Type talkResponseType = assembly.GetType("Ustas.RimAI.Communication.Data.TalkResponse");
			if (talkResponseType != null)
			{
				methodInfo = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault((MethodInfo m) => m.Name == "CreateInteraction" && m.GetParameters().Any((ParameterInfo p) => p.ParameterType == talkResponseType));
			}
		}
		if (methodInfo == null)
		{
			EALogger.Warn("CreateInteraction method not found");
			return null;
		}
		EALogger.Info("Found CreateInteraction for patching: " + methodInfo.DeclaringType.Name + "." + methodInfo.Name);
		return methodInfo;
	}

	[HarmonyPostfix]
	public static void Postfix(Pawn __0, object __1)
	{
		if (!EAModMain.Settings.Enabled)
		{
			return;
		}
		try
		{
			if (__1 == null)
			{
				return;
			}
			if (!_reflectionInitialized)
			{
				Type type = __1.GetType();
				_nameProperty = type.GetProperty("Name") ?? type.GetProperty("name");
				_idProperty = type.GetProperty("Id");
				_reflectionInitialized = true;
			}
			string text = _nameProperty?.GetValue(__1)?.ToString() ?? __0?.Name?.ToStringFull;
			if (!string.IsNullOrEmpty(text))
			{
				TalkResponseBehaviorStore.BehaviorEntry behaviorEntry = TalkResponseBehaviorStore.TryGetAndRemove(text);
				if (behaviorEntry != null && behaviorEntry.Behaviors.Count != 0)
				{
					string text2 = _idProperty?.GetValue(__1)?.ToString() ?? behaviorEntry.ConversationId;
					EALogger.Info(string.Format("Dispatching {0} behaviors for {1} (conv: {2}): {3}", behaviorEntry.Behaviors.Count, text, text2, string.Join(", ", behaviorEntry.Behaviors)));
					ProcessBehaviors(behaviorEntry.Behaviors, __0, text, text2);
				}
			}
		}
		catch (Exception ex)
		{
			EALogger.Error("Error in CreateInteraction postfix", ex);
		}
	}

	private static void ProcessBehaviors(List<string> behaviors, Pawn pawn, string speakerName, string conversationId)
	{
		Task.Run(async delegate
		{
			try
			{
				var conversion = await SecondaryLLMCaller.ConvertBehaviorsAsync(behaviors, speakerName);
				if (conversion.Actions.Count == 0)
				{
					EALogger.Debug("No capabilities converted from behaviors");
					return;
				}
				EALogger.Info($"Converted {behaviors.Count} behaviors to {conversion.Actions.Count} RimAI requests");
				MainThreadDispatcher.Enqueue(delegate
				{
					ActionsCapabilityFrontend.Execute(conversationId, pawn, conversion.Actions);
				});
			}
			catch (Exception ex)
			{
				EALogger.Error("Error processing behaviors", ex);
			}
		});
	}
}
