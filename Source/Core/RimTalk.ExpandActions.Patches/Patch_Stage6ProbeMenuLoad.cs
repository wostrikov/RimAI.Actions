using HarmonyLib;
using RimTalk.ExpandActions.Integration;
using Verse;

namespace RimTalk.ExpandActions.Patches;

[HarmonyPatch(typeof(Root_Play), "Update")]
public static class Patch_Stage6ProbeMenuLoad
{
	private static int stableFrames;
	private static bool attempted;

	public static void Postfix()
	{
		if (attempted || LongEventHandler.AnyEventNowOrWaiting)
		{
			return;
		}
		if (++stableFrames < 300)
		{
			return;
		}
		attempted = Stage6RuntimeProbe.TryLoadDisposableSave();
	}
}
