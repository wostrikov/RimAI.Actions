using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.DLC;

public class ConvertHandler : IActionHandler
{
	public string ActionId => "convert";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn resolvedActor = context.ResolvedActor;
		Pawn pawn = context.ResolvedTarget as Pawn;
		if (!context.CanActorAct())
		{
			return ExecutionResult.Failed(context, ErrorCode.ActorIncapable, "Actor cannot act");
		}
		if (pawn == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "Target pawn not found");
		}
		if (pawn.Dead || pawn.Destroyed)
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Target is dead");
		}
		if (!resolvedActor.CanReserveAndReach(pawn, PathEndMode.Touch, Danger.Some))
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetUnreachable, "Cannot reach target");
		}
		JobDef namedSilentFail = DefDatabase<JobDef>.GetNamedSilentFail("ConvertIdeo");
		if (namedSilentFail == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.ExecutionException, "ConvertIdeo JobDef not found");
		}
		Verse.AI.Job job = JobMaker.MakeJob(namedSilentFail, pawn);
		context.StartOrQueueJob(job);
		EALogger.Debug(resolvedActor.Name.ToStringShort + " converting " + pawn.Name.ToStringShort);
		return ExecutionResult.Succeeded(context);
	}
}
