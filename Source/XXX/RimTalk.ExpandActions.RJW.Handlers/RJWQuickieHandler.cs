using RimTalk.ExpandActions.Actions;
using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.RJW.Util;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.RJW.Handlers;

public class RJWQuickieHandler : IActionHandler
{
	public string ActionId => "rjw_quickie";

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
		if (!RJWReflectionCache.CanFuck(resolvedActor))
		{
			return ExecutionResult.Failed(context, ErrorCode.ActorIncapable, "Actor cannot perform this action");
		}
		if (!RJWReflectionCache.CanBeFucked(targetPawn))
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Target cannot receive this action");
		}
		if (resolvedActor.Position.DistanceTo(targetPawn.Position) > 15f)
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Target too far for quickie");
		}
		JobDef quickieDef = RJWReflectionCache.GetQuickieDef();
		if (quickieDef == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.ExecutionException, "Quickie JobDef not found");
		}
		Job job = JobMaker.MakeJob(quickieDef, targetPawn);
		context.StartOrQueueJob(job);
		return ExecutionResult.Succeeded(context);
	}
}
