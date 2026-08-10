using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Movement;

public class MoveToHandler : IActionHandler
{
	public string ActionId => "move_to";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn resolvedActor = context.ResolvedActor;
		Map map = context.Map;
		IntVec3 intVec;
		if (context.ResolvedTarget is Pawn pawn)
		{
			intVec = pawn.Position;
		}
		else
		{
			Thing resolvedTarget = context.ResolvedTarget;
			if (resolvedTarget != null)
			{
				intVec = resolvedTarget.Position;
			}
			else
			{
				if (!context.ResolvedCell.HasValue)
				{
					string text = context.ActionCall?.Target ?? "(none)";
					EALogger.Info("[EA] MoveToHandler: target '" + text + "' not found, action failed");
					return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Move target not found: " + text);
				}
				intVec = context.ResolvedCell.Value;
			}
		}
		if (!intVec.InBounds(map))
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Destination out of bounds");
		}
		if (!resolvedActor.CanReach(intVec, PathEndMode.OnCell, Danger.Some))
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Cannot reach destination");
		}
		Verse.AI.Job job = JobMaker.MakeJob(JobDefOf.Goto, intVec);
		context.StartOrQueueJob(job);
		EALogger.Debug($"{resolvedActor.Name.ToStringShort} moving to {intVec}");
		return ExecutionResult.Succeeded(context);
	}
}
