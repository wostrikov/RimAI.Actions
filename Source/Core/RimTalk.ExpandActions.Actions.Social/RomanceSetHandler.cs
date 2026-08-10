using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;

namespace RimTalk.ExpandActions.Actions.Social;

public class RomanceSetHandler : IActionHandler
{
	public string ActionId => "romance_set";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn resolvedActor = context.ResolvedActor;
		if (!(context.ResolvedTarget is Pawn pawn))
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "Target pawn required for romance action");
		}
		string text = context.ActionCall.GetArg("mode", "new_lover")?.ToLower() ?? "new_lover";
		if (resolvedActor.relations == null || pawn.relations == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Pawn has no relations tracker");
		}
		if (!(text == "new_lover"))
		{
			if (!(text == "breakup"))
			{
				return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Unknown romance mode: " + text + ". Use 'new_lover' or 'breakup'");
			}
			bool flag = false;
			DirectPawnRelation directRelation = resolvedActor.relations.GetDirectRelation(PawnRelationDefOf.Lover, pawn);
			if (directRelation != null)
			{
				resolvedActor.relations.RemoveDirectRelation(directRelation);
				resolvedActor.relations.AddDirectRelation(PawnRelationDefOf.ExLover, pawn);
				flag = true;
			}
			DirectPawnRelation directRelation2 = resolvedActor.relations.GetDirectRelation(PawnRelationDefOf.Fiance, pawn);
			if (directRelation2 != null)
			{
				resolvedActor.relations.RemoveDirectRelation(directRelation2);
				resolvedActor.relations.AddDirectRelation(PawnRelationDefOf.ExLover, pawn);
				flag = true;
			}
			DirectPawnRelation directRelation3 = resolvedActor.relations.GetDirectRelation(PawnRelationDefOf.Spouse, pawn);
			if (directRelation3 != null)
			{
				resolvedActor.relations.RemoveDirectRelation(directRelation3);
				resolvedActor.relations.AddDirectRelation(PawnRelationDefOf.ExSpouse, pawn);
				flag = true;
			}
			if (!flag)
			{
				return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "No romantic relationship to break up");
			}
			EALogger.Debug(resolvedActor.Name.ToStringShort + " broke up with " + pawn.Name.ToStringShort);
		}
		else if (resolvedActor.relations.GetDirectRelation(PawnRelationDefOf.Lover, pawn) == null)
		{
			resolvedActor.relations.AddDirectRelation(PawnRelationDefOf.Lover, pawn);
			EALogger.Debug(resolvedActor.Name.ToStringShort + " and " + pawn.Name.ToStringShort + " are now lovers");
		}
		else
		{
			EALogger.Debug(resolvedActor.Name.ToStringShort + " and " + pawn.Name.ToStringShort + " are already lovers");
		}
		return ExecutionResult.Succeeded(context);
	}
}
