using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Parsing;
using RimTalk.ExpandActions.Util;
using Verse;

namespace RimTalk.ExpandActions.Actions.DLC;

public class CommandMechanoidHandler : IActionHandler
{
	public string ActionId => "command_mechanoid";

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
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "Target mechanoid not found");
		}
		RaceProperties raceProps = pawn.RaceProps;
		if (raceProps == null || !raceProps.IsMechanoid)
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Target is not a mechanoid");
		}
		ActionCall actionCall = context.ActionCall;
		object value = default(object);
		string text = ((actionCall != null && actionCall.Args?.TryGetValue("command", out value) == true) ? (value as string) : null);
		if (string.IsNullOrEmpty(text))
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Command required (args.command: go_to/attack/follow)");
		}
		if (pawn.GetOverseer() != resolvedActor)
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Actor is not the overseer of this mechanoid");
		}
		switch (text.ToLowerInvariant())
		{
		case "attack":
			if (pawn.drafter != null)
			{
				pawn.drafter.Drafted = true;
			}
			EALogger.Debug(resolvedActor.Name.ToStringShort + " commanding mechanoid " + pawn.LabelCap + " to attack");
			break;
		case "follow":
			if (pawn.drafter != null)
			{
				pawn.drafter.Drafted = true;
			}
			EALogger.Debug(resolvedActor.Name.ToStringShort + " commanding mechanoid " + pawn.LabelCap + " to follow");
			break;
		case "go_to":
			if (pawn.drafter != null)
			{
				pawn.drafter.Drafted = true;
			}
			EALogger.Debug(resolvedActor.Name.ToStringShort + " commanding mechanoid " + pawn.LabelCap + " to go");
			break;
		default:
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Unknown command: " + text + ". Use: go_to, attack, follow");
		}
		return ExecutionResult.Succeeded(context);
	}
}
