using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;

namespace RimTalk.ExpandActions.Actions.Social;

public class ThoughtAddHandler : IActionHandler
{
	public string ActionId => "thought_add";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn resolvedActor = context.ResolvedActor;
		string arg = context.ActionCall.GetArg<string>("thought");
		if (string.IsNullOrEmpty(arg))
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Thought name required (args.thought)");
		}
		ThoughtDef namedSilentFail = DefDatabase<ThoughtDef>.GetNamedSilentFail(arg);
		if (namedSilentFail == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "ThoughtDef '" + arg + "' not found");
		}
		if (resolvedActor.needs?.mood?.thoughts?.memories == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Pawn has no mood/thought system");
		}
		if (context.ResolvedTarget is Pawn pawn)
		{
			resolvedActor.needs.mood.thoughts.memories.TryGainMemory(namedSilentFail, pawn);
			EALogger.Debug(resolvedActor.Name.ToStringShort + " gained thought " + arg + " about " + pawn.Name.ToStringShort);
		}
		else
		{
			resolvedActor.needs.mood.thoughts.memories.TryGainMemory(namedSilentFail);
			EALogger.Debug(resolvedActor.Name.ToStringShort + " gained thought " + arg);
		}
		return ExecutionResult.Succeeded(context);
	}
}
