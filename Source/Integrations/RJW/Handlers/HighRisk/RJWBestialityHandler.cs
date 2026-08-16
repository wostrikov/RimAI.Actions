using Ustas.RimAI.Actions.Actions;
using Ustas.RimAI.Actions.Core;
using Ustas.RimAI.Actions.Execution;
using Ustas.RimAI.Actions.RJW.Util;
using Verse;
using Verse.AI;

namespace Ustas.RimAI.Actions.RJW.Handlers.RJWHighRisk;

public class RJWBestialityHandler : IActionHandler
{
	public string ActionId => "rjw_bestiality";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn resolvedActor = context.ResolvedActor;
		Thing resolvedTarget = context.ResolvedTarget;
		if (resolvedTarget == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "Target not found");
		}
		if (resolvedTarget is Pawn pawn)
		{
			RaceProperties raceProps = pawn.RaceProps;
			if (raceProps != null && raceProps.Animal)
			{
				if (pawn.Dead)
				{
					return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Target is dead");
				}
				if (!RJWReflectionCache.CanFuck(resolvedActor))
				{
					return ExecutionResult.Failed(context, ErrorCode.ActorIncapable, "Actor cannot perform this action");
				}
				JobDef quickieDef = RJWReflectionCache.GetQuickieDef();
				if (quickieDef == null)
				{
					return ExecutionResult.Failed(context, ErrorCode.ExecutionException, "Quickie JobDef not found");
				}
				Job job = JobMaker.MakeJob(quickieDef, resolvedTarget);
				context.StartOrQueueJob(job);
				return ExecutionResult.Succeeded(context);
			}
		}
		return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Target must be an animal");
	}
}
