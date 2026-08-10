using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Recreation;

public class UseDrugHandler : IActionHandler
{
	public string ActionId => "use_drug";

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
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "Drug not found");
		}
		if (!resolvedTarget.def.IsDrug)
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Target is not a drug");
		}
		if (!resolvedActor.CanReserveAndReach(resolvedTarget, PathEndMode.Touch, Danger.Some))
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetUnreachable, "Cannot reach drug");
		}
		Verse.AI.Job job = JobMaker.MakeJob(JobDefOf.Ingest, resolvedTarget);
		context.StartOrQueueJob(job);
		EALogger.Debug(resolvedActor.Name.ToStringShort + " using " + resolvedTarget.LabelCap);
		return ExecutionResult.Succeeded(context);
	}
}
