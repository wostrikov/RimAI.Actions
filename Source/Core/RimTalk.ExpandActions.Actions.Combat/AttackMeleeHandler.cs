using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Combat;

public class AttackMeleeHandler : IActionHandler
{
	public string ActionId => "attack_melee";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn resolvedActor = context.ResolvedActor;
		Pawn targetPawn = context.TargetPawn;
		if (targetPawn == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "No valid target pawn");
		}
		if (targetPawn.Dead)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "Target is dead");
		}
		if (!resolvedActor.CanReach(targetPawn, PathEndMode.Touch, Danger.Deadly))
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetUnreachable, "Cannot reach target");
		}
		Verse.AI.Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, targetPawn);
		job.killIncappedTarget = false;
		context.StartOrQueueJob(job);
		EALogger.Debug(resolvedActor.Name.ToStringShort + " attacking " + targetPawn.Name.ToStringShort + " (melee)");
		return ExecutionResult.Succeeded(context);
	}
}
