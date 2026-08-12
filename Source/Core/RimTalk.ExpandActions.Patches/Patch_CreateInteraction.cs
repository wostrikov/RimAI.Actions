using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.LLM;
using RimTalk.ExpandActions.Mod;
using RimTalk.ExpandActions.Parsing;
using RimTalk.ExpandActions.Util;
using Verse;

namespace RimTalk.ExpandActions.Patches;

[HarmonyPatch]
public static class Patch_CreateInteraction
{
	private static PropertyInfo _idProperty;

	private static PropertyInfo _nameProperty;

	private static bool _reflectionInitialized;

	[HarmonyTargetMethod]
	public static MethodBase TargetMethod()
	{
		Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((Assembly a) => a.GetName().Name == "RimTalk");
		if (assembly == null)
		{
			EALogger.Warn("RimTalk assembly not found for CreateInteraction patch");
			return null;
		}
		Type type = assembly.GetType("RimTalk.Service.TalkService");
		if (type == null)
		{
			EALogger.Warn("TalkService type not found");
			return null;
		}
		MethodInfo methodInfo = type.GetMethod("CreateInteraction", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		if (methodInfo == null)
		{
			Type talkResponseType = assembly.GetType("RimTalk.Data.TalkResponse");
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
				ToolcallResponse toolcallResponse = await SecondaryLLMCaller.ConvertBehaviorsAsync(behaviors, speakerName);
				if (toolcallResponse.Actions.Count == 0)
				{
					EALogger.Debug("No actions converted from behaviors");
				}
				else
				{
					EALogger.Info($"Converted {behaviors.Count} behaviors to {toolcallResponse.Actions.Count} actions");
					Map map = pawn?.Map;
					MainThreadDispatcher.Enqueue(delegate
					{
						List<ExecutionResult> list = ActionExecutor.ExecuteAll(conversationId, toolcallResponse.Actions, map);
						int count = list.FindAll((ExecutionResult r) => r.Success).Count;
						int num = list.Count - count;
						foreach (ExecutionResult failed in list.FindAll((ExecutionResult r) => !r.Success))
						{
							EALogger.Warn($"[EA_TRACE] conv={conversationId} action={failed.ActionId ?? "unknown"} state=FAILED code={failed.ErrorCode}");
						}
						if (count > 0 || num > 0)
						{
							EALogger.Info($"Executed {count} actions, {num} failed");
						}
					});
				}
			}
			catch (Exception ex)
			{
				EALogger.Error("Error processing behaviors", ex);
			}
		});
	}
}
