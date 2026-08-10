using System.Linq;
using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Funeral;

public class CremateHandler : IActionHandler
{
	public string ActionId => "cremate";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn pawn = context.ResolvedActor;
		Thing resolvedTarget = context.ResolvedTarget;
		if (!context.CanActorAct())
		{
			return ExecutionResult.Failed(context, ErrorCode.ActorIncapable, "Actor cannot act");
		}
		if (resolvedTarget == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "Corpse not found");
		}
		if (!(resolvedTarget is Corpse))
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Target is not a corpse");
		}
		if (!pawn.CanReserveAndReach(resolvedTarget, PathEndMode.Touch, Danger.Some))
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetUnreachable, "Cannot reach corpse");
		}
		Building building = (from b in pawn.Map.listerBuildings.AllBuildingsColonistOfClass<Building>()
			where b.def.defName == "ElectricCrematorium" && pawn.CanReserveAndReach(b, PathEndMode.InteractionCell, Danger.None)
			orderby b.Position.DistanceTo(pawn.Position)
			select b).FirstOrDefault();
		if (building == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "No crematorium found");
		}
		Verse.AI.Job job = HaulAIUtility.HaulToContainerJob(pawn, resolvedTarget, building);
		if (job == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.ExecutionException, "Cannot create cremation job");
		}
		context.StartOrQueueJob(job);
		EALogger.Debug(pawn.Name.ToStringShort + " cremating " + resolvedTarget.LabelCap);
		return ExecutionResult.Succeeded(context);
	}
}
