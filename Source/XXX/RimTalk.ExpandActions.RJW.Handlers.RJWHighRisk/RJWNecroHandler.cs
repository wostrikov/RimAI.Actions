using RimTalk.ExpandActions.Actions;
using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.RJW.Util;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.RJW.Handlers.RJWHighRisk;

public class RJWNecroHandler : IActionHandler
{
	public string ActionId => "rjw_necro";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn resolvedActor = context.ResolvedActor;
		Thing resolvedTarget = context.ResolvedTarget;
		if (resolvedTarget == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "Target not found");
		}
		if (resolvedTarget is Pawn { Dead: false })
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Target must be dead for this action");
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
		Job job = JobMaker.MakeJob(quickieDef, resolvedTarget);
		context.StartOrQueueJob(job);
		return ExecutionResult.Succeeded(context);
	}
}
