using System;
using System.Linq;
using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Item;

public class TakeInventoryHandler : IActionHandler
{
	public string ActionId => "take_inventory";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn actor = context.ResolvedActor;
		string thingName = context.ActionCall.Thing ?? context.ActionCall.Target;
		if (string.IsNullOrWhiteSpace(thingName))
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Item name required (thing or target)");
		}

		var items = ThingMatcher.FindMatching(context.Map.listerThings.AllThings, thingName)
			.Where(t => t.Spawned && t.def.EverHaulable)
			.OrderBy(t => t.Position.DistanceToSquared(actor.Position))
			.Where(t => actor.CanReserveAndReach(t, PathEndMode.Touch, Danger.Some))
			.ToList();
		if (items.Count == 0)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "No reachable matching item found: " + thingName);
		}

		int requested = Math.Max(1, context.ActionCall.GetArg("quantity", 1));
		int queued = 0;
		foreach (Thing item in items)
		{
			int count = Math.Min(requested - queued, item.stackCount);
			if (count <= 0)
			{
				break;
			}
			Verse.AI.Job job = JobMaker.MakeJob(RimWorld.JobDefOf.TakeInventory, item);
			job.count = count;
			if (!context.TryStartOrQueueJob(job, out string failure))
			{
				if (queued == 0)
				{
					return ExecutionResult.Failed(context, ErrorCode.JobNotQueued, failure);
				}
				break;
			}
			queued += count;
		}
		string description = $"Queued {queued}/{requested} {thingName}";
		return queued >= requested
			? ExecutionResult.Queued(context, description)
			: ExecutionResult.Partial(context, description);
	}
}
