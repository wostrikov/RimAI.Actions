using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Production;

public class SowHandler : IActionHandler
{
	public string ActionId => "sow";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn resolvedActor = context.ResolvedActor;
		Thing resolvedTarget = context.ResolvedTarget;
		if (!context.CanActorAct())
		{
			return ExecutionResult.Failed(context, ErrorCode.ActorIncapable, "Actor cannot act");
		}
		IntVec3? intVec = context.ResolvedCell ?? resolvedTarget?.Position;
		if (!intVec.HasValue)
		{
			intVec = NearbyTargetFinder.FindSowableCell(resolvedActor);
		}
		if (!intVec.HasValue)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "No sowable location found nearby");
		}
		if (!resolvedActor.CanReach(intVec.Value, PathEndMode.Touch, Danger.Some))
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetUnreachable, "Cannot reach location");
		}
		Verse.AI.Job job = JobMaker.MakeJob(JobDefOf.Sow, intVec.Value);
		context.StartOrQueueJob(job);
		EALogger.Debug(resolvedActor.Name.ToStringShort + " sowing");
		return ExecutionResult.Succeeded(context);
	}
}
