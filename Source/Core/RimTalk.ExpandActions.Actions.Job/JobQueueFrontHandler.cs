using System;
using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Job;

public class JobQueueFrontHandler : IActionHandler
{
	public string ActionId => "job_queue_front";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn resolvedActor = context.ResolvedActor;
		string job = context.ActionCall.Job;
		if (string.IsNullOrEmpty(job))
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Job name required");
		}
		JobDef namedSilentFail = DefDatabase<JobDef>.GetNamedSilentFail(job);
		if (namedSilentFail == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "JobDef '" + job + "' not found");
		}
		LocalTargetInfo targetA = LocalTargetInfo.Invalid;
		if (context.ResolvedTarget != null)
		{
			Thing resolvedTarget = context.ResolvedTarget;
			if (resolvedTarget != null)
			{
				targetA = resolvedTarget;
			}
			else if (context.ResolvedTarget is Pawn pawn)
			{
				targetA = pawn;
			}
		}
		LocalTargetInfo localTargetInfo = LocalTargetInfo.Invalid;
		if (!string.IsNullOrEmpty(context.ActionCall.Cell))
		{
			string[] array = context.ActionCall.Cell.Split(new[] { ',' }, StringSplitOptions.None);
			if (array.Length == 2 && int.TryParse(array[0], out var result) && int.TryParse(array[1], out var result2))
			{
				localTargetInfo = new IntVec3(result, 0, result2);
			}
		}
		Verse.AI.Job j = ((targetA.IsValid && localTargetInfo.IsValid) ? JobMaker.MakeJob(namedSilentFail, targetA, localTargetInfo) : (targetA.IsValid ? JobMaker.MakeJob(namedSilentFail, targetA) : ((!localTargetInfo.IsValid) ? JobMaker.MakeJob(namedSilentFail) : JobMaker.MakeJob(namedSilentFail, localTargetInfo))));
		resolvedActor.jobs.jobQueue.EnqueueFirst(j);
		EALogger.Debug(resolvedActor.Name.ToStringShort + " queued job " + job + " at front");
		return ExecutionResult.Succeeded(context);
	}
}
