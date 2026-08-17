using HarmonyLib;
using Ustas.RimAI.Actions.Core;
using Ustas.RimAI.Actions.Integration;
using Ustas.RimAI.Actions.UI;
using Ustas.RimAI.Actions.Util;
using Ustas.RimAI.Core.Handshake;
using UnityEngine;
using Verse;

namespace Ustas.RimAI.Actions.Mod;

public class EAModMain : Verse.Mod
{
	public const string HandshakeModuleVersion = "0.3.0-beta.2";
	public static EAModMain Instance { get; private set; }

	public static EASettings Settings { get; private set; }

	public static Harmony HarmonyInstance { get; private set; }

	public EAModMain(ModContentPack content)
		: base(content)
	{
		Instance = this;
		Settings = GetSettings<EASettings>();
		Settings.Validate();
		RimAiHandshake.TryActivate(
			RimAiHandshakeDescriptor.Current(RimAiModuleIds.Actions, HandshakeModuleVersion, isOptional: true),
			Activate);
	}

	static void Activate()
	{
		HarmonyInstance = new Harmony("ustas.rimai.actions");
		EALogger.Info("Expand Actions initializing...");
		HarmonyInstance.PatchAll();
		ActionRegistry.Initialize();
		CommunicationIntegration.Initialize();
		Ustas.RimAI.Core.Modules.RimAIModuleRegistry.Current.Register(
			new Ustas.RimAI.Core.Modules.RimAIModuleDescriptor(
				"actions",
				"RimAI.Actions",
				"RimAI.Actions",
				"Actions"));
		EALogger.Info("Expand Actions initialized.");
	}

	public override string SettingsCategory()
	{
		return Content?.Name ?? "RimAI.Actions";
	}

	public override void DoSettingsWindowContents(Rect inRect)
	{
		Ustas.RimAI.Core.Modules.RimAISettingsNavigation.Open("actions");
		EASettingsWindow.DoWindowContents(inRect, Settings);
	}
}
