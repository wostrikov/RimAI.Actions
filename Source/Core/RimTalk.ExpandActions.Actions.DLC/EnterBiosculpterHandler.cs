using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.DLC;

public class EnterBiosculpterHandler : IActionHandler
{
	public string ActionId => "enter_biosculpter";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn resolvedActor = context.ResolvedActor;
		Thing resolvedTarget = context.ResolvedTarget;
		if (!context.CanActorAct())
		{
			return ExecutionResult.Failed(context, ErrorCode.ActorIncapable, "Actor cannot act");
		}
		if (resolvedTarget == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "Biosculpter pod not found");
		}
		if (resolvedTarget.def.defName != "BiosculpterPod")
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Target is not a biosculpter pod");
		}
		if (!resolvedActor.CanReserveAndReach(resolvedTarget, PathEndMode.InteractionCell, Danger.None))
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetUnreachable, "Cannot reach biosculpter pod");
		}
		JobDef namedSilentFail = DefDatabase<JobDef>.GetNamedSilentFail("EnterBiosculpterPod");
		if (namedSilentFail == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.ExecutionException, "EnterBiosculpterPod JobDef not found");
		}
		Verse.AI.Job job = JobMaker.MakeJob(namedSilentFail, resolvedTarget);
		context.StartOrQueueJob(job);
		EALogger.Debug(resolvedActor.Name.ToStringShort + " entering biosculpter pod");
		return ExecutionResult.Succeeded(context);
	}
}
