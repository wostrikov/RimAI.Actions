using System;
using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Parsing;
using RimTalk.ExpandActions.Util;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RimTalk.ExpandActions.Actions.DLC;

public class PsycastHandler : IActionHandler
{
	public string ActionId => "psycast";

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
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "Target not found");
		}
		ActionCall actionCall = context.ActionCall;
		object value = default(object);
		string abilityName = ((actionCall != null && actionCall.Args?.TryGetValue("ability", out value) == true) ? (value as string) : null);
		if (string.IsNullOrEmpty(abilityName))
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Ability name required (args.ability)");
		}
		Ability ability = resolvedActor.abilities?.abilities?.FirstOrDefault((Ability a) => string.Equals(a.def.defName, abilityName, StringComparison.OrdinalIgnoreCase) || string.Equals(a.def.label, abilityName, StringComparison.OrdinalIgnoreCase));
		if (ability == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Pawn does not have ability: " + abilityName);
		}
		if (!ability.CanCast)
		{
			return ExecutionResult.Failed(context, ErrorCode.ActorIncapable, "Cannot cast " + ability.def.label + " now");
		}
		GlobalTargetInfo target = new GlobalTargetInfo(resolvedTarget);
		ability.QueueCastingJob(target);
		EALogger.Debug(resolvedActor.Name.ToStringShort + " casting " + ability.def.label + " on " + resolvedTarget.LabelCap);
		return ExecutionResult.Succeeded(context);
	}
}
