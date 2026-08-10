using System.Collections.Generic;
using System.Linq;
using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Mod;
using UnityEngine;
using Verse;

namespace RimTalk.ExpandActions.Execution;

public static class CooldownTracker
{
	private struct CooldownKey
	{
		public int PawnThingId;

		public string ActionId;

		public int TargetThingId;

		public override int GetHashCode()
		{
			return (PawnThingId * 397) ^ (ActionId?.GetHashCode() ?? 0) ^ (TargetThingId * 17);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is CooldownKey cooldownKey))
			{
				return false;
			}
			if (PawnThingId == cooldownKey.PawnThingId && ActionId == cooldownKey.ActionId)
			{
				return TargetThingId == cooldownKey.TargetThingId;
			}
			return false;
		}
	}

	private static readonly Dictionary<CooldownKey, int> _records = new Dictionary<CooldownKey, int>();

	private static int _lastCleanupTick = 0;

	private const int CleanupInterval = 10000;

	public static bool IsOnCooldown(int pawnThingId, string actionId, int targetThingId, ActionCategory category)
	{
		EASettings settings = EAModMain.Settings;
		if (!settings.EnableCooldown)
		{
			return false;
		}
		CooldownKey key = new CooldownKey
		{
			PawnThingId = pawnThingId,
			ActionId = actionId,
			TargetThingId = targetThingId
		};
		if (!_records.TryGetValue(key, out var value))
		{
			return false;
		}
		int num = Find.TickManager?.TicksGame ?? 0;
		int cooldownForCategory = GetCooldownForCategory(category, settings);
		return num - value < cooldownForCategory;
	}

	public static void RecordExecution(int pawnThingId, string actionId, int targetThingId)
	{
		CooldownKey key = new CooldownKey
		{
			PawnThingId = pawnThingId,
			ActionId = actionId,
			TargetThingId = targetThingId
		};
		_records[key] = Find.TickManager?.TicksGame ?? 0;
		MaybeCleanup();
	}

	private static void MaybeCleanup()
	{
		int currentTick = Find.TickManager?.TicksGame ?? 0;
		if (currentTick - _lastCleanupTick < 10000)
		{
			return;
		}
		_lastCleanupTick = currentTick;
		int maxCooldown = Mathf.Max(EAModMain.Settings.DefaultCooldownTicks, EAModMain.Settings.CombatCooldownTicks, EAModMain.Settings.SocialCooldownTicks) * 2;
		foreach (CooldownKey item in (from kv in _records
			where currentTick - kv.Value > maxCooldown
			select kv.Key).ToList())
		{
			_records.Remove(item);
		}
	}

	private static int GetCooldownForCategory(ActionCategory category, EASettings settings)
	{
		switch (category)
		{
		case ActionCategory.Movement:
		case ActionCategory.Production:
		case ActionCategory.Recreation:
			return settings.MovementCooldownTicks;
		case ActionCategory.Combat:
			return settings.CombatCooldownTicks;
		case ActionCategory.Social:
			return settings.SocialCooldownTicks;
		default:
			return settings.DefaultCooldownTicks;
		}
	}
}
