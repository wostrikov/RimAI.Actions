using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;

namespace RimTalk.ExpandActions.Actions.Social;

public class InsultHandler : IActionHandler
{
	public string ActionId => "insult";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn resolvedActor = context.ResolvedActor;
		Pawn pawn = context.ResolvedTarget as Pawn;
		if (!context.CanActorAct())
		{
			return ExecutionResult.Failed(context, ErrorCode.ActorIncapable, "Actor cannot act");
		}
		if (pawn == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "Target pawn not found");
		}
		if (pawn.Dead || pawn.Destroyed)
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Target is dead");
		}
		ThoughtDef thoughtDef = ThoughtDef.Named("Insulted");
		if (thoughtDef != null)
		{
			pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(thoughtDef, resolvedActor);
		}
		EALogger.Debug(resolvedActor.Name.ToStringShort + " insulting " + pawn.Name.ToStringShort);
		return ExecutionResult.Succeeded(context);
	}
}
