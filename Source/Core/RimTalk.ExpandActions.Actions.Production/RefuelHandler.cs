using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Production;

public class RefuelHandler : IActionHandler
{
	public string ActionId => "refuel";

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
			thing = NearbyTargetFinder.FindRefuelable(resolvedActor);
		}
		if (thing == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "No refuelable thing found nearby");
		}
		if (thing.TryGetComp<CompRefuelable>() == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Target is not refuelable");
		}
		if (!resolvedActor.CanReserveAndReach(thing, PathEndMode.Touch, Danger.Some))
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetUnreachable, "Cannot reach target");
		}
		Verse.AI.Job job = RefuelWorkGiverUtility.RefuelJob(resolvedActor, thing);
		if (job == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.ExecutionException, "Cannot create refuel job (no fuel available?)");
		}
		context.StartOrQueueJob(job);
		EALogger.Debug(resolvedActor.Name.ToStringShort + " refueling " + thing.LabelCap);
		return ExecutionResult.Succeeded(context);
	}
}
