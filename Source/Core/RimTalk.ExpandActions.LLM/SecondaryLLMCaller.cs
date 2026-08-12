using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Mod;
using RimTalk.ExpandActions.Parsing;
using RimTalk.ExpandActions.Util;

namespace RimTalk.ExpandActions.LLM;

public static class SecondaryLLMCaller
{
	private const int MinScoreThreshold = 10;

	private static Type _aiClientFactoryType;

	private static MethodInfo _getAIClientAsyncMethod;

	private static Type _roleType;

	private static object _roleSystem;

	private static object _roleUser;

	private static bool _initialized;

	private static bool _initFailed;

	public static void Initialize()
	{
		if (_initialized || _initFailed)
		{
			return;
		}
		try
		{
			_aiClientFactoryType = AccessTools.TypeByName("RimTalk.Client.AIClientFactory");
			if (_aiClientFactoryType == null)
			{
				EALogger.Warn("AIClientFactory not found - using fallback pattern matching");
				_initFailed = true;
				return;
			}
			_getAIClientAsyncMethod = AccessTools.Method(_aiClientFactoryType, "GetAIClientAsync");
			if (_getAIClientAsyncMethod == null)
			{
				EALogger.Warn("GetAIClientAsync not found - using fallback pattern matching");
				_initFailed = true;
				return;
			}
			_roleType = AccessTools.TypeByName("RimTalk.Data.Role");
			if (_roleType == null)
			{
				_roleType = AccessTools.TypeByName("RimTalk.Source.Data.Role");
			}
			if (_roleType != null)
			{
				_roleSystem = Enum.Parse(_roleType, "System");
				_roleUser = Enum.Parse(_roleType, "User");
			}
			_initialized = true;
			EALogger.Info("SecondaryLLMCaller initialized with RimTalk AIClient");
		}
		catch (Exception ex)
		{
			EALogger.Error("Failed to initialize SecondaryLLMCaller", ex);
			_initFailed = true;
		}
	}

	public static async Task<ToolcallResponse> ConvertBehaviorsAsync(List<string> behaviors, string speakerName)
	{
		if (behaviors == null || behaviors.Count == 0)
		{
			return new ToolcallResponse();
		}
		Initialize();
		if (_initFailed || !EAModMain.Settings.UseSecondaryLLM)
		{
			return FallbackConversion(behaviors, speakerName);
		}
		try
		{
			string systemPrompt = ToolcallPromptBuilder.BuildSystemPrompt();
			string userPrompt = ToolcallPromptBuilder.BuildUserPrompt(behaviors, speakerName);
			TimeSpan timeout = TimeSpan.FromSeconds(EAModMain.Settings.SecondaryLLMTimeout);
			EALogger.Debug($"Calling LLM for toolcall conversion: {behaviors.Count} behaviors");
			string text = await CallAIClientAsync(systemPrompt, userPrompt, timeout);
			if (string.IsNullOrEmpty(text))
			{
				EALogger.Warn("Empty response from LLM, using fallback");
				return FallbackConversion(behaviors, speakerName);
			}
			ToolcallResponse toolcallResponse = ToolcallParser.ParseAndValidate(text, CapabilityCatalogBridge.BuildEnabledCatalog());
			foreach (string validationError in toolcallResponse.ValidationErrors)
			{
				EALogger.Warn("Planner result rejected before executor: " + validationError);
			}
			EALogger.Info($"LLM returned {toolcallResponse.Actions.Count} actions");
			return toolcallResponse;
		}
		catch (TimeoutException)
		{
			EALogger.Warn("Secondary LLM call timed out, using fallback");
			return FallbackConversion(behaviors, speakerName);
		}
		catch (Exception ex2)
		{
			EALogger.Error("Secondary LLM call failed, using fallback", ex2);
			return FallbackConversion(behaviors, speakerName);
		}
	}

	private static async Task<string> CallAIClientAsync(string systemPrompt, string userPrompt, TimeSpan timeout)
	{
		_ = 3;
		try
		{
			if (!(_getAIClientAsyncMethod.Invoke(null, null) is Task getClientTask))
			{
				throw new InvalidOperationException("GetAIClientAsync returned null");
			}
			Task timeoutTask = Task.Delay(timeout);
			if (await Task.WhenAny(getClientTask, timeoutTask) == timeoutTask)
			{
				throw new TimeoutException("Timeout waiting for AIClient");
			}
			await getClientTask;
			object obj = getClientTask.GetType().GetProperty("Result")?.GetValue(getClientTask);
			if (obj == null)
			{
				throw new InvalidOperationException("AIClient is null");
			}
			object obj2 = CreateMessageList(new(object, string)[1] { (_roleSystem, systemPrompt) });
			object obj3 = CreateMessageList(new(object, string)[1] { (_roleUser, userPrompt) });
			MethodInfo method = obj.GetType().GetMethod("GetChatCompletionAsync");
			if (method == null)
			{
				Type type = AccessTools.TypeByName("RimTalk.Client.IAIClient");
				if (type != null)
				{
					method = type.GetMethod("GetChatCompletionAsync");
				}
			}
			if (method == null)
			{
				throw new InvalidOperationException("GetChatCompletionAsync not found");
			}
			if (!(method.Invoke(obj, new object[3] { obj2, obj3, null }) is Task chatTask))
			{
				throw new InvalidOperationException("GetChatCompletionAsync returned null");
			}
			if (await Task.WhenAny(chatTask, Task.Delay(timeout)) != chatTask)
			{
				throw new TimeoutException("LLM call timed out");
			}
			await chatTask;
			object obj4 = chatTask.GetType().GetProperty("Result")?.GetValue(chatTask);
			if (obj4 == null)
			{
				return null;
			}
			return obj4.GetType().GetProperty("Response")?.GetValue(obj4) as string;
		}
		catch (Exception ex)
		{
			EALogger.Error("AIClient call failed", ex);
			throw;
		}
	}

	private static object CreateMessageList((object role, string content)[] items)
	{
		Type type = typeof(ValueTuple<, >).MakeGenericType(_roleType, typeof(string));
		Type type2 = typeof(List<>).MakeGenericType(type);
		object obj = Activator.CreateInstance(type2);
		MethodInfo method = type2.GetMethod("Add");
		for (int i = 0; i < items.Length; i++)
		{
			(object role, string content) tuple = items[i];
			object item = tuple.role;
			string item2 = tuple.content;
			object obj2 = Activator.CreateInstance(type, item, item2);
			method.Invoke(obj, new object[1] { obj2 });
		}
		return obj;
	}

	private static ToolcallResponse FallbackConversion(List<string> behaviors, string speakerName)
	{
		ToolcallResponse toolcallResponse = new ToolcallResponse();
		List<ActionCall> list = new List<ActionCall>();
		foreach (string behavior in behaviors)
		{
			ActionCall actionCall = PatternMatchBehavior(behavior, speakerName);
			if (actionCall != null)
			{
				list.Add(actionCall);
				EALogger.Debug("Pattern matched: " + behavior + " -> " + actionCall.Id);
			}
		}
		Dictionary<string, ActionCall> dictionary = new Dictionary<string, ActionCall>();
		foreach (ActionCall item in list)
		{
			string key = item.Actor?.ToLowerInvariant() + "|" + item.Id;
			if (dictionary.ContainsKey(key))
			{
				EALogger.Debug("Dedup: " + item.Actor + " " + item.Id + " (overriding previous)");
			}
			dictionary[key] = item;
		}
		toolcallResponse.Actions.AddRange(dictionary.Values);
		return toolcallResponse;
	}

	private static ActionCall PatternMatchBehavior(string behavior, string defaultActor)
	{
		string text = behavior.ToLowerInvariant();
		string[] array = behavior.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		string actor = ((array.Length > 1) ? array[0] : defaultActor);
		string text2 = ((array.Length > 2) ? array[array.Length - 1] : null);
		string text3 = ((array.Length > 2) ? string.Join(" ", array, 1, array.Length - 2) : ((array.Length > 1) ? array[1] : behavior));
		string verbLower = text3.ToLowerInvariant();
		ActionDefinition byId = ActionRegistry.GetById(verbLower);
		if (byId != null && EAModMain.Settings.IsActionEnabled(byId.Id))
		{
			EALogger.Debug("Exact ID match: " + verbLower + " -> " + byId.Id);
			return new ActionCall
			{
				Id = byId.Id,
				Actor = actor,
				Target = text2,
				Reason = behavior
			};
		}
		if (KeywordConfigManager.GetAllMovementVerbs().Any((string mv) => verbLower.Contains(mv.ToLowerInvariant())))
		{
			string text4 = text2 + " " + text;
			foreach (KeyValuePair<string, string> allTargetNounKeyword in KeywordConfigManager.GetAllTargetNounKeywords())
			{
				if (text4.Contains(allTargetNounKeyword.Key.ToLowerInvariant()))
				{
					ActionDefinition byId2 = ActionRegistry.GetById(allTargetNounKeyword.Value);
					if (byId2 != null && EAModMain.Settings.IsActionEnabled(byId2.Id))
					{
						EALogger.Info("[EA] composite intent: " + text3 + "+" + allTargetNounKeyword.Key + " → " + allTargetNounKeyword.Value);
						return new ActionCall
						{
							Id = byId2.Id,
							Actor = actor,
							Target = text2,
							Reason = behavior
						};
					}
				}
			}
		}
		ActionDefinition actionDefinition = null;
		int num = 0;
		foreach (ActionDefinition enabledAction in ActionRegistry.GetEnabledActions())
		{
			List<string> allMatchKeywords = KeywordConfigManager.GetAllMatchKeywords(enabledAction.Id);
			if (allMatchKeywords.Count == 0)
			{
				continue;
			}
			int num2 = 0;
			foreach (string item in allMatchKeywords)
			{
				string text5 = item.ToLowerInvariant();
				if (verbLower.Contains(text5) || text5.Contains(verbLower))
				{
					num2 = ((!(verbLower == text5)) ? (num2 + 10) : (num2 + 100));
				}
				else if (text.Contains(text5))
				{
					num2++;
				}
			}
			if (num2 > num || (num2 == num && num2 > 0 && enabledAction.SourceModule != null && (actionDefinition == null || actionDefinition.SourceModule == null)))
			{
				num = num2;
				actionDefinition = enabledAction;
			}
		}
		if (actionDefinition != null)
		{
			if (num < 10)
			{
				EALogger.Info($"[EA] Match below threshold: {behavior} → {actionDefinition.Id} (score={num}), rejected");
				return null;
			}
			EALogger.Debug($"Keyword match: {behavior} -> {actionDefinition.Id} (score={num})");
			return new ActionCall
			{
				Id = actionDefinition.Id,
				Actor = actor,
				Target = text2,
				Reason = behavior
			};
		}
		EALogger.Debug("No pattern matched for behavior: " + behavior);
		return null;
	}
}
