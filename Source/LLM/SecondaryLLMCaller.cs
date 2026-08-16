using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RimAI.Core.Application;
using RimAI.Core.Catalog;
using RimAI.Core.Execution;
using RimAI.RimWorld.Application;
using Ustas.RimAI.Actions.Core;
using Ustas.RimAI.Actions.Mod;
using Ustas.RimAI.Actions.Parsing;
using Ustas.RimAI.Actions.Util;
using Ustas.RimAI.Core.AI;

namespace Ustas.RimAI.Actions.LLM;

/// <summary>
/// Converts observed pawn behaviors into structured capability requests
/// using the shared Core text-AI orchestrator.
/// </summary>
public static class SecondaryLLMCaller
{
	private const int MinScoreThreshold = 10;

	public static void Initialize()
	{
	}

	public static async Task<FrontendStructuredConversion> ConvertBehaviorsAsync(List<string> behaviors, string speakerName)
	{
		if (behaviors == null || behaviors.Count == 0)
		{
			return new FrontendStructuredConversion();
		}

		if (!EAModMain.Settings.UseSecondaryLLM)
		{
			return FallbackConversion(behaviors, speakerName);
		}

		try
		{
			string systemPrompt = ToolcallPromptBuilder.BuildSystemPrompt();
			string userPrompt = ToolcallPromptBuilder.BuildUserPrompt(behaviors, speakerName);
			TimeSpan timeout = TimeSpan.FromSeconds(EAModMain.Settings.SecondaryLLMTimeout);
			EALogger.Debug($"Calling shared text AI for capability conversion: {behaviors.Count} behaviors");
			string text = await CompleteOnceAsync(systemPrompt, userPrompt, timeout);
			if (string.IsNullOrEmpty(text))
			{
				EALogger.Warn("Empty response from shared text AI, using fallback");
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
		return Task.Run(() => CompleteShared(systemPrompt, userPrompt, timeout));
	}

	private static string CompleteShared(string systemPrompt, string userPrompt, TimeSpan timeout)
	{
		var request = new TextAiRequest
		{
			Messages = new[]
			{
				new TextAiMessage("system", systemPrompt ?? string.Empty),
				new TextAiMessage("user", userPrompt ?? string.Empty)
			},
			TimeoutMs = timeout.TotalMilliseconds > 0 ? (int)timeout.TotalMilliseconds : 30000,
			Caller = "actions.capability",
			Arbitration = AiRequestMetadata.FromCaller("actions.capability")
		};
		var response = SharedTextAiOrchestrator.Complete(request);
		if (!response.Succeeded)
		{
			throw new InvalidOperationException(response.Error ?? "shared text AI failed");
		}

		return response.Text;
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
