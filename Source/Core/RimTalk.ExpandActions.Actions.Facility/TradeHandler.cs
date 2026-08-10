using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using Verse;

namespace RimTalk.ExpandActions.Actions.Facility;

public class TradeHandler : IActionHandler
{
	public string ActionId => "trade";

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
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "Trade target not found");
		}
		if (pawn.Faction == null || pawn.TraderKind == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Target's faction cannot trade");
		}
		EALogger.Info(resolvedActor.Name.ToStringShort + " wants to trade with " + pawn.Name.ToStringShort + " (requires player interaction)");
		return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Trading requires player interaction through the trade dialog. EA cannot automate trades.");
	}
}
