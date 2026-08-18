using HarmonyLib;
using Ustas.RimAI.Actions.UI;
using Ustas.RimAI.Core.Handshake;
using UnityEngine;
using Verse;

namespace Ustas.RimAI.Actions.Mod;

public class EAModMain : Verse.Mod
{
	public const string HandshakeModuleVersion = "0.3.0-beta.2";
	public static EAModMain Instance { get; private set; }

	public static EASettings Settings { get; private set; }

	public static Harmony HarmonyInstance => ActionsComposition.Current.Harmony;

	public EAModMain(ModContentPack content)
		: base(content)
	{
		Instance = this;
		Settings = GetSettings<EASettings>();
		Settings.Validate();
		RimAiHandshake.TryActivate(
			RimAiHandshakeDescriptor.Current(RimAiModuleIds.Actions, HandshakeModuleVersion, isOptional: true),
			ActionsComposition.Current.Start);
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
