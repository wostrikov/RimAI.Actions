using HarmonyLib;
using RimAI.RimWorld.Runtime;
using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Integration;
using RimTalk.ExpandActions.UI;
using RimTalk.ExpandActions.Util;
using UnityEngine;
using Verse;

namespace RimTalk.ExpandActions.Mod;

public class EAModMain : Verse.Mod
{
	public static EAModMain Instance { get; private set; }

	public static EASettings Settings { get; private set; }

	public static Harmony HarmonyInstance { get; private set; }

	public EAModMain(ModContentPack content)
		: base(content)
	{
		Instance = this;
		Settings = GetSettings<EASettings>();
		Settings.Validate();
		HarmonyInstance = new Harmony("rimtalk.expand.actions");
		EALogger.Info("Expand Actions initializing...");
		// TEMPORARY_MIGRATION_BRIDGE: EA is still the active mod bootstrap.
		// Remove when RimAI has its own package entry point.
		RimAIRuntimeHost.Install();
		HarmonyInstance.PatchAll();
		ActionRegistry.Initialize();
		RimTalkIntegration.Initialize();
		EALogger.Info("Expand Actions initialized.");
	}

	public override string SettingsCategory()
	{
		return "RimTalk - Expand Actions";
	}

	public override void DoSettingsWindowContents(Rect inRect)
	{
		EASettingsWindow.DoWindowContents(inRect, Settings);
	}
}
