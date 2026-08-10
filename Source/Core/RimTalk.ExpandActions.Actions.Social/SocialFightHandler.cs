using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Social;

public class SocialFightHandler : IActionHandler
{
	public string ActionId => "social_fight";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn resolvedActor = context.ResolvedActor;
		Pawn pawn = context.ResolvedTarget as Pawn;
		if (!context.CanActorAct())
		{
			return ExecutionResult.Failed(context, ErrorCode.ActorIncapable, "Actor cannot act");
		}
		if (pawn == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "Target pawn not found");
		}
		if (pawn.Dead || pawn.Downed)
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Target is dead or downed");
		}
		Verse.AI.Job job = JobMaker.MakeJob(JobDefOf.SocialFight, pawn);
		context.StartOrQueueJob(job);
		EALogger.Debug(resolvedActor.Name.ToStringShort + " starting social fight with " + pawn.Name.ToStringShort);
		return ExecutionResult.Succeeded(context);
	}
}
