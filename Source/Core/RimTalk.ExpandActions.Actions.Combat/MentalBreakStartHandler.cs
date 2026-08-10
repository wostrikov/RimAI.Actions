using System.Collections.Generic;
using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using Verse;

namespace RimTalk.ExpandActions.Actions.Combat;

public class MentalBreakStartHandler : IActionHandler
{
	private static readonly HashSet<string> AllowedMentalStates = new HashSet<string>
	{
		"Berserk", "Wander_Sad", "Wander_Psychotic", "Wander_OwnRoom", "Binging_Food", "Binging_DrugMajor", "Tantrum", "SocialFighting", "InsultingSpree", "TargetedInsultingSpree",
		"Slaughterer", "Murderous", "GiveUpExit", "RunWild", "HideInRoom", "Jailbreaker", "Corpse_Obsession", "SadWander"
	};

	public string ActionId => "mental_break_start";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn resolvedActor = context.ResolvedActor;
		string arg = context.ActionCall.GetArg<string>("state");
		if (string.IsNullOrEmpty(arg))
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Mental state name required (args.state)");
		}
		if (!AllowedMentalStates.Contains(arg))
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Mental state '" + arg + "' is not in the allowed list");
		}
		MentalStateDef namedSilentFail = DefDatabase<MentalStateDef>.GetNamedSilentFail(arg);
		if (namedSilentFail == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Mental state def '" + arg + "' not found");
		}
		if (resolvedActor.mindState?.mentalStateHandler == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Pawn has no mental state handler");
		}
		if (!resolvedActor.mindState.mentalStateHandler.TryStartMentalState(namedSilentFail, "EA triggered", forced: false, forceWake: true))
		{
			return ExecutionResult.Failed(context, ErrorCode.ExecutionException, "Failed to start mental state " + arg);
		}
		EALogger.Debug(resolvedActor.Name.ToStringShort + " started mental state: " + arg);
		return ExecutionResult.Succeeded(context);
	}
}
