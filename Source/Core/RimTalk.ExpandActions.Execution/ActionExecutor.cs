using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Integration;
using RimTalk.ExpandActions.Mod;
using RimTalk.ExpandActions.Parsing;
using RimTalk.ExpandActions.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimTalk.ExpandActions.Execution;

public static class ActionExecutor
{
	private static readonly Dictionary<string, int> ActionPriorityMap = new Dictionary<string, int>
	{
		{ "arrest", 5 },
		{ "mental_break_start", 5 },
		{ "execute", 5 },
		{ "play_music", 1 },
		{ "lovin", 1 },
		{ "social_fight", 1 },
		{ "insult", 1 },
		{ "visit_sick", 1 },
		{ "romance_set", 1 },
		{ "thought_add", 1 },
		{ "relation_set", 1 },
		{ "give_inspiration", 1 },
		{ "marry", 1 },
		{ "recruit", 1 }
	};

	private const int DefaultPriority = 3;

	private static readonly HashSet<string> BaseJobWhitelist = new HashSet<string>
	{
		"Goto", "Wait", "Wait_Combat", "LayDown", "Ingest", "AttackMelee", "AttackStatic", "Arrest", "FleeAndCower", "Follow",
		"Flee", "Rescue", "TendPatient", "FeedPatient", "DropEquipment", "Equip", "TakeInventory", "HaulToCell", "HaulToContainer", "CutPlant",
		"Harvest", "Sow", "Mine", "FinishFrame", "Deconstruct", "Repair", "DoBill", "Clean", "Haul", "Research",
		"Train", "Tame"
	};

	public static List<ExecutionResult> ExecuteAll(string conversationId, List<ActionCall> actions, Map map)
	{
		List<ExecutionResult> list = new List<ExecutionResult>();
		EASettings settings = EAModMain.Settings;
		if (!settings.Enabled)
		{
			EALogger.Debug("EA disabled, skipping all actions");
			return list;
		}
		List<ActionCall> list2 = actions.OrderBy((ActionCall a) => a.Priority).ToList();
		if (list2.Count > settings.MaxActionsPerConversation)
		{
			EALogger.Warn($"Truncating actions from {list2.Count} to {settings.MaxActionsPerConversation}");
			list2 = list2.Take(settings.MaxActionsPerConversation).ToList();
		}
		foreach (IGrouping<string, ActionCall> item in from a in list2
			group a by a.Actor?.ToLowerInvariant() ?? "unknown")
		{
			List<ActionCall> actions2 = item.ToList();
			actions2 = TailClear(actions2, item.Key);
			ActionCall actionCall = actions2.FirstOrDefault();
			Pawn pawn = ((actionCall != null) ? PawnResolver.ResolvePawn(actionCall.Actor, map) : null);
			string text = pawn?.LabelShort ?? item.Key;
			if (pawn == null)
			{
				EALogger.Debug($"[DEBUG] Skipping {actions2.Count} actions for {text}: pawn not found");
				foreach (ActionCall item2 in actions2)
				{
					_ = item2;
					list.Add(ExecutionResult.Failed(ErrorCode.ActorNotFound, "Actor not found: " + item.Key));
				}
				continue;
			}
			if (pawn.Dead || pawn.Destroyed)
			{
				EALogger.Debug($"[DEBUG] Skipping {actions2.Count} actions for {text}: pawn is dead/destroyed");
				foreach (ActionCall item3 in actions2)
				{
					_ = item3;
					list.Add(ExecutionResult.Failed(ErrorCode.ActorIncapable, "Actor is dead: " + text));
				}
				continue;
			}
			if (pawn.Downed)
			{
				EALogger.Debug($"[DEBUG] Skipping {actions2.Count} actions for {text}: pawn is downed");
				foreach (ActionCall item4 in actions2)
				{
					_ = item4;
					list.Add(ExecutionResult.Failed(ErrorCode.ActorIncapable, "Actor is downed: " + text));
				}
				continue;
			}
			if (settings.SkipDraftedPawns)
			{
				Pawn_DraftController drafter = pawn.drafter;
				if (drafter != null && drafter.Drafted)
				{
					EALogger.Debug($"[DEBUG] Skipping {actions2.Count} actions for {text}: pawn is drafted (setting enabled)");
					continue;
				}
			}
			if (settings.SkipWorkTimePawns && pawn.timetable?.CurrentAssignment == TimeAssignmentDefOf.Work)
			{
				EALogger.Debug($"[DEBUG] Skipping {actions2.Count} actions for {text}: pawn is on Work schedule (setting enabled)");
				continue;
			}
			EALogger.Debug($"[DEBUG] Processing {actions2.Count} actions for {text}");
			bool flag = true;
			foreach (ActionCall item5 in actions2)
			{
				string text2 = (flag ? "override" : "enqueue");
				EALogger.Debug("[DEBUG] " + text + ": " + item5.Id + " (" + text2 + ")");
				ExecutionResult executionResult = ExecuteSingle(conversationId, item5, map, flag);
				list.Add(executionResult);
				if (executionResult.Success)
				{
					EALogger.Debug("[DEBUG] " + text + ": " + item5.Id + " SUCCESS");
					flag = false;
				}
				else
				{
					EALogger.Debug($"[DEBUG] {text}: {item5.Id} FAILED ({executionResult.ErrorCode}) - continuing to next action");
				}
			}
		}
		return list;
	}

	public static ExecutionResult ExecuteSingle(string conversationId, ActionCall action, Map map, bool isFirstForActor = true)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		try
		{
			if (!IdempotencyTracker.TryMarkExecuted(conversationId, action))
			{
				return ExecutionResult.Failed(ErrorCode.AlreadyExecuted, "Action already executed: " + action.GetSignature());
			}
			string definitionId = RimAICapabilityMigrationRouter.DefinitionId(action.Id);
			ActionDefinition byId = ActionRegistry.GetById(definitionId);
			if (byId != null)
			{
				int targetThingId = 0;
				string semanticTarget = RimTalk.ExpandActions.CapabilityRuntime.SemanticTargetSelector.Select(action.Target, action.Thing);
				if (!string.IsNullOrEmpty(semanticTarget))
				{
					Pawn pawn = PawnResolver.ResolvePawn(semanticTarget, map);
					if (pawn != null)
					{
						targetThingId = pawn.thingIDNumber;
					}
					else
					{
						Pawn pawn2 = PawnResolver.ResolvePawn(action.Actor, map);
						Thing thing = PawnResolver.ResolveThing(semanticTarget, map, pawn2?.Position);
						if (thing != null)
						{
							targetThingId = thing.thingIDNumber;
						}
					}
				}
				Pawn pawn3 = PawnResolver.ResolvePawn(action.Actor, map);
				if (pawn3 != null && CooldownTracker.IsOnCooldown(pawn3.thingIDNumber, action.Id, targetThingId, byId.Category))
				{
					ExecutionResult cooldown = ExecutionResult.Failed(ErrorCode.OnCooldown, "Action on cooldown: " + action.Id);
					cooldown.ActionId = action.Id;
					return cooldown;
				}
			}
			ActionDefinition byId2 = ActionRegistry.GetById(definitionId);
			if (byId2 == null)
			{
				ExecutionResult rejected = ExecutionResult.Failed(ErrorCode.ActionNotInWhitelist, "Unknown action: " + action.Id);
				rejected.ActionId = action.Id;
				return rejected;
			}
			if (!EAModMain.Settings.IsActionEnabled(definitionId))
			{
				return ExecutionResult.Failed(ErrorCode.ActionDisabled, "Action disabled: " + action.Id);
			}
			if (action.Id.StartsWith("job_") && !string.IsNullOrEmpty(action.Job) && !IsJobWhitelisted(action.Job))
			{
				return ExecutionResult.Failed(ErrorCode.JobNotInWhitelist, "Job not whitelisted: " + action.Job);
			}
			ExecutionContext executionContext = BuildContext(conversationId, action, map);
			if (executionContext == null)
			{
				return ExecutionResult.Failed(ErrorCode.ActorNotFound, "Failed to resolve actor: " + action.Actor);
			}
			executionContext.IsFirstActionForActor = isFirstForActor;
			executionContext.ActionPriority = GetActionPriority(action.Id);
			if (!executionContext.CanActorAct())
			{
				return ExecutionResult.Failed(executionContext, ErrorCode.ActorIncapable, "Actor cannot act (dead, downed, or in mental state)");
			}
			if (RimAICapabilityMigrationRouter.TryExecute(executionContext, out var migratedResult))
			{
				return CompleteExecution(stopwatch, executionContext, migratedResult, definitionId);
			}
			if (byId2.Handler == null)
			{
				return ExecutionResult.Failed(executionContext, ErrorCode.ExecutionException, "No handler for action: " + action.Id);
			}
			ExecutionResult executionResult = byId2.Handler.Execute(executionContext);
			return CompleteExecution(stopwatch, executionContext, executionResult, definitionId);
		}
		catch (Exception ex)
		{
			stopwatch.Stop();
			EALogger.Error("Exception executing " + action.Id, ex);
			return new ExecutionResult
			{
				Success = false,
				ActionId = action.Id,
				ErrorCode = ErrorCode.ExecutionException,
				ErrorMessage = ex.Message,
				ExecutionTimeMs = stopwatch.ElapsedMilliseconds
			};
		}
	}

	private static ExecutionResult CompleteExecution(
		Stopwatch stopwatch,
		ExecutionContext context,
		ExecutionResult result,
		string definitionId)
	{
		if (result.Success && context.ResolvedActor != null)
		{
			int targetThingId = context.ResolvedTarget?.thingIDNumber ?? 0;
			CooldownTracker.RecordExecution(context.ResolvedActor.thingIDNumber, definitionId, targetThingId);
		}
		stopwatch.Stop();
		result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
		ShowExecutionBubble(context.ResolvedActor, definitionId, result.Success);
		EALogger.Debug(string.Format(
			"Executed {0}: {1} in {2}ms",
			definitionId,
			result.Success ? "SUCCESS" : "FAILED",
			result.ExecutionTimeMs));
		return result;
	}

	public static int GetActionPriority(string actionId)
	{
		string definitionId = RimAICapabilityMigrationRouter.DefinitionId(actionId);
		ActionDefinition byId = ActionRegistry.GetById(definitionId);
		if (byId != null && byId.Priority.HasValue)
		{
			return byId.Priority.Value;
		}
		if (!ActionPriorityMap.TryGetValue(definitionId, out var value))
		{
			return 3;
		}
		return value;
	}

	public static bool IsJobWhitelisted(string jobDefName)
	{
		if (BaseJobWhitelist.Contains(jobDefName))
		{
			return true;
		}
		List<string> customJobWhitelist = EAModMain.Settings.CustomJobWhitelist;
		if (customJobWhitelist != null && customJobWhitelist.Contains(jobDefName))
		{
			return true;
		}
		if (ModuleRegistry.IsJobInModuleWhitelist(jobDefName))
		{
			return true;
		}
		return false;
	}

	public static HashSet<string> GetFullJobWhitelist()
	{
		HashSet<string> hashSet = new HashSet<string>(BaseJobWhitelist);
		EASettings settings = EAModMain.Settings;
		if (settings.CustomJobWhitelist != null)
		{
			hashSet.UnionWith(settings.CustomJobWhitelist);
		}
		hashSet.UnionWith(ModuleRegistry.GetModuleJobWhitelist());
		return hashSet;
	}

	private static List<ActionCall> TailClear(List<ActionCall> actions, string actorName)
	{
		if (actions.Count <= 1)
		{
			return actions;
		}
		List<ActionCall> list = new List<ActionCall>();
		foreach (ActionCall action in actions)
		{
			int actionPriority = GetActionPriority(action.Id);
			while (list.Count > 0 && GetActionPriority(list[list.Count - 1].Id) < actionPriority)
			{
				list.RemoveAt(list.Count - 1);
			}
			list.Add(action);
		}
		if (list.Count != actions.Count)
		{
			EALogger.Info($"[EA] Tail-clear: {actions.Count} → {list.Count} actions for {actorName}");
		}
		return list;
	}

	private static ExecutionContext BuildContext(string conversationId, ActionCall action, Map map)
	{
		Pawn pawn = PawnResolver.ResolvePawn(action.Actor, map);
		if (pawn == null)
		{
			return null;
		}
		ExecutionContext executionContext = new ExecutionContext
		{
			ConversationId = conversationId,
			ActionCall = action,
			ResolvedActor = pawn,
			Map = map,
			StartTick = (Find.TickManager?.TicksGame ?? 0)
		};
		string semanticTarget = RimTalk.ExpandActions.CapabilityRuntime.SemanticTargetSelector.Select(action.Target, action.Thing);
		if (!string.IsNullOrEmpty(semanticTarget))
		{
			executionContext.ResolvedTarget = PawnResolver.ResolvePawn(semanticTarget, map) ?? PawnResolver.ResolveThing(semanticTarget, map, pawn.Position);
			if (executionContext.ResolvedTarget == null && !executionContext.ResolvedCell.HasValue && !string.IsNullOrEmpty(action.Target))
			{
				executionContext.ResolvedCell = PawnResolver.ResolveRoom(action.Target, map, pawn.Position);
			}
		}
		if (!string.IsNullOrEmpty(action.Cell))
		{
			executionContext.ResolvedCell = PawnResolver.ParseCell(action.Cell);
		}
		return executionContext;
	}

	private static void ShowExecutionBubble(Pawn pawn, string actionId, bool success)
	{
		if (pawn == null || !EAModMain.Settings.ShowExecutionBubbles)
		{
			return;
		}
		try
		{
			string text = (success ? ("✓ " + actionId) : ("✗ " + actionId));
			Color color = (success ? Color.green : Color.red);
			MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, text, color, 2f);
		}
		catch (Exception ex)
		{
			EALogger.Debug("Failed to show execution bubble: " + ex.Message);
		}
	}
}
