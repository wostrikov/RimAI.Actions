using System;
using System.Collections.Generic;
using System.Linq;
using RimAI.Core.Application;
using RimAI.RimWorld.Application;
using RimTalk.ExpandActions.Mod;
using RimTalk.ExpandActions.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimTalk.ExpandActions.Frontend;

/// <summary>
/// RimTalk interaction adapter. Supplies conversation context and presentation.
/// Gameplay execution is owned by RimAIApplicationHost.
/// </summary>
public static class RimTalkCapabilityFrontend
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

		var context = new FrontendContext(
			FrontendKind.RimTalk,
			conversationId,
			conversationId,
			speaker == null ? null : new RimAI.Core.World.PawnRef(SemanticName: speaker.LabelShort),
			MapId: map.uniqueID.ToString());

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
			if (!result.Succeeded)
				EALogger.Warn($"[RIMAI_FRONTEND] conv={conversationId} capability={result.CapabilityId} state=FAILED code={result.Code}");
			ShowBubble(speaker, result);
		}

		int ok = results.FindAll(item => item.Succeeded).Count;
		if (ok > 0 || results.Count > 0)
			EALogger.Info($"Executed {ok} capabilities, {results.Count - ok} failed via direct RimAI path");
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
			settings.AllowUndesignatedTargets);
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
			string text = (result.Succeeded ? "✓ " : "✗ ") + result.CapabilityId;
			Color color = result.Succeeded ? Color.green : Color.red;
			MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, text, color, 2f);
		}
		catch (Exception ex)
		{
			EALogger.Debug("Failed to show execution bubble: " + ex.Message);
		}
	}
}
