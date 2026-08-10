using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Production;

public class FlickHandler : IActionHandler
{
	public string ActionId => "flick";

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
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "Target not found");
		}
		if (resolvedTarget.TryGetComp<CompFlickable>() == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Target is not flickable");
		}
		if (!resolvedActor.CanReserveAndReach(resolvedTarget, PathEndMode.Touch, Danger.Some))
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetUnreachable, "Cannot reach target");
		}
		FlickUtility.UpdateFlickDesignation(resolvedTarget);
		JobDef namedSilentFail = DefDatabase<JobDef>.GetNamedSilentFail("Flick");
		if (namedSilentFail == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.ExecutionException, "Flick JobDef not found");
		}
		Verse.AI.Job job = JobMaker.MakeJob(namedSilentFail, resolvedTarget);
		context.StartOrQueueJob(job);
		EALogger.Debug(resolvedActor.Name.ToStringShort + " flicking " + resolvedTarget.LabelCap);
		return ExecutionResult.Succeeded(context);
	}
}
