using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Recreation;

public class GoForWalkHandler : IActionHandler
{
	public string ActionId => "go_for_walk";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn resolvedActor = context.ResolvedActor;
		if (!context.CanActorAct())
		{
			return ExecutionResult.Failed(context, ErrorCode.ActorIncapable, "Actor cannot act");
		}
		IntVec3 intVec = RCellFinder.RandomWanderDestFor(resolvedActor, resolvedActor.Position, 12f, null, Danger.None);
		if (!intVec.IsValid)
		{
			return ExecutionResult.Failed(context, ErrorCode.ExecutionException, "No valid wander destination found");
		}
		Verse.AI.Job job = JobMaker.MakeJob(JobDefOf.GotoWander, intVec);
		job.locomotionUrgency = LocomotionUrgency.Amble;
		context.StartOrQueueJob(job);
		EALogger.Debug($"{resolvedActor.Name.ToStringShort} going for a walk to {intVec}");
		return ExecutionResult.Succeeded(context);
	}
}
