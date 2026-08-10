using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Prisoner;

public class RecruitPrisonerHandler : IActionHandler
{
	public string ActionId => "recruit_prisoner";

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
		if (!pawn.IsPrisonerOfColony)
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Target is not a prisoner");
		}
		if (!resolvedActor.CanReserveAndReach(pawn, PathEndMode.Touch, Danger.Some))
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetUnreachable, "Cannot reach prisoner");
		}
		Verse.AI.Job job = JobMaker.MakeJob(JobDefOf.PrisonerAttemptRecruit, pawn);
		context.StartOrQueueJob(job);
		EALogger.Debug(resolvedActor.Name.ToStringShort + " recruiting " + pawn.Name.ToStringShort);
		return ExecutionResult.Succeeded(context);
	}
}
