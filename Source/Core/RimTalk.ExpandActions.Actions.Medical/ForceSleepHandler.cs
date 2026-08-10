using System.Linq;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Medical;

public class ForceSleepHandler : IActionHandler
{
	public string ActionId => "force_sleep";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn pawn = context.ResolvedActor;
		Map map = context.Map;
		Building_Bed building_Bed = RestUtility.FindBedFor(pawn, pawn, checkSocialProperness: false);
		if (building_Bed == null)
		{
			building_Bed = (from b in map.listerBuildings.AllBuildingsColonistOfClass<Building_Bed>()
				where !b.Medical && pawn.CanReserveAndReach(b, PathEndMode.OnCell, Danger.Some)
				select b).FirstOrDefault();
		}
		if (building_Bed != null)
		{
			Verse.AI.Job job = JobMaker.MakeJob(JobDefOf.LayDown, building_Bed);
			job.forceSleep = true;
			context.StartOrQueueJob(job);
			EALogger.Debug(pawn.Name.ToStringShort + " going to sleep in bed");
		}
		else
		{
			Verse.AI.Job job2 = JobMaker.MakeJob(JobDefOf.LayDown, pawn.Position);
			job2.forceSleep = true;
			context.StartOrQueueJob(job2);
			EALogger.Debug(pawn.Name.ToStringShort + " laying down to sleep on ground");
		}
		return ExecutionResult.Succeeded(context);
	}
}
