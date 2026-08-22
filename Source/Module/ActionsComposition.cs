using HarmonyLib;
using Ustas.RimAI.Actions.Core;
using Ustas.RimAI.Actions.Integration;
using Ustas.RimAI.Actions.Util;
using Ustas.RimAI.Core.Composition;
using Ustas.RimAI.Core.Handshake;
using Ustas.RimAI.Core.Modules;

namespace Ustas.RimAI.Actions.Mod;

/// <summary>
/// Module composition root for RimAI.Actions. Owns Harmony and registry wiring.
/// Extension discovery remains in <see cref="EAPostInit"/> (needs all mods constructed).
/// </summary>
public sealed class ActionsComposition : IRimAiModuleComposition
{
    public static ActionsComposition Current { get; } = new();

    public string ModuleId => RimAiModuleIds.Actions;

    public bool IsStarted { get; private set; }

    public Harmony Harmony { get; private set; }

    public void Start()
    {
        if (IsStarted)
            return;

        Harmony = new Harmony("ustas.rimai.actions");
        EALogger.Info("Expand Actions initializing...");
        Harmony.PatchAll();
        ActionRegistry.Initialize();
        CommunicationIntegration.Initialize();
        ActionsPipelineProbe.Register();
        RimAIModuleRegistry.Current.Register(
            new RimAIModuleDescriptor(
                "actions",
                "RimAI.Actions",
                "RimAI.Actions",
                "Actions"));
        EALogger.Info("Expand Actions initialized.");
        IsStarted = true;
    }

    public void Stop()
    {
        IsStarted = false;
    }
}
