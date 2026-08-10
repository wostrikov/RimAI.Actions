using System.Linq;
using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.DLC;

public class MeditateHandler : IActionHandler
{
	public string ActionId => "meditate";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn pawn = context.ResolvedActor;
		if (!context.CanActorAct())
		{
			return ExecutionResult.Failed(context, ErrorCode.ActorIncapable, "Actor cannot act");
		}
		Building building = (from b in pawn.Map.listerBuildings.AllBuildingsColonistOfClass<Building>()
			where b.def.defName == "MeditationSpot" || b.def.defName == "Throne"
			where pawn.CanReserveAndReach(b, PathEndMode.InteractionCell, Danger.None)
			orderby b.Position.DistanceTo(pawn.Position)
			select b).FirstOrDefault();
		if (building == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "No meditation spot or throne found");
		}
		JobDef namedSilentFail = DefDatabase<JobDef>.GetNamedSilentFail("Meditate");
		if (namedSilentFail == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.ExecutionException, "Meditate JobDef not found");
		}
		Verse.AI.Job job = JobMaker.MakeJob(namedSilentFail, building);
		context.StartOrQueueJob(job);
		EALogger.Debug(pawn.Name.ToStringShort + " meditating at " + building.Label);
		return ExecutionResult.Succeeded(context);
	}
}
