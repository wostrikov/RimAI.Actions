using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using Verse;

namespace RimTalk.ExpandActions.Actions.Combat;

public class DraftSetHandler : IActionHandler
{
	public string ActionId => "draft_set";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn resolvedActor = context.ResolvedActor;
		bool arg = context.ActionCall.GetArg("drafted", defaultValue: true);
		if (resolvedActor.drafter == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.ActorIncapable, "Pawn cannot be drafted");
		}
		resolvedActor.drafter.Drafted = arg;
		EALogger.Debug($"{resolvedActor.Name.ToStringShort} drafted: {arg}");
		return ExecutionResult.Succeeded(context);
	}
}
