using System.Collections.Generic;
using System.Linq;
using RimAI.Core.Application;
using RimAI.Core.Runtime;
using Ustas.RimAI.Actions.Frontend;
using Ustas.RimAI.Actions.LLM;
using Ustas.RimAI.Actions.Core;
using Ustas.RimAI.Actions.Parsing;
using Ustas.RimAI.Core.TestDriver;
using Verse;

namespace Ustas.RimAI.Actions.Integration;

/// <summary>
/// Deterministic TestDriver fixture for the Actions post-provider pipeline.
/// It never calls a paid provider: the caller supplies the raw payload or the
/// utterance. Both modes share the same methods the conversation path uses.
/// </summary>
public static class ActionsPipelineProbe
{
	public static void Register()
	{
		TestDriverModuleOperations.Register(
			TestDriverCommandNames.ProbeActions,
			(request, _) => new TestDriverDelegateOperation(() => Run(request)));
	}

	static TestDriverProgress Run(TestDriverRequest request)
	{
		if (Current.ProgramState != ProgramState.Playing || Find.CurrentMap == null)
			return TestDriverProgress.Failed("probe_actions requires a loaded game");

		var mode = request.Arguments.GetString("mode");
		var correlationId = request.Arguments.GetString("correlationId", request.RequestId);
		if (string.Equals(mode, "provider_response", System.StringComparison.OrdinalIgnoreCase))
			return ProbeProviderResponse(request, correlationId);
		if (string.Equals(mode, "intent", System.StringComparison.OrdinalIgnoreCase))
			return ProbeIntent(request, correlationId);
		return TestDriverProgress.Failed("mode must be provider_response or intent");
	}

	static TestDriverProgress ProbeProviderResponse(TestDriverRequest request, string correlationId)
	{
		var raw = request.Arguments.GetString("raw");
		var processed = SecondaryLLMCaller.ProcessProviderResponse(raw, fallbackBehaviors: null);
		var verdict = processed.Verdict;

		return TestDriverProgress.Completed(new TestDriverJsonWriter()
			.Text("mode", "provider_response")
			.Text("correlationId", correlationId)
			.Text("classification", verdict.Classification)
			.Text("disposition", verdict.Disposition.ToString())
			.Flag("mayExecute", verdict.MayExecute)
			.Flag("executorInvoked", false)
			.Integer("parsedActionCount", processed.ParsedActionCount)
			.Integer("parseErrorCount", processed.ParseErrorCount)
			.TextArray("errors", processed.ParseErrors)
			.Text("policyMarker", verdict.DiagnosticMarker)
			.Flag("paused", Find.TickManager?.Paused ?? true)
			.Integer("ticksGame", Find.TickManager?.TicksGame ?? 0));
	}

	static TestDriverProgress ProbeIntent(TestDriverRequest request, string correlationId)
	{
		var utterance = request.Arguments.GetString("text");
		if (string.IsNullOrWhiteSpace(utterance))
			return TestDriverProgress.Failed("intent mode requires text");

		var pawn = Find.CurrentMap.mapPawns.FreeColonistsSpawned.FirstOrDefault(p => !p.Dead && !p.Downed);
		if (pawn == null)
			return TestDriverProgress.Failed("no eligible colonist");

		var recognition = SecondaryLLMCaller.RecognizeIntent(utterance, pawn.LabelShort);
		var verdict = recognition.Verdict;
		var execute = request.Arguments.GetBool("execute", false);
		var executorInvoked = false;
		var guardReached = false;
		var guardAllowed = false;
		string guardId = null;
		string guardCode = null;
		string outcome = null;
		string resultCode = null;
		var completed = false;
		var queued = false;

		if (verdict.Accepted && recognition.Action != null)
		{
			var action = recognition.Action with { Actor = pawn.LabelShort };
			if (execute)
			{
				var results = ActionsCapabilityFrontend.Execute(
					correlationId,
					pawn,
					new List<LegacyStructuredAction> { action });
				executorInvoked = true;
				guardReached = true;
				var result = results.FirstOrDefault();
				if (result != null)
				{
					outcome = result.Outcome.ToString();
					resultCode = result.Code;
					completed = result.IsCompleted;
					queued = result.IsQueued;
					guardAllowed = result.Outcome != ActionsOutcome.Rejected;
				}
			}
			else
			{
				var snapshot = new ActionsActorSnapshot(
					Resolved: true,
					Dead: pawn.Dead || pawn.Destroyed,
					Downed: pawn.Downed,
					Drafted: pawn.drafter is { Drafted: true },
					MentalStateDefName: pawn.InMentalState ? pawn.MentalStateDef?.defName : null,
					CurrentJobDefName: pawn.CurJobDef?.defName);
				var guard = RimAiRuntimeGateway.EvaluateActionsExecutionGuards(
					new ActionsGuardRequest(
						verdict.CapabilityId,
						LegacyActionId: null,
						Family: null,
						snapshot,
						TargetRequired: false,
						TargetResolved: true,
						ActionEnabled: true,
						JobWhitelist: null));
				guardReached = true;
				guardAllowed = guard.Allowed;
				guardId = guard.GuardId;
				guardCode = guard.DeniedCode;
			}
		}

		return TestDriverProgress.Completed(new TestDriverJsonWriter()
			.Text("mode", "intent")
			.Text("correlationId", correlationId)
			.Text("utterance", utterance)
			.Text("language", KeywordConfigManager.GetCurrentLanguage())
			.Text("actor", pawn.LabelShort)
			.Text("tier", verdict.Tier.ToString())
			.Flag("accepted", verdict.Accepted)
			.Text("capabilityId", verdict.CapabilityId)
			.Integer("keywordScore", recognition.KeywordScore)
			.Text("reason", verdict.Reason)
			.Text("policyMarker", verdict.DiagnosticMarker)
			.Flag("guardReached", guardReached)
			.Flag("guardAllowed", guardAllowed)
			.Text("guardId", guardId)
			.Text("guardCode", guardCode)
			.Flag("executorInvoked", executorInvoked)
			.Text("outcome", outcome)
			.Text("resultCode", resultCode)
			.Flag("completed", completed)
			.Flag("queued", queued)
			.Flag("paused", Find.TickManager?.Paused ?? true)
			.Integer("ticksGame", Find.TickManager?.TicksGame ?? 0));
	}
}
