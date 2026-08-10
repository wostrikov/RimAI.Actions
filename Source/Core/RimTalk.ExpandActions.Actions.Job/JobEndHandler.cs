using RimTalk.ExpandActions.Execution;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Job;

public class JobEndHandler : IActionHandler
{
	public string ActionId => "job_end";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn resolvedActor = context.ResolvedActor;
		if (resolvedActor.jobs?.curJob != null)
		{
			resolvedActor.jobs.EndCurrentJob(JobCondition.InterruptForced);
		}
		return ExecutionResult.Succeeded(context);
	}
}
