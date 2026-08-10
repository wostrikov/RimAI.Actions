using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Combat;

public class AttackRangedHandler : IActionHandler
{
	public string ActionId => "attack_ranged";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn resolvedActor = context.ResolvedActor;
		Thing resolvedTarget = context.ResolvedTarget;
		if (resolvedTarget == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "No target for ranged attack");
		}
		ThingWithComps thingWithComps = resolvedActor.equipment?.Primary;
		if (thingWithComps == null || !thingWithComps.def.IsRangedWeapon)
		{
			return ExecutionResult.Failed(context, ErrorCode.ActorIncapable, "Pawn has no ranged weapon");
		}
		if (resolvedActor.drafter != null && !resolvedActor.Drafted)
		{
			resolvedActor.drafter.Drafted = true;
		}
		Verse.AI.Job job = JobMaker.MakeJob(JobDefOf.AttackStatic, resolvedTarget);
		context.StartOrQueueJob(job);
		EALogger.Debug($"{resolvedActor.Name.ToStringShort} attacking {resolvedTarget} with ranged weapon");
		return ExecutionResult.Succeeded(context);
	}
}
