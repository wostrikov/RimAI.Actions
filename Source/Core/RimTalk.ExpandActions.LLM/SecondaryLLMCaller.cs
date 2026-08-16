using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using RimAI.Core.Application;
using RimAI.Core.Catalog;
using RimAI.Core.Execution;
using RimAI.RimWorld.Application;
using Ustas.RimAI.Actions.Core;
using Ustas.RimAI.Actions.Mod;
using Ustas.RimAI.Actions.Parsing;
using Ustas.RimAI.Actions.Util;

namespace Ustas.RimAI.Actions.LLM;

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
			_aiClientFactoryType = AccessTools.TypeByName("Ustas.RimAI.Communication.Client.AIClientFactory");
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
			_roleType = AccessTools.TypeByName("Ustas.RimAI.Communication.Data.Role");
			if (_roleType == null)
			{
				_roleType = AccessTools.TypeByName("Ustas.RimAI.Communication.Data.Role");
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

	public static async Task<FrontendStructuredConversion> ConvertBehaviorsAsync(List<string> behaviors, string speakerName)
	{
		if (behaviors == null || behaviors.Count == 0)
		{
			return new FrontendStructuredConversion();
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
			EALogger.Debug($"Calling LLM for capability conversion: {behaviors.Count} behaviors");
			string text = await CallAIClientAsync(systemPrompt, userPrompt, timeout);
			if (string.IsNullOrEmpty(text))
			{
				EALogger.Warn("Empty response from LLM, using fallback");
				return FallbackConversion(behaviors, speakerName);
			}
			FrontendStructuredConversion conversion = StructuredCapabilityJsonParser.Parse(text);
			foreach (string error in conversion.Errors)
			{
				EALogger.Warn("Structured frontend result rejected before RimAI: " + error);
			}
			EALogger.Info($"LLM returned {conversion.Actions.Count} structured requests");
			return conversion;
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

	internal static Task<string> CompleteOnceAsync(string systemPrompt, string userPrompt, TimeSpan timeout)
	{
		Initialize();
		if (_initFailed)
			throw new InvalidOperationException("RimTalk AIClient is unavailable");
		return CallAIClientAsync(systemPrompt, userPrompt, timeout);
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
				Type type = AccessTools.TypeByName("Ustas.RimAI.Communication.Client.IAIClient");
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

	private static FrontendStructuredConversion FallbackConversion(List<string> behaviors, string speakerName)
	{
		FrontendStructuredConversion conversion = new FrontendStructuredConversion();
		List<LegacyStructuredAction> list = new List<LegacyStructuredAction>();
		foreach (string behavior in behaviors)
		{
			LegacyStructuredAction action = PatternMatchBehavior(behavior, speakerName);
			if (action != null)
			{
				list.Add(action);
				EALogger.Debug("Pattern matched: " + behavior + " -> " + action.Id);
			}
		}
		Dictionary<string, LegacyStructuredAction> dictionary = new Dictionary<string, LegacyStructuredAction>();
		foreach (LegacyStructuredAction item in list)
		{
			string key = item.Actor?.ToLowerInvariant() + "|" + item.Id;
			if (dictionary.ContainsKey(key))
			{
				EALogger.Debug("Dedup: " + item.Actor + " " + item.Id + " (overriding previous)");
			}
			dictionary[key] = item;
		}
		conversion.Actions.AddRange(dictionary.Values);
		return conversion;
	}

	private static LegacyStructuredAction PatternMatchBehavior(string behavior, string defaultActor)
	{
		ICapabilityCatalog catalog = RimAIApplicationHost.Catalog;
		string text = behavior.ToLowerInvariant();
		string[] array = behavior.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		string actor = ((array.Length > 1) ? array[0] : defaultActor);
		string text2 = ((array.Length > 2) ? array[array.Length - 1] : null);
		string text3 = ((array.Length > 2) ? string.Join(" ", array, 1, array.Length - 2) : ((array.Length > 1) ? array[1] : behavior));
		string verbLower = text3.ToLowerInvariant();
		if (TryMatchId(verbLower, catalog, out var exactId))
		{
			EALogger.Debug("Exact ID match: " + verbLower + " -> " + exactId);
			return new LegacyStructuredAction(exactId, actor, text2, Reason: behavior);
		}
		if (KeywordConfigManager.GetAllMovementVerbs().Any((string mv) => verbLower.Contains(mv.ToLowerInvariant())))
		{
			string text4 = text2 + " " + text;
			foreach (KeyValuePair<string, string> allTargetNounKeyword in KeywordConfigManager.GetAllTargetNounKeywords())
			{
				if (text4.Contains(allTargetNounKeyword.Key.ToLowerInvariant())
				    && TryMatchId(allTargetNounKeyword.Value, catalog, out var compositeId))
				{
					EALogger.Info("[EA] composite intent: " + text3 + "+" + allTargetNounKeyword.Key + " → " + compositeId);
					return new LegacyStructuredAction(compositeId, actor, text2, Reason: behavior);
				}
			}
		}
		string matchedId = null;
		int num = 0;
		foreach (CapabilityOwnership ownership in CapabilityOwnershipRegistry.All)
		{
			if (!TryMatchId(ownership.LegacyActionId, catalog, out var candidateId))
				continue;
			List<string> allMatchKeywords = KeywordConfigManager.GetAllMatchKeywords(ownership.LegacyActionId);
			if (allMatchKeywords.Count == 0)
				continue;
			int num2 = 0;
			foreach (string item in allMatchKeywords)
			{
				string text5 = item.ToLowerInvariant();
				if (verbLower.Contains(text5) || text5.Contains(verbLower))
					num2 = verbLower == text5 ? num2 + 100 : num2 + 10;
				else if (text.Contains(text5))
					num2++;
			}
			if (num2 > num)
			{
				num = num2;
				matchedId = candidateId;
			}
		}
		if (matchedId != null)
		{
			if (num < MinScoreThreshold)
			{
				EALogger.Info($"[EA] Match below threshold: {behavior} → {matchedId} (score={num}), rejected");
				return null;
			}
			EALogger.Debug($"Keyword match: {behavior} -> {matchedId} (score={num})");
			return new LegacyStructuredAction(matchedId, actor, text2, Reason: behavior);
		}
		EALogger.Debug("No pattern matched for behavior: " + behavior);
		return null;
	}

	private static bool TryMatchId(string requestedId, ICapabilityCatalog catalog, out string id)
	{
		id = null;
		if (RetiredCapabilityAliases.IsRetired(requestedId))
			return false;
		var lookup = CapabilityLookup.Resolve(requestedId, catalog);
		if (lookup.Status != CapabilityLookupStatus.Found || lookup.Capability == null || !lookup.Capability.IsExecutable)
			return false;
		if (CapabilityOwnershipRegistry.TryResolve(requestedId, out var ownership)
		    && ownership != null
		    && !EAModMain.Settings.IsActionEnabled(ownership.LegacyActionId))
			return false;
		id = lookup.RequestedId;
		return true;
	}
}
