using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Social;

public class LovinHandler : IActionHandler
{
	public string ActionId => "lovin";

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
		if (!LovePartnerRelationUtility.LovePartnerRelationExists(resolvedActor, pawn))
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Must be in a romantic relationship");
		}
		Building_Bed building_Bed = resolvedActor.CurrentBed() ?? pawn.CurrentBed();
		if (building_Bed == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Neither partner is in bed");
		}
		Verse.AI.Job job = JobMaker.MakeJob(JobDefOf.Lovin, pawn, building_Bed);
		context.StartOrQueueJob(job);
		EALogger.Debug(resolvedActor.Name.ToStringShort + " lovin' with " + pawn.Name.ToStringShort);
		return ExecutionResult.Succeeded(context);
	}
}
