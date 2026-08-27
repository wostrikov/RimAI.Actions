using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Ustas.RimAI.Actions.Mod;

public class EASettings : ModSettings
{
	public bool Enabled = true;

	public Dictionary<string, ActionConfig> ActionConfigs = new Dictionary<string, ActionConfig>();

	public List<string> CustomJobWhitelist = new List<string>();

	public bool ShowExecutionBubbles = true;

	public bool SkipWorkTimePawns;

	public bool SkipDraftedPawns = true;

	public bool UseSecondaryLLM;

	public int SecondaryLLMTimeout = 30;

	public int MaxActionsPerConversation = 10;

	public bool EnableKeywordPreFilter = true;

	public bool EnableCooldown = true;

	public int DefaultCooldownTicks = 3600;

	public int MovementCooldownTicks = 600;

	public int CombatCooldownTicks = 1800;

	public int SocialCooldownTicks = 7200;

	public int ActionEffortLevel = 1;

	public bool AllowUndesignatedTargets;

	public int JobProtectionTicks = 2500;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref Enabled, "enabled", defaultValue: true);
		Scribe_Values.Look(ref ShowExecutionBubbles, "showExecutionBubbles", defaultValue: true);
		Scribe_Values.Look(ref SkipWorkTimePawns, "skipWorkTimePawns", defaultValue: false);
		Scribe_Values.Look(ref SkipDraftedPawns, "skipDraftedPawns", defaultValue: true);
		Scribe_Values.Look(ref UseSecondaryLLM, "useSecondaryLLM", defaultValue: false);
		Scribe_Values.Look(ref SecondaryLLMTimeout, "secondaryLLMTimeout", 30);
		Scribe_Values.Look(ref MaxActionsPerConversation, "maxActionsPerConversation", 10);
		Scribe_Values.Look(ref EnableKeywordPreFilter, "enableKeywordPreFilter", defaultValue: true);
		Scribe_Values.Look(ref EnableCooldown, "enableCooldown", defaultValue: true);
		Scribe_Values.Look(ref DefaultCooldownTicks, "defaultCooldownTicks", 3600);
		Scribe_Values.Look(ref MovementCooldownTicks, "movementCooldownTicks", 600);
		Scribe_Values.Look(ref CombatCooldownTicks, "combatCooldownTicks", 1800);
		Scribe_Values.Look(ref SocialCooldownTicks, "socialCooldownTicks", 7200);
		Scribe_Values.Look(ref ActionEffortLevel, "actionEffortLevel", 1);
		Scribe_Values.Look(ref AllowUndesignatedTargets, "allowUndesignatedTargets", defaultValue: false);
		Scribe_Values.Look(ref JobProtectionTicks, "jobProtectionTicks", 2500);
		Scribe_Collections.Look(ref CustomJobWhitelist, "customJobWhitelist", LookMode.Value);
		Scribe_Collections.Look(ref ActionConfigs, "actionConfigs", LookMode.Value, LookMode.Deep);
		if (CustomJobWhitelist == null)
		{
			CustomJobWhitelist = new List<string>();
		}
		if (ActionConfigs == null)
		{
			ActionConfigs = new Dictionary<string, ActionConfig>();
		}
	}

	public ActionConfig GetActionConfig(string actionId)
	{
		if (!ActionConfigs.TryGetValue(actionId, out var value))
		{
			value = new ActionConfig();
			ActionConfigs[actionId] = value;
		}
		return value;
	}

	public bool IsActionEnabled(string actionId)
	{
		if (!Enabled)
		{
			return false;
		}
		return GetActionConfig(actionId).Enabled;
	}

	public string GetPromptDesc(string actionId, string defaultDesc)
	{
		ActionConfig actionConfig = GetActionConfig(actionId);
		if (!string.IsNullOrEmpty(actionConfig.CustomPromptDesc))
		{
			return actionConfig.CustomPromptDesc;
		}
		return defaultDesc;
	}

	public void Validate()
	{
		SecondaryLLMTimeout = Mathf.Clamp(SecondaryLLMTimeout, 1, 60);
		MaxActionsPerConversation = Mathf.Clamp(MaxActionsPerConversation, 1, 20);
		DefaultCooldownTicks = Mathf.Clamp(DefaultCooldownTicks, 0, 60000);
		MovementCooldownTicks = Mathf.Clamp(MovementCooldownTicks, 0, 60000);
		CombatCooldownTicks = Mathf.Clamp(CombatCooldownTicks, 0, 60000);
		SocialCooldownTicks = Mathf.Clamp(SocialCooldownTicks, 0, 60000);
		ActionEffortLevel = Mathf.Clamp(ActionEffortLevel, 0, 2);
		JobProtectionTicks = Mathf.Clamp(JobProtectionTicks, 0, 15000);
	}
}
