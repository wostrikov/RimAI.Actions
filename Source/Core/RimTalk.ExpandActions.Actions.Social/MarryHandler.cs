using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Social;

public class MarryHandler : IActionHandler
{
	public string ActionId => "marry";

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
		if (pawn.Dead || pawn.Destroyed)
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Target is dead");
		}
		if (!LovePartnerRelationUtility.LovePartnerRelationExists(resolvedActor, pawn))
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Must be in a romantic relationship first");
		}
		Verse.AI.Job job = JobMaker.MakeJob(JobDefOf.MarryAdjacentPawn, pawn);
		context.StartOrQueueJob(job);
		EALogger.Debug(resolvedActor.Name.ToStringShort + " marrying " + pawn.Name.ToStringShort);
		return ExecutionResult.Succeeded(context);
	}
}
