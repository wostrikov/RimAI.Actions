using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Production;

public class RepairHandler : IActionHandler
{
	public string ActionId => "repair";

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
			thing = NearbyTargetFinder.FindRepairable(resolvedActor);
		}
		if (thing == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "No repairable building found nearby");
		}
		if (!(thing is Building building))
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Target is not a building");
		}
		if (building.HitPoints >= building.MaxHitPoints)
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Building is not damaged");
		}
		if (!resolvedActor.CanReserveAndReach(thing, PathEndMode.Touch, Danger.Some))
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetUnreachable, "Cannot reach building");
		}
		Verse.AI.Job job = JobMaker.MakeJob(JobDefOf.Repair, thing);
		context.StartOrQueueJob(job);
		EALogger.Debug(resolvedActor.Name.ToStringShort + " repairing " + thing.LabelCap);
		return ExecutionResult.Succeeded(context);
	}
}
