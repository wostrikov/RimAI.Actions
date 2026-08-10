using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;

namespace RimTalk.ExpandActions.Actions.Social;

public class RelationSetHandler : IActionHandler
{
	public string ActionId => "relation_set";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn resolvedActor = context.ResolvedActor;
		if (!(context.ResolvedTarget is Pawn pawn))
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "Target pawn required for relation action");
		}
		string arg = context.ActionCall.GetArg<string>("relation");
		string text = context.ActionCall.GetArg("mode", "add")?.ToLower() ?? "add";
		if (string.IsNullOrEmpty(arg))
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Relation name required (args.relation)");
		}
		PawnRelationDef namedSilentFail = DefDatabase<PawnRelationDef>.GetNamedSilentFail(arg);
		if (namedSilentFail == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "PawnRelationDef '" + arg + "' not found");
		}
		if (resolvedActor.relations == null || pawn.relations == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Pawn has no relations tracker");
		}
		if (!(text == "add"))
		{
			if (!(text == "remove"))
			{
				return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Unknown mode: " + text + ". Use 'add' or 'remove'");
			}
			DirectPawnRelation directRelation = resolvedActor.relations.GetDirectRelation(namedSilentFail, pawn);
			if (directRelation == null)
			{
				return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "No " + arg + " relation to remove");
			}
			resolvedActor.relations.RemoveDirectRelation(directRelation);
			EALogger.Debug(resolvedActor.Name.ToStringShort + " removed " + arg + " relation with " + pawn.Name.ToStringShort);
		}
		else if (resolvedActor.relations.GetDirectRelation(namedSilentFail, pawn) != null)
		{
			EALogger.Debug(resolvedActor.Name.ToStringShort + " already has " + arg + " relation with " + pawn.Name.ToStringShort);
		}
		else
		{
			resolvedActor.relations.AddDirectRelation(namedSilentFail, pawn);
			EALogger.Debug(resolvedActor.Name.ToStringShort + " added " + arg + " relation with " + pawn.Name.ToStringShort);
		}
		return ExecutionResult.Succeeded(context);
	}
}
