using RimTalk.ExpandActions.Actions;
using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.RJW.Util;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.RJW.Handlers;

public class RJWBondageUnlockHandler : IActionHandler
{
	public string ActionId => "rjw_bondage_unlock";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn resolvedActor = context.ResolvedActor;
		Pawn targetPawn = context.TargetPawn;
		if (targetPawn == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "Target pawn not found");
		}
		if (targetPawn.Dead)
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Target is dead");
		}
		JobDef unlockBondageGearDef = RJWReflectionCache.GetUnlockBondageGearDef();
		if (unlockBondageGearDef == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.ExecutionException, "UnlockBondageGear JobDef not found");
		}
		Job job = JobMaker.MakeJob(unlockBondageGearDef, targetPawn);
		context.StartOrQueueJob(job);
		return ExecutionResult.Succeeded(context);
	}
}
