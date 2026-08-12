using HarmonyLib;
using RimTalk.ExpandActions.Integration;
using Verse;

namespace RimTalk.ExpandActions.Patches;

/// <summary>
/// Root_Play.Start is RimWorld's native transition into a ready main menu.
/// The marker and explicit save guards keep this patch inert in normal play.
/// </summary>
[HarmonyPatch(typeof(Root_Play), nameof(Root_Play.Start))]
public static class Patch_Stage6ProbeMenuStart
{
	public static void Postfix()
	{
		Stage6RuntimeProbe.TryLoadDisposableSave();
	}
}
