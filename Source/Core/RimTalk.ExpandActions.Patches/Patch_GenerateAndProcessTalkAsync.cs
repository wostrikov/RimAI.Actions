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

public static class Patch_GenerateAndProcessTalkAsync
{
	private static PropertyInfo _initiatorProperty;

	private static bool _initialized;

	[HarmonyTargetMethod]
	public static MethodBase TargetMethod()
	{
		Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((Assembly a) => a.GetName().Name == "RimTalk");
		if (assembly == null)
		{
			EALogger.Warn("RimTalk assembly not found for GenerateAndProcessTalkAsync patch");
			return null;
		}
		Type type = assembly.GetType("RimTalk.Service.TalkService");
		if (type == null)
		{
			EALogger.Warn("TalkService type not found");
			return null;
		}
		MethodInfo method = type.GetMethod("GenerateAndProcessTalkAsync", BindingFlags.Static | BindingFlags.NonPublic);
		if (method == null)
		{
			method = type.GetMethod("GenerateAndProcessTalkAsync", BindingFlags.Static | BindingFlags.Public);
		}
		if (method == null)
		{
			EALogger.Warn("GenerateAndProcessTalkAsync method not found");
			return null;
		}
		EALogger.Info("Found GenerateAndProcessTalkAsync for patching");
		return method;
	}

	[HarmonyPostfix]
	public static void Postfix(object __0, object __result)
	{
		EALogger.Debug("Postfix called - GenerateAndProcessTalkAsync");
		if (!EAModMain.Settings.Enabled)
		{
			EALogger.Debug("EA is disabled, skipping");
			return;
		}
		try
		{
			Pawn pawn = ExtractPawnFromRequest(__0);
			EALogger.Debug("Extracted pawn: " + (pawn?.Name?.ToStringShort ?? "null"));
			if (__result is Task task)
			{
				EALogger.Debug("Processing async task response");
				ProcessTaskResponse(task, pawn, __0);
			}
			else
			{
				EALogger.Debug("__result is not a Task, type: " + (__result?.GetType().Name ?? "null"));
			}
		}
		catch (Exception ex)
		{
			EALogger.Error("Error in GenerateAndProcessTalkAsync postfix", ex);
		}
	}

	private static Pawn ExtractPawnFromRequest(object talkRequest)
	{
		if (talkRequest == null)
		{
			return null;
		}
		try
		{
			if (!_initialized)
			{
				Type type = talkRequest.GetType();
				_initiatorProperty = type.GetProperty("Initiator") ?? type.GetProperty("Speaker") ?? type.GetProperty("Pawn");
				_initialized = true;
			}
			if (_initiatorProperty != null)
			{
				return _initiatorProperty.GetValue(talkRequest) as Pawn;
			}
		}
		catch (Exception ex)
		{
			EALogger.Debug("Failed to extract pawn from request: " + ex.Message);
		}
		return null;
	}

	private static async void ProcessTaskResponse(Task task, Pawn pawn, object talkRequest)
	{
		try
		{
			EALogger.Debug("Awaiting task completion...");
			await task;
			EALogger.Debug("Task completed");
			PropertyInfo property = task.GetType().GetProperty("Result");
			EALogger.Debug($"Result property found: {property != null}");
			if (property != null)
			{
				object value = property.GetValue(task);
				EALogger.Debug("Result type: " + (value?.GetType().Name ?? "null"));
				string text = ExtractJsonFromResponse(value);
				EALogger.Debug($"Extracted JSON length: {text?.Length ?? 0}");
				if (!string.IsNullOrEmpty(text))
				{
					EALogger.Debug("JSON preview: " + text.Substring(0, Math.Min(200, text.Length)) + "...");
					ProcessJsonResponse(text, pawn);
				}
				else
				{
					EALogger.Debug("No JSON extracted from response");
				}
			}
		}
		catch (Exception ex)
		{
			EALogger.Debug("Task processing error: " + ex.Message);
		}
	}

	private static string ExtractJsonFromResponse(object response)
	{
		if (response == null)
		{
			return null;
		}
		if (response is string result)
		{
			return result;
		}
		Type type = response.GetType();
		PropertyInfo propertyInfo = type.GetProperty("Json") ?? type.GetProperty("RawJson") ?? type.GetProperty("Response");
		if (propertyInfo != null)
		{
			return propertyInfo.GetValue(response)?.ToString();
		}
		string text = response.ToString();
		if (text.StartsWith("{"))
		{
			return text;
		}
		return null;
	}

	private static void ProcessJsonResponse(string json, Pawn pawn)
	{
		List<string> behaviors = EaObservedParser.Parse(json);
		if (behaviors.Count == 0)
		{
			EALogger.Debug("No ea_observed in response");
			return;
		}
		string speakerName = EaObservedParser.GetSpeakerName(json) ?? pawn?.Name?.ToStringFull;
		EALogger.Debug($"Found {behaviors.Count} behaviors from {speakerName}");
		string conversationId = string.Format("{0}_{1}", pawn?.ThingID ?? "unknown", Find.TickManager?.TicksGame ?? 0);
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
					EALogger.Info($"Converting {behaviors.Count} behaviors to {toolcallResponse.Actions.Count} actions");
					Map map = pawn?.Map;
					MainThreadDispatcher.Enqueue(delegate
					{
						List<ExecutionResult> list = ActionExecutor.ExecuteAll(conversationId, toolcallResponse.Actions, map);
						int count = list.FindAll((ExecutionResult r) => r.Success).Count;
						int num = list.Count - count;
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
