using HarmonyLib;
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
		HarmonyInstance.PatchAll();
		ActionRegistry.Initialize();
		RimTalkIntegration.Initialize();
		EALogger.Info("Expand Actions initialized.");
	}

	public override string SettingsCategory()
	{
		return Content?.Name ?? "RimTalk - Expand Actions Core";
	}

	public override void DoSettingsWindowContents(Rect inRect)
	{
		EASettingsWindow.DoWindowContents(inRect, Settings);
	}
}
