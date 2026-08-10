using System.Linq;
using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Combat;

public class ArrestHandler : IActionHandler
{
	public string ActionId => "arrest";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn pawn = context.ResolvedActor;
		Map map = context.Map;
		Pawn pawn2 = context.ResolvedTarget as Pawn;
		if (pawn2 == null)
		{
			pawn2 = (from p in map.mapPawns.AllPawnsSpawned
				where !p.Dead && !p.Downed && p.HostileTo(pawn.Faction) && pawn.CanReserveAndReach(p, PathEndMode.Touch, Danger.Some)
				orderby p.Position.DistanceTo(pawn.Position)
				select p).FirstOrDefault();
		}
		if (pawn2 == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "No target to arrest");
		}
		if (pawn2.Faction == pawn.Faction && !pawn2.IsPrisonerOfColony)
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Cannot arrest own faction member");
		}
		if (!pawn.Drafted)
		{
			pawn.drafter.Drafted = true;
		}
		Verse.AI.Job job = JobMaker.MakeJob(JobDefOf.Arrest, pawn2);
		context.StartOrQueueJob(job);
		EALogger.Debug(pawn.Name.ToStringShort + " arresting " + pawn2.Name.ToStringShort);
		return ExecutionResult.Succeeded(context);
	}
}
