using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Production;

public class FixBrokenHandler : IActionHandler
{
	public string ActionId => "fix_broken";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn resolvedActor = context.ResolvedActor;
		Thing thing = context.ResolvedTarget;
		if (!context.CanActorAct())
		{
			return ExecutionResult.Failed(context, ErrorCode.ActorIncapable, "Actor cannot act");
		}
		if (thing == null)
		{
			thing = NearbyTargetFinder.FindBrokenDown(resolvedActor);
		}
		if (thing == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "No broken-down building found nearby");
		}
		CompBreakdownable compBreakdownable = thing.TryGetComp<CompBreakdownable>();
		if (compBreakdownable == null || !compBreakdownable.BrokenDown)
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Target is not broken down");
		}
		if (!resolvedActor.CanReserveAndReach(thing, PathEndMode.Touch, Danger.Some))
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetUnreachable, "Cannot reach target");
		}
		JobDef namedSilentFail = DefDatabase<JobDef>.GetNamedSilentFail("FixBrokenDownBuilding");
		if (namedSilentFail == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.ExecutionException, "FixBrokenDownBuilding JobDef not found");
		}
		Verse.AI.Job job = JobMaker.MakeJob(namedSilentFail, thing);
		context.StartOrQueueJob(job);
		EALogger.Debug(resolvedActor.Name.ToStringShort + " fixing " + thing.LabelCap);
		return ExecutionResult.Succeeded(context);
	}
}
