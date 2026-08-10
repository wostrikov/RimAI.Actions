using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Production;

public class HaulHandler : IActionHandler
{
	public string ActionId => "haul";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn resolvedActor = context.ResolvedActor;
		Thing thing = context.ResolvedTarget;
		if (!context.CanActorAct())
		{
			return ExecutionResult.Failed(context, ErrorCode.ActorIncapable, "Actor cannot act");
		}
		if (thing == null)
		{
			thing = NearbyTargetFinder.FindHaulable(resolvedActor);
		}
		if (thing == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "No haulable item found nearby");
		}
		if (!resolvedActor.CanReserveAndReach(thing, PathEndMode.Touch, Danger.Some))
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetUnreachable, "Cannot reach target");
		}
		Verse.AI.Job job = HaulAIUtility.HaulToStorageJob(resolvedActor, thing, forced: false);
		if (job == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.ExecutionException, "No valid storage location found");
		}
		context.StartOrQueueJob(job);
		EALogger.Debug(resolvedActor.Name.ToStringShort + " hauling " + thing.LabelCap);
		return ExecutionResult.Succeeded(context);
	}
}
