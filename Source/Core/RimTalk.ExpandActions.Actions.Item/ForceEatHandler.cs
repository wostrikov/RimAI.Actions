using System.Linq;
using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Item;

public class ForceEatHandler : IActionHandler
{
	public string ActionId => "force_eat";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn pawn = context.ResolvedActor;
		Map map = context.Map;
		Thing thing = null;
		if (!string.IsNullOrEmpty(context.ActionCall.Thing))
		{
			EALogger.Debug("[force_eat] " + ThingMatcher.GetSearchDebugInfo(context.ActionCall.Thing));
			thing = ThingMatcher.FindMatching(map.listerThings.AllThings.Where((Thing t) => t.def.IsIngestible && pawn.CanReserveAndReach(t, PathEndMode.Touch, Danger.Some)), context.ActionCall.Thing).FirstOrDefault();
			if (thing != null)
			{
				EALogger.Debug("[force_eat] Found matching food: " + thing.Label);
			}
		}
		if (thing == null)
		{
			thing = FoodUtility.BestFoodSourceOnMap(pawn, pawn, desperate: false, out var _, FoodPreferability.MealLavish, allowPlant: true, allowDrug: false, allowCorpse: false, allowDispenserFull: true, allowDispenserEmpty: false);
		}
		if (thing == null && pawn.inventory?.innerContainer != null)
		{
			thing = pawn.inventory.innerContainer.Where((Thing t) => t.def.IsIngestible && t.def.ingestible != null).FirstOrDefault();
		}
		if (thing == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "No food found");
		}
		Verse.AI.Job job = JobMaker.MakeJob(JobDefOf.Ingest, thing);
		job.count = FoodUtility.WillIngestStackCountOf(pawn, thing.def, thing.def.IsNutritionGivingIngestible ? thing.def.ingestible.CachedNutrition : 0.05f);
		context.StartOrQueueJob(job);
		EALogger.Debug(pawn.Name.ToStringShort + " force eating " + thing.Label);
		return ExecutionResult.Succeeded(context);
	}
}
