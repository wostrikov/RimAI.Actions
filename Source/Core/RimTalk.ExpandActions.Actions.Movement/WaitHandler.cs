using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Movement;

public class WaitHandler : IActionHandler
{
	public string ActionId => "wait";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn resolvedActor = context.ResolvedActor;
		int arg = context.ActionCall.GetArg("ticks", 300);
		Verse.AI.Job job = JobMaker.MakeJob(JobDefOf.Wait, resolvedActor.Position);
		job.expiryInterval = arg;
		context.StartOrQueueJob(job);
		EALogger.Debug($"{resolvedActor.Name.ToStringShort} waiting for {arg} ticks");
		return ExecutionResult.Succeeded(context);
	}
}
