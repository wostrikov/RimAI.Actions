using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;

namespace RimTalk.ExpandActions.Actions.Social;

public class GiveInspirationHandler : IActionHandler
{
	public string ActionId => "give_inspiration";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn resolvedActor = context.ResolvedActor;
		string arg = context.ActionCall.GetArg<string>("type");
		if (resolvedActor.mindState?.inspirationHandler == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Pawn has no inspiration handler");
		}
		InspirationDef inspirationDef = null;
		if (!string.IsNullOrEmpty(arg))
		{
			inspirationDef = DefDatabase<InspirationDef>.GetNamedSilentFail(arg);
			if (inspirationDef == null)
			{
				return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "InspirationDef '" + arg + "' not found");
			}
		}
		else
		{
			foreach (InspirationDef allDef in DefDatabase<InspirationDef>.AllDefs)
			{
				if (allDef.Worker.InspirationCanOccur(resolvedActor))
				{
					inspirationDef = allDef;
					break;
				}
			}
			if (inspirationDef == null)
			{
				return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "No valid inspiration found for this pawn");
			}
		}
		if (!resolvedActor.mindState.inspirationHandler.TryStartInspiration(inspirationDef))
		{
			return ExecutionResult.Failed(context, ErrorCode.ExecutionException, "Failed to give inspiration " + inspirationDef.defName);
		}
		EALogger.Debug(resolvedActor.Name.ToStringShort + " received inspiration: " + inspirationDef.defName);
		return ExecutionResult.Succeeded(context);
	}
}
