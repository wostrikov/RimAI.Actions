using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RimAI.Core.Application;
using RimAI.Core.Catalog;
using RimAI.Core.Execution;
using RimAI.Core.Runtime;
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
			return ProcessProviderResponse(text, behaviors, speakerName).Conversion;
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

	/// <summary>
	/// The post-provider Actions pipeline. The host has already obtained a
	/// raw payload (or failed to); this method is the only place that payload
	/// is parsed, classified and either accepted or discarded. A TestDriver
	/// fixture calls the same method so a malformed live proof never has to
	/// corrupt a paid response.
	/// </summary>
	public static ActionsProviderResponseResult ProcessProviderResponse(
		string text,
		List<string> fallbackBehaviors = null,
		string speakerName = null)
	{
		if (string.IsNullOrEmpty(text))
		{
			var empty = RimAiRuntimeGateway.ValidateActionsResponse(new ActionsResponseValidationRequest(
				ResponseReceived: false,
				RawLength: 0,
				ParsedActionCount: 0,
				ParseErrorCount: 0));
			LogResponse(empty, parsed: 0, errors: 0);
			return new ActionsProviderResponseResult(
				empty.Disposition == ActionsResponseDisposition.RetryFallback
					? FallbackOrEmpty(fallbackBehaviors, speakerName)
					: new FrontendStructuredConversion(),
				empty,
				0,
				0,
				Array.Empty<string>());
		}

		FrontendStructuredConversion conversion = StructuredCapabilityJsonParser.Parse(text);
		foreach (string error in conversion.Errors)
		{
			EALogger.Warn("Structured frontend result rejected before RimAI: " + error);
		}

		var parseErrors = conversion.Errors.ToArray();
		var verdict = RimAiRuntimeGateway.ValidateActionsResponse(new ActionsResponseValidationRequest(
			ResponseReceived: true,
			RawLength: text.Length,
			ParsedActionCount: conversion.Actions.Count,
			ParseErrorCount: conversion.Errors.Count));
		LogResponse(verdict, conversion.Actions.Count, conversion.Errors.Count);

		if (verdict.MayExecute)
			return new ActionsProviderResponseResult(conversion, verdict, conversion.Actions.Count, parseErrors.Length, parseErrors);

		// Whatever partially decoded is discarded rather than executed: the
		// rows that survived a malformed payload are an accident of parsing,
		// not an instruction the provider successfully gave.
		var discarded = verdict.Disposition == ActionsResponseDisposition.RetryFallback
			? FallbackOrEmpty(fallbackBehaviors, speakerName)
			: new FrontendStructuredConversion();
		if (discarded.Errors.Count == 0)
			discarded.Errors.Add(verdict.Classification + ": " + verdict.Reason);
		return new ActionsProviderResponseResult(
			discarded,
			verdict,
			conversion.Actions.Count,
			parseErrors.Length,
			parseErrors);
	}

	/// <summary>
	/// Runs the recognition tiers and lets the Runtime pick between them.
	/// The same method the conversation fallback uses, so a live fixture
	/// exercises the real tier path rather than a parallel one.
	/// </summary>
	public static ActionsIntentRecognition RecognizeIntent(string behavior, string speakerName)
	{
		if (string.IsNullOrWhiteSpace(behavior))
		{
			var empty = RimAiRuntimeGateway.ResolveActionsIntent(new ActionsIntentRequest(behavior ?? string.Empty));
			return new ActionsIntentRecognition(empty, null);
		}

		return PatternMatchBehavior(behavior, speakerName ?? string.Empty);
	}

	private static FrontendStructuredConversion FallbackOrEmpty(List<string> fallbackBehaviors, string speakerName)
	{
		if (fallbackBehaviors == null || fallbackBehaviors.Count == 0)
			return new FrontendStructuredConversion();
		return FallbackConversion(fallbackBehaviors, speakerName);
	}

	private static void LogResponse(ActionsResponseVerdict verdict, int parsed, int errors)
	{
		EALogger.Info(
			$"[RIMAI_ACTIONS_RESPONSE] class={verdict.Classification} disposition={verdict.Disposition} " +
			$"mayExecute={verdict.MayExecute} parsed={parsed} errors={errors} policy={verdict.DiagnosticMarker}");
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
			LegacyStructuredAction action = PatternMatchBehavior(behavior, speakerName).Action;
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

	/// <summary>
	/// Runs every recognition tier and lets the Runtime pick between them.
	/// The tiers gather candidates; they no longer decide, so tier ordering and
	/// the score threshold are reloadable policy rather than compiled constants.
	/// </summary>
	private static ActionsIntentRecognition PatternMatchBehavior(string behavior, string defaultActor)
	{
		ICapabilityCatalog catalog = RimAIApplicationHost.Catalog;
		string text = behavior.ToLowerInvariant();
		string[] array = behavior.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		string actor = ((array.Length > 1) ? array[0] : defaultActor);
		string text2 = ((array.Length > 2) ? array[array.Length - 1] : null);
		string text3 = ((array.Length > 2) ? string.Join(" ", array, 1, array.Length - 2) : ((array.Length > 1) ? array[1] : behavior));
		string verbLower = text3.ToLowerInvariant();

		TryMatchId(verbLower, catalog, out var directId);
		string compositeId = MatchComposite(catalog, verbLower, text, text2);
		string scoredId = MatchByKeywordScore(catalog, verbLower, text, out int score);

		var verdict = RimAiRuntimeGateway.ResolveActionsIntent(new ActionsIntentRequest(
			behavior,
			directId,
			compositeId,
			scoredId,
			score));

		EALogger.Info(
			$"[RIMAI_ACTIONS_INTENT] tier={verdict.Tier} accepted={verdict.Accepted} " +
			$"capability={verdict.CapabilityId ?? "none"} score={score} " +
			$"reason={verdict.Reason} policy={verdict.DiagnosticMarker}");

		if (!verdict.Accepted || string.IsNullOrEmpty(verdict.CapabilityId))
			return new ActionsIntentRecognition(verdict, null, score);

		return new ActionsIntentRecognition(
			verdict,
			new LegacyStructuredAction(verdict.CapabilityId, actor, text2, Reason: behavior),
			score);
	}

	private static string MatchComposite(ICapabilityCatalog catalog, string verbLower, string text, string target)
	{
		if (!KeywordConfigManager.GetAllMovementVerbs().Any((string mv) => verbLower.Contains(mv.ToLowerInvariant())))
			return null;
		string haystack = target + " " + text;
		foreach (KeyValuePair<string, string> noun in KeywordConfigManager.GetAllTargetNounKeywords())
		{
			if (haystack.Contains(noun.Key.ToLowerInvariant())
			    && TryMatchId(noun.Value, catalog, out var compositeId))
			{
				return compositeId;
			}
		}
		return null;
	}

	private static string MatchByKeywordScore(ICapabilityCatalog catalog, string verbLower, string text, out int best)
	{
		string matchedId = null;
		best = 0;
		foreach (CapabilityOwnership ownership in CapabilityOwnershipRegistry.All)
		{
			if (!TryMatchId(ownership.LegacyActionId, catalog, out var candidateId))
				continue;
			List<string> allMatchKeywords = KeywordConfigManager.GetAllMatchKeywords(ownership.LegacyActionId);
			if (allMatchKeywords.Count == 0)
				continue;
			int score = 0;
			foreach (string item in allMatchKeywords)
			{
				string keyword = item.ToLowerInvariant();
				if (verbLower.Contains(keyword) || keyword.Contains(verbLower))
					score = verbLower == keyword ? score + 100 : score + 10;
				else if (text.Contains(keyword))
					score++;
			}

			if (score > best)
			{
				best = score;
				matchedId = candidateId;
			}
		}
		return matchedId;
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
