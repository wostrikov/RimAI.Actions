using Ustas.RimAI.Actions.Actions;
using Ustas.RimAI.Actions.Core;
using Ustas.RimAI.Actions.Execution;
using Ustas.RimAI.Actions.RJW.Util;
using Verse;
using Verse.AI;

namespace Ustas.RimAI.Actions.RJW.Handlers.RJWHighRisk;

public class RJWRapeHandler : IActionHandler
{
	public string ActionId => "rjw_rape";

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
		if (!RJWReflectionCache.CanFuck(resolvedActor))
		{
			return ExecutionResult.Failed(context, ErrorCode.ActorIncapable, "Actor cannot perform this action");
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
