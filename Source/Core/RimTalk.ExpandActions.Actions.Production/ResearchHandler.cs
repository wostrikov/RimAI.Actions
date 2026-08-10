using System.Linq;
using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Production;

public class ResearchHandler : IActionHandler
{
	public string ActionId => "research";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn pawn = context.ResolvedActor;
		if (!context.CanActorAct())
		{
			return ExecutionResult.Failed(context, ErrorCode.ActorIncapable, "Actor cannot act");
		}
		if (Find.ResearchManager.GetProject() == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "No active research project");
		}
		Building_ResearchBench building_ResearchBench = (from b in pawn.Map.listerBuildings.AllBuildingsColonistOfClass<Building_ResearchBench>()
			where pawn.CanReserveAndReach(b, PathEndMode.InteractionCell, Danger.None)
			orderby b.Position.DistanceTo(pawn.Position)
			select b).FirstOrDefault();
		if (building_ResearchBench == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "No available research bench");
		}
		Verse.AI.Job job = JobMaker.MakeJob(JobDefOf.Research, building_ResearchBench);
		context.StartOrQueueJob(job);
		EALogger.Debug(pawn.Name.ToStringShort + " researching at " + building_ResearchBench.LabelCap);
		return ExecutionResult.Succeeded(context);
	}
}
