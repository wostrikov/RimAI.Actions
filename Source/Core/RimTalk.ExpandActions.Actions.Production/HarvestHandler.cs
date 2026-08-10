using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Production;

public class HarvestHandler : IActionHandler
{
	public string ActionId => "harvest";

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
			thing = NearbyTargetFinder.FindHarvestable(resolvedActor);
		}
		if (thing == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "No harvestable plant found nearby");
		}
		if (!(thing is Plant plant))
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Target is not a plant");
		}
		if (!plant.HarvestableNow)
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Plant is not ready for harvest");
		}
		if (!resolvedActor.CanReserveAndReach(thing, PathEndMode.Touch, Danger.Some))
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetUnreachable, "Cannot reach plant");
		}
		Verse.AI.Job job = JobMaker.MakeJob(JobDefOf.Harvest, thing);
		context.StartOrQueueJob(job);
		EALogger.Debug(resolvedActor.Name.ToStringShort + " harvesting " + thing.LabelCap);
		return ExecutionResult.Succeeded(context);
	}
}
