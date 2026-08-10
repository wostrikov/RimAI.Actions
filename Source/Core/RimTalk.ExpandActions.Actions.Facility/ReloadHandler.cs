using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Facility;

public class ReloadHandler : IActionHandler
{
	public string ActionId => "reload";

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
		if (!resolvedActor.CanReserveAndReach(resolvedTarget, PathEndMode.Touch, Danger.Some))
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetUnreachable, "Cannot reach target");
		}
		JobDef namedSilentFail = DefDatabase<JobDef>.GetNamedSilentFail("Reload");
		if (namedSilentFail == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.ExecutionException, "Reload JobDef not found");
		}
		Verse.AI.Job job = JobMaker.MakeJob(namedSilentFail, resolvedTarget);
		context.StartOrQueueJob(job);
		EALogger.Debug(resolvedActor.Name.ToStringShort + " reloading " + resolvedTarget.LabelCap);
		return ExecutionResult.Succeeded(context);
	}
}
