using System.Linq;
using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Medical;

public class RescueHandler : IActionHandler
{
	public string ActionId => "rescue";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn pawn = context.ResolvedActor;
		Map map = context.Map;
		Pawn pawn2 = context.ResolvedTarget as Pawn;
		if (pawn2 == null)
		{
			pawn2 = (from p in map.mapPawns.AllPawnsSpawned
				where p.Downed && !p.Dead && p.Faction == pawn.Faction && pawn.CanReserveAndReach(p, PathEndMode.Touch, Danger.Some)
				orderby p.Position.DistanceTo(pawn.Position)
				select p).FirstOrDefault();
		}
		if (pawn2 == null || !pawn2.Downed)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "No downed pawn to rescue");
		}
		Building_Bed building_Bed = RestUtility.FindBedFor(pawn2, pawn, checkSocialProperness: false);
		if (building_Bed == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "No bed available for rescue");
		}
		Verse.AI.Job job = JobMaker.MakeJob(JobDefOf.Rescue, pawn2, building_Bed);
		job.count = 1;
		context.StartOrQueueJob(job);
		EALogger.Debug(pawn.Name.ToStringShort + " rescuing " + pawn2.Name.ToStringShort);
		return ExecutionResult.Succeeded(context);
	}
}
