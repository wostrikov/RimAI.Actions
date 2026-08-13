using HarmonyLib;
using RimAI.RimWorld.Jobs;
using RimTalk.ExpandActions.Util;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Patches;

[HarmonyPatch]
public static class Patch_JobLifecycle
{
	[HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.StartJob))]
	[HarmonyPostfix]
	public static void StartPostfix(Job newJob, Pawn ___pawn)
	{
		Pawn pawn = ___pawn;
		RimAIJobEntry entry = pawn == null ? null : RimAIJobQueue.FindEntry(pawn.thingIDNumber, newJob);
		if (entry != null)
			EALogger.Info($"[RIMAI_TRACE] conv={entry.ConversationId} action={entry.CapabilityId ?? newJob.def?.defName} pawn={pawn.thingIDNumber} state=STARTED");
	}

	[HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.EndCurrentJob))]
	[HarmonyPrefix]
	public static void EndPrefix(Pawn_JobTracker __instance, JobCondition condition, Pawn ___pawn)
	{
		Pawn pawn = ___pawn;
		Job job = __instance?.curJob;
		if (pawn != null && job != null) RimAIJobQueue.CompleteEntry(pawn.thingIDNumber, job, condition);
	}
}
