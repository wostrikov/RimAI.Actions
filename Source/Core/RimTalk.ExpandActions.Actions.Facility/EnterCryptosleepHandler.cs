using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Facility;

public class EnterCryptosleepHandler : IActionHandler
{
	public string ActionId => "enter_cryptosleep";

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
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "Cryptosleep casket not found");
		}
		if (!(resolvedTarget is Building_CryptosleepCasket))
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Target is not a cryptosleep casket");
		}
		if (!resolvedActor.CanReserveAndReach(resolvedTarget, PathEndMode.InteractionCell, Danger.None))
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetUnreachable, "Cannot reach cryptosleep casket");
		}
		Verse.AI.Job job = JobMaker.MakeJob(JobDefOf.EnterCryptosleepCasket, resolvedTarget);
		context.StartOrQueueJob(job);
		EALogger.Debug(resolvedActor.Name.ToStringShort + " entering cryptosleep");
		return ExecutionResult.Succeeded(context);
	}
}
