using System.Linq;
using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Item;

public class GiveItemHandler : IActionHandler
{
	public string ActionId => "give_item";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn resolvedActor = context.ResolvedActor;
		Map map = context.Map;
		if (!(context.ResolvedTarget is Pawn pawn))
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "Target pawn required for give_item");
		}
		Thing thing = null;
		if (resolvedActor.carryTracker?.CarriedThing != null)
		{
			thing = resolvedActor.carryTracker.CarriedThing;
		}
		else
		{
			Pawn_InventoryTracker inventory = resolvedActor.inventory;
			if (inventory != null && inventory.innerContainer?.Count > 0)
			{
				if (!string.IsNullOrEmpty(context.ActionCall.Thing))
				{
					EALogger.Debug("[give_item] " + ThingMatcher.GetSearchDebugInfo(context.ActionCall.Thing));
					thing = ThingMatcher.FindMatching(resolvedActor.inventory.innerContainer, context.ActionCall.Thing).FirstOrDefault();
					if (thing != null)
					{
						EALogger.Debug("[give_item] Found matching item in inventory: " + thing.Label);
					}
				}
				else
				{
					thing = resolvedActor.inventory.innerContainer.FirstOrDefault();
				}
			}
		}
		if (thing == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "No item to give. Pawn must be carrying or have item in inventory");
		}
		if (!resolvedActor.CanReserveAndReach(pawn, PathEndMode.Touch, Danger.Some))
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Cannot reach target pawn");
		}
		Verse.AI.Job job = JobMaker.MakeJob(JobDefOf.GiveToPackAnimal, thing, pawn);
		if (job == null)
		{
			resolvedActor.carryTracker?.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out var resultingThing);
			resolvedActor.inventory?.innerContainer?.TryDrop(thing, pawn.Position, map, ThingPlaceMode.Near, out resultingThing);
			EALogger.Debug(resolvedActor.Name.ToStringShort + " dropped " + thing.Label + " near " + pawn.Name.ToStringShort);
			return ExecutionResult.Succeeded(context);
		}
		context.StartOrQueueJob(job);
		EALogger.Debug(resolvedActor.Name.ToStringShort + " giving " + thing.Label + " to " + pawn.Name.ToStringShort);
		return ExecutionResult.Succeeded(context);
	}
}
