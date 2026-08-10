using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Prisoner;

public class CaptureHandler : IActionHandler
{
	public string ActionId => "capture";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn resolvedActor = context.ResolvedActor;
		Pawn pawn = context.ResolvedTarget as Pawn;
		if (!context.CanActorAct())
		{
			return ExecutionResult.Failed(context, ErrorCode.ActorIncapable, "Actor cannot act");
		}
		if (pawn == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "Target pawn not found");
		}
		if (!pawn.Downed)
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Target must be downed to capture");
		}
		if (!resolvedActor.CanReserveAndReach(pawn, PathEndMode.Touch, Danger.Some))
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetUnreachable, "Cannot reach target");
		}
		Verse.AI.Job job = JobMaker.MakeJob(JobDefOf.Capture, pawn);
		context.StartOrQueueJob(job);
		EALogger.Debug(resolvedActor.Name.ToStringShort + " capturing " + pawn.Name.ToStringShort);
		return ExecutionResult.Succeeded(context);
	}
}
