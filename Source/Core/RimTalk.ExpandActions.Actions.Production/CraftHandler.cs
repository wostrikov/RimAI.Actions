using System.Linq;
using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Production;

public class CraftHandler : IActionHandler
{
	public string ActionId => "craft";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn pawn = context.ResolvedActor;
		Thing resolvedTarget = context.ResolvedTarget;
		if (!context.CanActorAct())
		{
			return ExecutionResult.Failed(context, ErrorCode.ActorIncapable, "Actor cannot act");
		}
		Building_WorkTable building_WorkTable = resolvedTarget as Building_WorkTable;
		if (building_WorkTable == null)
		{
			building_WorkTable = (from b in pawn.Map.listerBuildings.AllBuildingsColonistOfClass<Building_WorkTable>()
				where b.BillStack.AnyShouldDoNow && pawn.CanReserveAndReach(b, PathEndMode.InteractionCell, Danger.None)
				orderby b.Position.DistanceTo(pawn.Position)
				select b).FirstOrDefault();
		}
		if (building_WorkTable == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "No workbench with active bills found");
		}
		Verse.AI.Job job = JobMaker.MakeJob(JobDefOf.DoBill, building_WorkTable);
		context.StartOrQueueJob(job);
		EALogger.Debug(pawn.Name.ToStringShort + " crafting at " + building_WorkTable.LabelCap);
		return ExecutionResult.Succeeded(context);
	}
}
