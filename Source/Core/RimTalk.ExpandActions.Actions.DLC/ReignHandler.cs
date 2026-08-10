using System.Linq;
using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.DLC;

public class ReignHandler : IActionHandler
{
	public string ActionId => "reign";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn pawn = context.ResolvedActor;
		if (!context.CanActorAct())
		{
			return ExecutionResult.Failed(context, ErrorCode.ActorIncapable, "Actor cannot act");
		}
		Building building = (from b in pawn.Map.listerBuildings.AllBuildingsColonistOfClass<Building>()
			where b.def.defName == "Throne"
			where pawn.CanReserveAndReach(b, PathEndMode.InteractionCell, Danger.None)
			orderby b.Position.DistanceTo(pawn.Position)
			select b).FirstOrDefault();
		if (building == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "No throne found");
		}
		JobDef namedSilentFail = DefDatabase<JobDef>.GetNamedSilentFail("Reign");
		if (namedSilentFail == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.ExecutionException, "Reign JobDef not found");
		}
		Verse.AI.Job job = JobMaker.MakeJob(namedSilentFail, building);
		context.StartOrQueueJob(job);
		EALogger.Debug(pawn.Name.ToStringShort + " reigning on throne");
		return ExecutionResult.Succeeded(context);
	}
}
