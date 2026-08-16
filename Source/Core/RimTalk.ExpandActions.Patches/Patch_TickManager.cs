using HarmonyLib;
using Ustas.RimAI.Actions.Execution;
using Verse;

namespace Ustas.RimAI.Actions.Patches;

[HarmonyPatch(typeof(TickManager), "DoSingleTick")]
public static class Patch_TickManager
{
	[HarmonyPostfix]
	public static void Postfix()
	{
		MainThreadDispatcher.ProcessQueue();
	}
}
