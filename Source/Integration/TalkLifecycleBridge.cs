using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RimAI.Core.Application;
using RimAI.RimWorld.Application;
using Ustas.RimAI.Actions.Execution;
using Ustas.RimAI.Actions.Frontend;
using Ustas.RimAI.Actions.LLM;
using Ustas.RimAI.Actions.Mod;
using Ustas.RimAI.Actions.Util;
using Ustas.RimAI.Communication.Data;
using Ustas.RimAI.Core.Communication;
using Verse;
using RimAI.Core.Runtime;

namespace Ustas.RimAI.Actions.Integration;

public static class TalkLifecycleBridge
{
	static readonly PresentationLanguageGuard Guard = new();
	static readonly HashSet<int> Allowed = new();
	static readonly HashSet<int> InFlight = new();
	static bool _registered;

	public static void Register()
	{
		if (_registered)
			return;
		_registered = true;
		DialogueLanguage.OverrideNativeName = () =>
			LanguagePromptContract.NativePromptName(LanguageRuntime.Current.OutputLanguage.Code);
		DialogueLanguage.OverrideDialogueInstruction = () =>
			LanguagePromptContract.BuildPawnDialogueInstruction(LanguageRuntime.Current);
		TalkLifecycle.InteractionCreated += OnInteractionCreated;
		TalkLifecycle.TalkResponseEnqueueGate += OnEnqueueGate;
		TalkLifecycle.TalkJsonReceived += OnTalkJsonReceived;
	}

	static void OnInteractionCreated(TalkInteractionCreatedArgs args)
	{
		if (!EAModMain.Settings.Enabled || args == null)
			return;
		try
		{
			string text = args.SpeakerName;
			if (string.IsNullOrEmpty(text) && args.Speaker is Pawn pawnName)
				text = pawnName.Name?.ToStringFull;
			if (string.IsNullOrEmpty(text))
				return;
			var behaviorEntry = TalkResponseBehaviorStore.TryGetAndRemove(text);
			if (behaviorEntry == null || behaviorEntry.Behaviors.Count == 0)
				return;
			string conversationId = string.IsNullOrEmpty(args.TalkId) ? behaviorEntry.ConversationId : args.TalkId;
			ProcessBehaviors(behaviorEntry.Behaviors, args.Speaker as Pawn, text, conversationId);
		}
		catch (Exception ex)
		{
			EALogger.Error("Error in InteractionCreated subscriber", ex);
		}
	}

	static bool OnEnqueueGate(object pawnStateObj, object talkResponseObj)
	{
		if (pawnStateObj is not PawnState pawnState || talkResponseObj is not TalkResponse talkResponse)
			return true;
		var key = RuntimeHelpers.GetHashCode(talkResponse);
		lock (Allowed)
		{
			if (Allowed.Contains(key))
				return true;
		}

		var original = talkResponse.Text;
		var language = LanguageRuntime.Current;
		var first = Guard.Validator.Validate(original, language);
		if (first.Verdict != OutputLanguageVerdict.ClearlyWrongLanguage)
			return true;

		lock (InFlight)
		{
			if (!InFlight.Add(key))
				return false;
		}

		_ = RimAiBackground.Run(async () =>
		{
			try
			{
				var result = await Guard.EnsureHumanTextAsync(
					original,
					language,
					new ActionsHumanTextRewriter()).ConfigureAwait(false);
				talkResponse.Text = result.Text;
				lock (Allowed)
					Allowed.Add(key);
				MainThreadDispatcher.Enqueue(() => pawnState.QueueIncomingResponse(talkResponse));
				EALogger.Info(
					"[RIMAI_LANGUAGE] output_check=held_until_corrected first=" + first.Verdict +
					" after=" + result.Validation.Verdict +
					" retry=" + result.CorrectiveRetryUsed +
					" output=" + language.OutputLanguage.Code);
			}
			catch (Exception ex)
			{
				EALogger.Debug("Language correction skipped: " + ex.Message);
				lock (Allowed)
					Allowed.Add(key);
				MainThreadDispatcher.Enqueue(() => pawnState.QueueIncomingResponse(talkResponse));
			}
			finally
			{
				lock (InFlight)
					InFlight.Remove(key);
			}
		});
		return false;
	}

	static void OnTalkJsonReceived(string json, string targetTypeName)
	{
		if (!EAModMain.Settings.Enabled || string.IsNullOrEmpty(json) || !json.Contains("ea_observed"))
			return;
		if (!string.Equals(targetTypeName, nameof(TalkResponse), StringComparison.Ordinal))
			return;
		try
		{
			ExtractAndStore(json);
		}
		catch (Exception ex)
		{
			EALogger.Debug("Error extracting ea_observed: " + ex.Message);
		}
	}

	static void ExtractAndStore(string json)
	{
		JObject jObject;
		try
		{
			jObject = JObject.Parse(json);
		}
		catch (JsonException)
		{
			return;
		}

		JToken jToken = jObject["ea_observed"];
		if (jToken == null || jToken.Type != JTokenType.Array)
			return;
		JArray jArray = (JArray)jToken;
		if (jArray.Count == 0)
			return;
		string text = jObject["name"]?.ToString();
		if (string.IsNullOrEmpty(text))
			return;
		List<string> list = new List<string>();
		foreach (JToken item in jArray)
		{
			string text2 = item.ToString();
			if (!string.IsNullOrWhiteSpace(text2))
				list.Add(text2);
		}
		if (list.Count == 0)
			return;
		string speakerPrefix = text + " ";
		List<string> list2 = list.Where(b => b.StartsWith(speakerPrefix, StringComparison.OrdinalIgnoreCase)).ToList();
		if (list2.Count == 0)
		{
			EALogger.Debug("Sanitize: " + text + " — no speaker-own behaviors in ea_observed (skipped)");
			return;
		}
		string conversationId = $"conv_{DateTime.UtcNow.Ticks}";
		TalkResponseBehaviorStore.Store(text, list2, conversationId);
		EALogger.Info($"Sanitize: stored {list2.Count} behaviors for {text}");
	}

	static void ProcessBehaviors(List<string> behaviors, Pawn pawn, string speakerName, string conversationId)
	{
		RimAiBackground.Run(async delegate
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
