using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.DLC;

public class SuppressEntityHandler : IActionHandler
{
	public string ActionId => "suppress_entity";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn resolvedActor = context.ResolvedActor;
		Thing resolvedTarget = context.ResolvedTarget;
		if (!context.CanActorAct())
		{
			return ExecutionResult.Failed(context, ErrorCode.ActorIncapable, "Actor cannot act");
		}
		if (resolvedTarget == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "Entity not found");
		}
		if (!resolvedActor.CanReserveAndReach(resolvedTarget, PathEndMode.Touch, Danger.Some))
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetUnreachable, "Cannot reach entity");
		}
		JobDef namedSilentFail = DefDatabase<JobDef>.GetNamedSilentFail("Suppress");
		if (namedSilentFail == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.ExecutionException, "Suppress JobDef not found");
		}
		Verse.AI.Job job = JobMaker.MakeJob(namedSilentFail, resolvedTarget);
		context.StartOrQueueJob(job);
		EALogger.Debug(resolvedActor.Name.ToStringShort + " suppressing entity " + resolvedTarget.LabelCap);
		return ExecutionResult.Succeeded(context);
	}
}
