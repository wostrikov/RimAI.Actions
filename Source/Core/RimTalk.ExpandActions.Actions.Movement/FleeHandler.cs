using System.Collections.Generic;
using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Movement;

public class FleeHandler : IActionHandler
{
	public string ActionId => "flee";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn pawn = context.ResolvedActor;
		Map map = context.Map;
		Thing thing = null;
		Thing resolvedTarget = context.ResolvedTarget;
		if (resolvedTarget != null)
		{
			thing = resolvedTarget;
		}
		else if (context.ResolvedTarget is Pawn pawn2)
		{
			thing = pawn2;
		}
		IntVec3 intVec = ((thing == null) ? CellFinderLoose.RandomCellWith((IntVec3 c) => c.Standable(map) && pawn.CanReach(c, PathEndMode.OnCell, Danger.Some), map, 100) : CellFinderLoose.GetFleeDest(pawn, new List<Thing> { thing }, 24f));
		if (!intVec.IsValid || !intVec.InBounds(map))
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "No valid flee destination found");
		}
		Verse.AI.Job job = JobMaker.MakeJob(JobDefOf.Flee, intVec);
		if (thing != null)
		{
			job.targetB = thing;
		}
		context.StartOrQueueJob(job);
		EALogger.Debug($"{pawn.Name.ToStringShort} fleeing to {intVec}");
		return ExecutionResult.Succeeded(context);
	}
}
