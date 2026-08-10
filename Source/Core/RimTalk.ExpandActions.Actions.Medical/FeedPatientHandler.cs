using System.Linq;
using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Medical;

public class FeedPatientHandler : IActionHandler
{
	public string ActionId => "feed_patient";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn pawn = context.ResolvedActor;
		Map map = context.Map;
		Pawn pawn2 = context.ResolvedTarget as Pawn;
		if (pawn2 == null)
		{
			pawn2 = (from p in map.mapPawns.FreeColonistsAndPrisonersSpawned
				where p != pawn && !p.Dead && p.InBed() && p.needs?.food != null && p.needs.food.CurLevelPercentage < 0.4f && pawn.CanReserveAndReach(p, PathEndMode.Touch, Danger.Some)
				orderby p.needs.food.CurLevelPercentage
				select p).FirstOrDefault();
		}
		if (pawn2 == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "No patient needs feeding");
		}
		if (!pawn2.InBed())
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Target is not in bed");
		}
		ThingDef foodDef;
		Thing thing = FoodUtility.BestFoodSourceOnMap(pawn, pawn2, desperate: false, out foodDef, FoodPreferability.MealLavish, allowPlant: false, allowDrug: false, allowCorpse: false, allowDispenserFull: true, allowDispenserEmpty: false);
		if (thing == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "No suitable food found");
		}
		Verse.AI.Job job = JobMaker.MakeJob(JobDefOf.FeedPatient, thing, pawn2);
		job.count = FoodUtility.WillIngestStackCountOf(pawn2, thing.def, thing.def.IsNutritionGivingIngestible ? thing.def.ingestible.CachedNutrition : 0.05f);
		context.StartOrQueueJob(job);
		EALogger.Debug(pawn.Name.ToStringShort + " feeding " + pawn2.Name.ToStringShort);
		return ExecutionResult.Succeeded(context);
	}
}
