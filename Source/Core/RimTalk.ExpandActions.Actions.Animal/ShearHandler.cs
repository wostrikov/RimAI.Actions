using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Animal;

public class ShearHandler : IActionHandler
{
	public string ActionId => "shear";

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
				if (pawn.TryGetComp<CompShearable>() == null)
				{
					return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Animal is not shearable");
				}
				if (!resolvedActor.CanReserveAndReach(pawn, PathEndMode.Touch, Danger.Some))
				{
					return ExecutionResult.Failed(context, ErrorCode.TargetUnreachable, "Cannot reach animal");
				}
				JobDef namedSilentFail = DefDatabase<JobDef>.GetNamedSilentFail("Shear");
				if (namedSilentFail == null)
				{
					return ExecutionResult.Failed(context, ErrorCode.ExecutionException, "Shear JobDef not found");
				}
				Verse.AI.Job job = JobMaker.MakeJob(namedSilentFail, pawn);
				context.StartOrQueueJob(job);
				EALogger.Debug(resolvedActor.Name.ToStringShort + " shearing " + pawn.LabelCap);
				return ExecutionResult.Succeeded(context);
			}
		}
		return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Target must be an animal");
	}
}
