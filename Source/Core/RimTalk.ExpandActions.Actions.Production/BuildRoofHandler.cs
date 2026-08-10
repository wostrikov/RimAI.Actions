using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Production;

public class BuildRoofHandler : IActionHandler
{
	public string ActionId => "build_roof";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn resolvedActor = context.ResolvedActor;
		if (!context.CanActorAct())
		{
			return ExecutionResult.Failed(context, ErrorCode.ActorIncapable, "Actor cannot act");
		}
		IntVec3? intVec = context.ResolvedCell ?? context.ResolvedTarget?.Position;
		if (!intVec.HasValue)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "Target location not found");
		}
		if (!resolvedActor.CanReach(intVec.Value, PathEndMode.Touch, Danger.Some))
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetUnreachable, "Cannot reach location");
		}
		JobDef namedSilentFail = DefDatabase<JobDef>.GetNamedSilentFail("BuildRoof");
		if (namedSilentFail == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.ExecutionException, "BuildRoof JobDef not found");
		}
		Verse.AI.Job job = JobMaker.MakeJob(namedSilentFail, intVec.Value);
		context.StartOrQueueJob(job);
		EALogger.Debug(resolvedActor.Name.ToStringShort + " building roof");
		return ExecutionResult.Succeeded(context);
	}
}
