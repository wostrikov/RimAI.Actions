using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Social;

public class VisitSickHandler : IActionHandler
{
	public string ActionId => "visit_sick";

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
		if (!pawn.InBed())
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Target is not in bed");
		}
		if (!resolvedActor.CanReserveAndReach(pawn, PathEndMode.Touch, Danger.None))
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetUnreachable, "Cannot reach target");
		}
		Verse.AI.Job job = JobMaker.MakeJob(JobDefOf.VisitSickPawn, pawn, pawn.CurrentBed());
		context.StartOrQueueJob(job);
		EALogger.Debug(resolvedActor.Name.ToStringShort + " visiting sick " + pawn.Name.ToStringShort);
		return ExecutionResult.Succeeded(context);
	}
}
