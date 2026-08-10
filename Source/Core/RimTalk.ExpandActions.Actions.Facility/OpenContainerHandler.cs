using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Facility;

public class OpenContainerHandler : IActionHandler
{
	public string ActionId => "open_container";

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
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "Container not found");
		}
		if (!resolvedActor.CanReserveAndReach(resolvedTarget, PathEndMode.Touch, Danger.Some))
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetUnreachable, "Cannot reach container");
		}
		Verse.AI.Job job = JobMaker.MakeJob(JobDefOf.Open, resolvedTarget);
		context.StartOrQueueJob(job);
		EALogger.Debug(resolvedActor.Name.ToStringShort + " opening " + resolvedTarget.LabelCap);
		return ExecutionResult.Succeeded(context);
	}
}
