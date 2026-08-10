using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Social;

public class SpectateHandler : IActionHandler
{
	public string ActionId => "spectate";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn resolvedActor = context.ResolvedActor;
		Thing resolvedTarget = context.ResolvedTarget;
		if (!context.CanActorAct())
		{
			return ExecutionResult.Failed(context, ErrorCode.ActorIncapable, "Actor cannot act");
		}
		if (resolvedTarget == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "Target not found");
		}
		if (!resolvedActor.CanReach(resolvedTarget, PathEndMode.Touch, Danger.None))
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetUnreachable, "Cannot reach target");
		}
		Verse.AI.Job job = JobMaker.MakeJob(JobDefOf.Goto, resolvedTarget.Position);
		job.locomotionUrgency = LocomotionUrgency.Walk;
		context.StartOrQueueJob(job);
		EALogger.Debug(resolvedActor.Name.ToStringShort + " going to watch " + resolvedTarget.LabelCap);
		return ExecutionResult.Succeeded(context);
	}
}
