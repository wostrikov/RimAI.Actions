using System.Collections.Generic;
using System.Linq;
using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Item;

public class EquipHandler : IActionHandler
{
	public string ActionId => "equip";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn pawn = context.ResolvedActor;
		Map map = context.Map;
		Thing thing = context.ResolvedTarget;
		string text = context.ActionCall.Thing ?? context.ActionCall.Target;
		bool flag = !string.IsNullOrEmpty(text);
		if (thing == null && flag)
		{
			EALogger.Debug("[equip] " + ThingMatcher.GetSearchDebugInfo(text, weaponOnly: true));
			IEnumerable<Thing> enumerable = map.listerThings.AllThings.Where((Thing t) => t.def.IsWeapon && !t.IsForbidden(pawn) && pawn.CanReserveAndReach(t, PathEndMode.Touch, Danger.Some));
			List<Thing> list = ThingMatcher.FindMatching(enumerable, text, weaponOnly: true).ToList();
			if (!list.Any())
			{
				List<string> values = (from t in enumerable.Take(5)
					select t.Label).ToList();
				EALogger.Debug("[equip] No match for '" + text + "'. Available: [" + string.Join(", ", values) + "]");
				return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "No weapon matching '" + text + "' found. Available: " + string.Join(", ", values));
			}
			thing = list.First();
			EALogger.Debug($"[equip] Found {list.Count} matches for '{text}', best: {thing.Label}");
		}
		if (thing == null && !flag)
		{
			thing = (from t in map.listerThings.AllThings
				where t.def.IsWeapon && !t.IsForbidden(pawn) && pawn.CanReserveAndReach(t, PathEndMode.Touch, Danger.Some)
				orderby t.Position.DistanceTo(pawn.Position)
				select t).FirstOrDefault();
			if (thing != null)
			{
				EALogger.Debug("[equip] No item specified, using nearest weapon: " + thing.Label);
			}
		}
		if (thing == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "No weapon to equip");
		}
		Verse.AI.Job job = JobMaker.MakeJob(JobDefOf.Equip, thing);
		context.StartOrQueueJob(job);
		EALogger.Debug(pawn.Name.ToStringShort + " equipping " + thing.Label);
		return ExecutionResult.Succeeded(context);
	}
}
