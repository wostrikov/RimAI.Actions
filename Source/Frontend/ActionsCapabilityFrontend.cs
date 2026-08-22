using System;
using System.Collections.Generic;
using System.Linq;
using RimAI.Core.Application;
using RimAI.Core.Execution;
using RimAI.Core.Runtime;
using RimAI.RimWorld.Application;
using Ustas.RimAI.Actions.Mod;
using Ustas.RimAI.Actions.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace Ustas.RimAI.Actions.Frontend;

/// <summary>
/// Communication interaction adapter. Supplies conversation context and presentation.
/// Gameplay execution is owned by RimAIApplicationHost.
/// </summary>
public static class ActionsCapabilityFrontend
{
	public static IReadOnlyList<CapabilityExecutionResult> Execute(
		string conversationId,
		Pawn speaker,
		IReadOnlyList<LegacyStructuredAction> actions)
	{
		var results = new List<CapabilityExecutionResult>();
		if (!EAModMain.Settings.Enabled || actions == null || actions.Count == 0)
			return results;

		var map = speaker?.Map;
		if (map == null)
		{
			EALogger.Warn("RimTalk frontend: map unavailable");
			return results;
		}

		var language = LanguageRuntime.Current;
		var context = new FrontendContext(
			FrontendKind.RimTalk,
			conversationId,
			conversationId,
			speaker == null ? null : new global::RimAI.Core.World.PawnRef(SemanticName: speaker.LabelShort),
			MapId: map.uniqueID.ToString())
		{
			Language = language
		};

		var requests = new List<SemanticCapabilityRequest>();
		foreach (var action in actions.Take(EAModMain.Settings.MaxActionsPerConversation))
		{
			try
			{
				var request = LegacyStructuredActionAdapter.ToRequest(action, context);
				if (ShouldSkipActor(request, map))
					continue;
				requests.Add(request);
			}
			catch (Exception ex)
			{
				EALogger.Warn("RimTalk frontend rejected structured action: " + ex.Message);
			}
		}

		var executed = RimAIApplicationHost.ExecuteBatch(requests, map, PolicyFromSettings());
		foreach (var result in executed)
		{
			results.Add(result);
			EALogger.Info(
				$"[RIMAI_ACTIONS_OUTCOME] conv={conversationId} capability={result.CapabilityId} " +
				$"outcome={result.Outcome} completed={result.IsCompleted} code={result.Code}");
			ShowBubble(speaker, result);
		}

		// Accepted and completed are counted apart. A queued job is a submitted
		// intention, and reporting it as a completion is what made the old log
		// line unusable as evidence.
		int completed = results.FindAll(item => item.IsCompleted).Count;
		int queued = results.FindAll(item => item.IsQueued).Count;
		if (results.Count > 0)
			EALogger.Info(
				$"Actions batch: {completed} completed, {queued} queued, " +
				$"{results.Count - completed - queued} not executed");
		return results;
	}

	internal static ExecutionPolicyOverride PolicyFromSettings()
	{
		var settings = EAModMain.Settings;
		return new ExecutionPolicyOverride(
			settings.EnableCooldown,
			settings.DefaultCooldownTicks,
			settings.MovementCooldownTicks,
			settings.CombatCooldownTicks,
			settings.SocialCooldownTicks,
			settings.JobProtectionTicks,
			settings.AllowUndesignatedTargets,
			settings.CustomJobWhitelist,
			DisabledActions(settings));
	}

	/// <summary>
	/// The per-action toggles the settings window already exposes. They used to
	/// affect only prompt text and keyword fallback, so a model that named a
	/// disabled action anyway still executed it; carrying them to the guard is
	/// what makes the toggle authoritative.
	/// </summary>
	private static IReadOnlyCollection<string> DisabledActions(EASettings settings)
	{
		var disabled = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (var ownership in CapabilityOwnershipRegistry.All)
		{
			if (!settings.IsActionEnabled(ownership.LegacyActionId))
				disabled.Add(ownership.LegacyActionId);
		}
		return disabled;
	}

	private static bool ShouldSkipActor(SemanticCapabilityRequest request, Map map)
	{
		var pawn = WorldRefResolver.ResolvePawn(request.Actor, map);
		if (pawn == null)
			return false;
		var settings = EAModMain.Settings;
		if (settings.SkipDraftedPawns && pawn.drafter != null && pawn.drafter.Drafted)
		{
			EALogger.Debug("Skipping drafted pawn " + pawn.LabelShort);
			return true;
		}
		if (settings.SkipWorkTimePawns && pawn.timetable?.CurrentAssignment == TimeAssignmentDefOf.Work)
		{
			EALogger.Debug("Skipping work-schedule pawn " + pawn.LabelShort);
			return true;
		}
		return false;
	}

	private static void ShowBubble(Pawn pawn, CapabilityExecutionResult result)
	{
		if (pawn == null || !EAModMain.Settings.ShowExecutionBubbles)
			return;
		try
		{
			string glyph;
			Color color;
			switch (result.Outcome)
			{
				case ActionsOutcome.Completed:
					glyph = "✓ ";
					color = Color.green;
					break;
				case ActionsOutcome.Queued:
				case ActionsOutcome.Started:
					glyph = "→ ";
					color = Color.cyan;
					break;
				case ActionsOutcome.Partial:
					glyph = "~ ";
					color = Color.yellow;
					break;
				default:
					glyph = "✗ ";
					color = Color.red;
					break;
			}
			MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, glyph + result.CapabilityId, color, 2f);
		}
		catch (Exception ex)
		{
			EALogger.Debug("Failed to show execution bubble: " + ex.Message);
		}
	}
}
