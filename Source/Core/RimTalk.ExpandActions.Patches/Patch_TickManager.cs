using HarmonyLib;
using RimTalk.ExpandActions.Execution;
using Verse;

namespace RimTalk.ExpandActions.Patches;

[HarmonyPatch(typeof(TickManager), "DoSingleTick")]
public static class Patch_TickManager
{
	[HarmonyPostfix]
	public static void Postfix()
	{
		MainThreadDispatcher.ProcessQueue();
	}
}
