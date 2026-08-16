using Ustas.RimAI.Actions.Actions;
using Ustas.RimAI.Actions.Core;
using Ustas.RimAI.Actions.Execution;
using Ustas.RimAI.Actions.RJW.Util;
using Ustas.RimAI.Actions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace Ustas.RimAI.Actions.RJW.Handlers;

public class RJWSexHandler : IActionHandler
{
	public string ActionId => "rjw_sex";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn resolvedActor = context.ResolvedActor;
		Pawn targetPawn = context.TargetPawn;
		if (targetPawn == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "Target pawn not found");
		}
		if (targetPawn.Dead || targetPawn.Downed)
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Target is dead or downed");
		}
		if (!RJWReflectionCache.CanFuck(resolvedActor))
		{
			return ExecutionResult.Failed(context, ErrorCode.ActorIncapable, "Actor cannot perform this action (CanFuck=false)");
		}
		if (!RJWReflectionCache.CanBeFucked(targetPawn))
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Target cannot receive this action (CanBeFucked=false)");
		}
		Building_Bed building_Bed = targetPawn.CurrentBed() ?? resolvedActor.CurrentBed();
		if (building_Bed == null)
		{
			building_Bed = RestUtility.FindBedFor(resolvedActor, resolvedActor, checkSocialProperness: false);
		}
		if (building_Bed != null)
		{
			JobDef joinInBedDef = RJWReflectionCache.GetJoinInBedDef();
			if (joinInBedDef != null)
			{
				Job job = JobMaker.MakeJob(joinInBedDef, targetPawn, building_Bed);
				context.StartOrQueueJob(job);
				EALogger.Debug("rjw_sex: " + resolvedActor.LabelShort + " JoinInBed with " + targetPawn.LabelShort);
				return ExecutionResult.Succeeded(context);
			}
		}
		JobDef quickieDef = RJWReflectionCache.GetQuickieDef();
		if (quickieDef != null)
		{
			Job job2 = JobMaker.MakeJob(quickieDef, targetPawn);
			context.StartOrQueueJob(job2);
			EALogger.Debug("rjw_sex: " + resolvedActor.LabelShort + " Quickie (no bed) with " + targetPawn.LabelShort);
			return ExecutionResult.Succeeded(context);
		}
		return ExecutionResult.Failed(context, ErrorCode.ExecutionException, "Neither JoinInBed nor Quickie JobDef found");
	}
}
