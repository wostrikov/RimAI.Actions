using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Parsing;
using RimTalk.ExpandActions.Util;
using Verse;

namespace RimTalk.ExpandActions.Actions.DLC;

public class RitualHandler : IActionHandler
{
	public string ActionId => "ritual";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn resolvedActor = context.ResolvedActor;
		if (!context.CanActorAct())
		{
			return ExecutionResult.Failed(context, ErrorCode.ActorIncapable, "Actor cannot act");
		}
		ActionCall actionCall = context.ActionCall;
		object value = default(object);
		string text = ((actionCall != null && actionCall.Args?.TryGetValue("ritual_type", out value) == true) ? (value as string) : null);
		if (string.IsNullOrEmpty(text))
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Ritual type required (args.ritual_type)");
		}
		EALogger.Info(resolvedActor.Name.ToStringShort + " wants to start ritual: " + text + " (ritual initiation is managed by game systems)");
		return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Ritual '" + text + "' must be started through the game's ritual interface. EA cannot directly start rituals.");
	}
}
