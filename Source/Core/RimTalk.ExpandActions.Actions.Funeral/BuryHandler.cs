using System.Linq;
using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Funeral;

public class BuryHandler : IActionHandler
{
	public string ActionId => "bury";

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
		Building_Grave building_Grave = (from g in pawn.Map.listerBuildings.AllBuildingsColonistOfClass<Building_Grave>()
			where !g.HasCorpse && pawn.CanReserveAndReach(g, PathEndMode.InteractionCell, Danger.None)
			orderby g.Position.DistanceTo(pawn.Position)
			select g).FirstOrDefault();
		if (building_Grave == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "No empty grave available");
		}
		Verse.AI.Job job = HaulAIUtility.HaulToContainerJob(pawn, resolvedTarget, building_Grave);
		if (job == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.ExecutionException, "Cannot create burial job");
		}
		context.StartOrQueueJob(job);
		EALogger.Debug(pawn.Name.ToStringShort + " burying " + resolvedTarget.LabelCap);
		return ExecutionResult.Succeeded(context);
	}
}
