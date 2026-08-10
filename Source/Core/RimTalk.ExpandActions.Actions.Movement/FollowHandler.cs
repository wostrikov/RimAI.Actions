using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Movement;

public class FollowHandler : IActionHandler
{
	private const int DefaultFollowDurationTicks = 7500;

	public string ActionId => "follow";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn resolvedActor = context.ResolvedActor;
		if (!(context.ResolvedTarget is Pawn pawn))
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "No target pawn to follow");
		}
		if (pawn == resolvedActor)
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Cannot follow self");
		}
		int num = context.ActionCall.GetArg("duration", 7500);
		if (num <= 0)
		{
			num = 7500;
		}
		Verse.AI.Job job = JobMaker.MakeJob(JobDefOf.FollowClose, pawn);
		job.followRadius = 3f;
		job.expiryInterval = num;
		job.checkOverrideOnExpire = true;
		context.StartOrQueueJob(job);
		float num2 = (float)num / 2500f;
		EALogger.Debug($"{resolvedActor.Name.ToStringShort} following {pawn.Name.ToStringShort} (max {num2:F1} hours)");
		return ExecutionResult.Succeeded(context);
	}
}
