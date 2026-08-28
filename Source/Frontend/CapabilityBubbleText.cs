using System;
using System.Collections.Generic;
using RimAI.Core.Catalog;
using RimWorld;
using Verse;

namespace Ustas.RimAI.Actions.Frontend;

/// <summary>
/// Turns an execution result into something a player can read above a pawn's
/// head. The bubble used to print the raw capability id and nothing else, so a
/// refused action showed "rimai.workgiver.mine" with no hint of why.
///
/// Almost every capability already names the def it drives, and that def
/// carries a label the game has already translated. Reading the label back is
/// therefore both shorter and better than a hand-written catalogue of two
/// hundred names that would go stale the moment the allowlist regenerates. The
/// keyed lookup stays in front of it for the handful of ids that name no def.
/// </summary>
internal static class CapabilityBubbleText
{
	private const string LabelKeyPrefix = "RimAI_Capability_";
	private const string ReasonKeyPrefix = "RimAI_ActionCode_";

	// Built once: the allowlist is fixed for the session, and its own resolver
	// rebuilds a 200-entry array per lookup. Labels are read live so a language
	// change during play is picked up.
	private static Dictionary<string, CapabilityExpansionEntry> _entries;

	internal static string Describe(string capabilityId)
	{
		if (string.IsNullOrEmpty(capabilityId))
			return string.Empty;

		string key = LabelKeyPrefix + capabilityId.Replace('.', '_');
		if (key.CanTranslate())
			return key.Translate();

		string defLabel = LabelFromDef(capabilityId);
		return string.IsNullOrEmpty(defLabel) ? Humanize(capabilityId) : defLabel;
	}

	/// <returns>
	/// A reason phrase, or empty when the code carries nothing a player needs -
	/// a success code says only what the glyph already said.
	/// </returns>
	internal static string Reason(string code, string detail)
	{
		if (string.IsNullOrEmpty(code))
			return detail ?? string.Empty;

		string key = ReasonKeyPrefix + code;
		if (key.CanTranslate())
			return key.Translate();

		// An unmapped code still beats silence: the adapter's detail is English
		// but concrete, and the code itself is at least a searchable token.
		return string.IsNullOrEmpty(detail) ? Humanize(code) : detail;
	}

	private static string LabelFromDef(string capabilityId)
	{
		if (_entries == null)
		{
			var built = new Dictionary<string, CapabilityExpansionEntry>(StringComparer.Ordinal);
			foreach (CapabilityExpansionEntry entry in CapabilityExpansionCatalog.All)
				built[entry.CapabilityId] = entry;
			_entries = built;
		}

		if (!_entries.TryGetValue(capabilityId, out CapabilityExpansionEntry match))
			return null;

		string identity = match.AllowedDefIdentity;
		if (string.IsNullOrEmpty(identity))
			return null;

		switch (match.Family)
		{
			case CapabilityFamily.Ability:
				return NonEmptyLabel(DefDatabase<AbilityDef>.GetNamedSilentFail(identity));
			case CapabilityFamily.Recipe:
				return NonEmptyLabel(DefDatabase<RecipeDef>.GetNamedSilentFail(identity));
			case CapabilityFamily.Social:
				return NonEmptyLabel(DefDatabase<InteractionDef>.GetNamedSilentFail(identity));
			case CapabilityFamily.Work:
				return WorkGiverLabel(DefDatabase<WorkGiverDef>.GetNamedSilentFail(identity));
			default:
				return null;
		}
	}

	private static string NonEmptyLabel(Def def)
	{
		string label = def?.label;
		return string.IsNullOrEmpty(label) ? null : def.LabelCap.ToString();
	}

	private static string WorkGiverLabel(WorkGiverDef def)
	{
		if (def == null)
			return null;

		// WorkGiverDefs rarely carry a label; the verb is what the game itself
		// shows the player, and it is translated through DefInjected like any
		// other def field.
		string label = NonEmptyLabel(def);
		if (!string.IsNullOrEmpty(label))
			return label;

		if (!string.IsNullOrEmpty(def.verb))
			return def.verb.CapitalizeFirst();

		string gerund = def.workType?.gerundLabel;
		return string.IsNullOrEmpty(gerund) ? null : gerund.CapitalizeFirst();
	}

	private static string Humanize(string token)
	{
		int lastDot = token.LastIndexOf('.');
		string tail = lastDot >= 0 && lastDot < token.Length - 1
			? token.Substring(lastDot + 1)
			: token;
		return tail.Replace('_', ' ');
	}
}
