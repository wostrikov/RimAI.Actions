using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Production;

public class CutPlantHandler : IActionHandler
{
	public string ActionId => "cut_plant";

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
			thing = NearbyTargetFinder.FindCuttablePlant(resolvedActor);
		}
		if (thing == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "No cuttable plant found nearby");
		}
		if (!(thing is Plant))
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Target is not a plant");
		}
		if (!resolvedActor.CanReserveAndReach(thing, PathEndMode.Touch, Danger.Some))
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetUnreachable, "Cannot reach plant");
		}
		Verse.AI.Job job = JobMaker.MakeJob(JobDefOf.CutPlant, thing);
		context.StartOrQueueJob(job);
		EALogger.Debug(resolvedActor.Name.ToStringShort + " cutting " + thing.LabelCap);
		return ExecutionResult.Succeeded(context);
	}
}
