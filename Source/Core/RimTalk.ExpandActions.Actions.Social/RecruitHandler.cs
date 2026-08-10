using System.Linq;
using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Social;

public class RecruitHandler : IActionHandler
{
	public string ActionId => "recruit";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn pawn = context.ResolvedActor;
		Map map = context.Map;
		Pawn pawn2 = context.ResolvedTarget as Pawn;
		if (pawn2 == null)
		{
			pawn2 = map.mapPawns.PrisonersOfColonySpawned.Where((Pawn p) => !p.Dead && !p.Downed && pawn.CanReserveAndReach(p, PathEndMode.Touch, Danger.Some)).FirstOrDefault();
		}
		if (pawn2 == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "No prisoner to recruit");
		}
		if (!pawn2.IsPrisonerOfColony)
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Target is not a prisoner");
		}
		Verse.AI.Job job = JobMaker.MakeJob(JobDefOf.PrisonerAttemptRecruit, pawn2);
		context.StartOrQueueJob(job);
		EALogger.Debug(pawn.Name.ToStringShort + " attempting to recruit " + pawn2.Name.ToStringShort);
		return ExecutionResult.Succeeded(context);
	}
}
