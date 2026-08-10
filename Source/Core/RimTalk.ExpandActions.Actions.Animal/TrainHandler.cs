using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Animal;

public class TrainHandler : IActionHandler
{
	public string ActionId => "train";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn resolvedActor = context.ResolvedActor;
		Pawn pawn = context.ResolvedTarget as Pawn;
		if (!context.CanActorAct())
		{
			return ExecutionResult.Failed(context, ErrorCode.ActorIncapable, "Actor cannot act");
		}
		if (pawn != null)
		{
			RaceProperties raceProps = pawn.RaceProps;
			if (raceProps != null && raceProps.Animal)
			{
				if (pawn.Faction != resolvedActor.Faction)
				{
					return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Animal must be tamed first");
				}
				if (!resolvedActor.CanReserveAndReach(pawn, PathEndMode.Touch, Danger.Some))
				{
					return ExecutionResult.Failed(context, ErrorCode.TargetUnreachable, "Cannot reach animal");
				}
				Verse.AI.Job job = JobMaker.MakeJob(JobDefOf.Train, pawn);
				context.StartOrQueueJob(job);
				EALogger.Debug(resolvedActor.Name.ToStringShort + " training " + pawn.LabelCap);
				return ExecutionResult.Succeeded(context);
			}
		}
		return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Target must be an animal");
	}
}
