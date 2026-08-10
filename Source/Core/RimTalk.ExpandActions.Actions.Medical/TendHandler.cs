using System.Linq;
using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Medical;

public class TendHandler : IActionHandler
{
	public string ActionId => "tend";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn pawn = context.ResolvedActor;
		Map map = context.Map;
		Pawn pawn2 = context.ResolvedTarget as Pawn;
		if (pawn2 == null)
		{
			pawn2 = (from p in map.mapPawns.AllPawnsSpawned
				where p.Faction == pawn.Faction && p.health.HasHediffsNeedingTend() && pawn.CanReserveAndReach(p, PathEndMode.Touch, Danger.Some)
				orderby p.Position.DistanceTo(pawn.Position)
				select p).FirstOrDefault();
		}
		if (pawn2 == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "No pawn needs tending");
		}
		if (!pawn2.health.HasHediffsNeedingTend())
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Target has no injuries to tend");
		}
		Verse.AI.Job job = JobMaker.MakeJob(JobDefOf.TendPatient, pawn2);
		context.StartOrQueueJob(job);
		EALogger.Debug(pawn.Name.ToStringShort + " tending " + pawn2.Name.ToStringShort);
		return ExecutionResult.Succeeded(context);
	}
}
