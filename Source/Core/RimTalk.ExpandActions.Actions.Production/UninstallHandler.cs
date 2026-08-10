using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Production;

public class UninstallHandler : IActionHandler
{
	public string ActionId => "uninstall";

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
		if (!(resolvedTarget is Building))
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Target is not a building");
		}
		if (!resolvedTarget.def.Minifiable)
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Target cannot be uninstalled");
		}
		if (!resolvedActor.CanReserveAndReach(resolvedTarget, PathEndMode.Touch, Danger.Some))
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetUnreachable, "Cannot reach building");
		}
		Verse.AI.Job job = JobMaker.MakeJob(JobDefOf.Uninstall, resolvedTarget);
		context.StartOrQueueJob(job);
		EALogger.Debug(resolvedActor.Name.ToStringShort + " uninstalling " + resolvedTarget.LabelCap);
		return ExecutionResult.Succeeded(context);
	}
}
