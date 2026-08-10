using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Recreation;

public class StargazeHandler : IActionHandler
{
	public string ActionId => "stargaze";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn resolvedActor = context.ResolvedActor;
		if (!context.CanActorAct())
		{
			return ExecutionResult.Failed(context, ErrorCode.ActorIncapable, "Actor cannot act");
		}
		JobDef namedSilentFail = DefDatabase<JobDef>.GetNamedSilentFail("Stargaze");
		if (namedSilentFail == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.ExecutionException, "Stargaze JobDef not found");
		}
		Verse.AI.Job job = JobMaker.MakeJob(namedSilentFail);
		context.StartOrQueueJob(job);
		EALogger.Debug(resolvedActor.Name.ToStringShort + " stargazing");
		return ExecutionResult.Succeeded(context);
	}
}
