using RimTalk.ExpandActions.Execution;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Movement;

public class StopHandler : IActionHandler
{
	public string ActionId => "stop";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn resolvedActor = context.ResolvedActor;
		if (resolvedActor.jobs?.curJob == null)
		{
			return ExecutionResult.Succeeded(context);
		}
		resolvedActor.jobs.EndCurrentJob(JobCondition.InterruptForced);
		resolvedActor.pather?.StopDead();
		return ExecutionResult.Succeeded(context);
	}
}
