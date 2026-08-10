using RimTalk.ExpandActions.Actions;
using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.RJW.Util;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.RJW.Handlers;

public class RJWBondageGiveHandler : IActionHandler
{
	public string ActionId => "rjw_bondage_give";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn resolvedActor = context.ResolvedActor;
		Pawn targetPawn = context.TargetPawn;
		if (targetPawn == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "Target pawn not found");
		}
		if (targetPawn.Dead || targetPawn.Downed)
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Target is dead or downed");
		}
		JobDef giveBondageGearDef = RJWReflectionCache.GetGiveBondageGearDef();
		if (giveBondageGearDef == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.ExecutionException, "GiveBondageGear JobDef not found");
		}
		Thing resolvedTarget = context.ResolvedTarget;
		if (resolvedTarget == null || resolvedTarget is Pawn)
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "No bondage gear specified");
		}
		Job job = JobMaker.MakeJob(giveBondageGearDef, targetPawn, resolvedTarget);
		context.StartOrQueueJob(job);
		return ExecutionResult.Succeeded(context);
	}
}
