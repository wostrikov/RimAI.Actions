using System;
using System.Collections.Generic;
using System.Linq;
using Ustas.RimAI.Actions.Core;
using Ustas.RimAI.Actions.Mod;
using UnityEngine;
using Verse;

namespace Ustas.RimAI.Actions.UI;

public static class EASettingsWindow
{
	private static Vector2 _scrollPosition;

	private static string _jobWhitelistBuffer;

	private static Dictionary<string, string> _keywordEditBuffers = new Dictionary<string, string>();

	private static HashSet<string> _expandedActions = new HashSet<string>();

	private const float UpperSettingsHeight = 620f;

	private const float ActionListHeight = 600f;

	public static void DoWindowContents(Rect inRect, EASettings settings)
	{
		List<ActionDefinition> list = ActionRegistry.GetAll().ToList();
		List<ActionCategory> list2 = (from c in list.Select((ActionDefinition a) => a.Category).Distinct()
			orderby c
			select c).ToList();
		int num = list.Count((ActionDefinition a) => _expandedActions.Contains(a.Id));
		int num2 = list2.Count * 30 + list.Count * 28 + num * 28 + 40;
		float height = 620f + (float)num2;
		Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, height);
		Widgets.BeginScrollView(inRect, ref _scrollPosition, viewRect);
		Listing_Standard listing_Standard = new Listing_Standard();
		// The height above is an estimate; if it ever comes up short, Verse would
		// wrap into an invisible second column rather than overflow the scroll view.
		listing_Standard.maxOneColumn = true;
		listing_Standard.Begin(new Rect(0f, 0f, viewRect.width, height));
		Text.Font = GameFont.Medium;
		listing_Standard.Label("EA_Settings_Title".Translate());
		Text.Font = GameFont.Small;
		listing_Standard.Gap();
		listing_Standard.CheckboxLabeled("EA_Settings_Enabled".Translate(), ref settings.Enabled, "EA_Settings_Enabled_Desc".Translate());
		listing_Standard.Gap();
		listing_Standard.CheckboxLabeled("EA_Settings_DebugMode".Translate(), ref settings.DebugMode, "EA_Settings_DebugMode_Desc".Translate());
		listing_Standard.CheckboxLabeled("EA_Settings_ShowBubbles".Translate(), ref settings.ShowExecutionBubbles, "EA_Settings_ShowBubbles_Desc".Translate());
		listing_Standard.Gap();
		listing_Standard.GapLine();
		listing_Standard.Label("EA_Settings_BehaviorControl".Translate());
		listing_Standard.Gap();
		listing_Standard.CheckboxLabeled("EA_Settings_SkipWorkTime".Translate(), ref settings.SkipWorkTimePawns, "EA_Settings_SkipWorkTime_Desc".Translate());
		listing_Standard.CheckboxLabeled("EA_Settings_SkipDrafted".Translate(), ref settings.SkipDraftedPawns, "EA_Settings_SkipDrafted_Desc".Translate());
		listing_Standard.CheckboxLabeled("EA_Settings_AllowUndesignated".Translate(), ref settings.AllowUndesignatedTargets, "EA_Settings_AllowUndesignated_Desc".Translate());
		listing_Standard.Gap();
		listing_Standard.Label("EA_Settings_JobProtection".Translate() + $": {settings.JobProtectionTicks / 60}s ({settings.JobProtectionTicks} ticks)");
		listing_Standard.Label("EA_Settings_JobProtection_Desc".Translate());
		settings.JobProtectionTicks = (int)listing_Standard.Slider(settings.JobProtectionTicks, 0f, 15000f);
		listing_Standard.Gap();
		listing_Standard.GapLine();
		listing_Standard.Label("EA_Settings_LLMSection".Translate());
		listing_Standard.Gap();
		listing_Standard.CheckboxLabeled("EA_Settings_UseSecondaryLLM".Translate(), ref settings.UseSecondaryLLM, "EA_Settings_UseSecondaryLLM_Desc".Translate());
		listing_Standard.Label("EA_Settings_LLMTimeout".Translate() + $": {settings.SecondaryLLMTimeout}s");
		settings.SecondaryLLMTimeout = (int)listing_Standard.Slider(settings.SecondaryLLMTimeout, 1f, 60f);
		listing_Standard.Label("EA_Settings_MaxActions".Translate() + $": {settings.MaxActionsPerConversation}");
		settings.MaxActionsPerConversation = (int)listing_Standard.Slider(settings.MaxActionsPerConversation, 1f, 20f);
		listing_Standard.Gap();
		listing_Standard.GapLine();
		listing_Standard.Label("EA_Settings_JobWhitelist".Translate());
		listing_Standard.Label("EA_Settings_JobWhitelist_Desc".Translate());
		if (_jobWhitelistBuffer == null)
		{
			_jobWhitelistBuffer = string.Join("\n", settings.CustomJobWhitelist);
		}
		_jobWhitelistBuffer = Widgets.TextArea(listing_Standard.GetRect(80f), _jobWhitelistBuffer);
		settings.CustomJobWhitelist = (from s in _jobWhitelistBuffer.Split(new[] { '\n' }, StringSplitOptions.None)
			select s.Trim() into s
			where !string.IsNullOrEmpty(s)
			select s).ToList();
		listing_Standard.Gap();
		listing_Standard.GapLine();
		listing_Standard.Label("EA_Settings_KeywordSection".Translate());
		listing_Standard.Gap();
		listing_Standard.CheckboxLabeled("EA_Settings_KeywordPreFilter".Translate(), ref settings.EnableKeywordPreFilter, "EA_Settings_KeywordPreFilter_Desc".Translate());
		listing_Standard.Gap();
		listing_Standard.GapLine();
		listing_Standard.Label("EA_Settings_CooldownSection".Translate());
		listing_Standard.Gap();
		listing_Standard.CheckboxLabeled("EA_Settings_Cooldown".Translate(), ref settings.EnableCooldown, "EA_Settings_Cooldown_Desc".Translate());
		listing_Standard.Label("EA_Settings_DefaultCooldown".Translate() + $": {settings.DefaultCooldownTicks / 60}s ({settings.DefaultCooldownTicks} ticks)");
		settings.DefaultCooldownTicks = (int)listing_Standard.Slider(settings.DefaultCooldownTicks, 0f, 60000f);
		listing_Standard.Label("EA_Settings_MovementCooldown".Translate() + $": {settings.MovementCooldownTicks / 60}s ({settings.MovementCooldownTicks} ticks)");
		settings.MovementCooldownTicks = (int)listing_Standard.Slider(settings.MovementCooldownTicks, 0f, 60000f);
		listing_Standard.Label("EA_Settings_CombatCooldown".Translate() + $": {settings.CombatCooldownTicks / 60}s ({settings.CombatCooldownTicks} ticks)");
		settings.CombatCooldownTicks = (int)listing_Standard.Slider(settings.CombatCooldownTicks, 0f, 60000f);
		listing_Standard.Label("EA_Settings_SocialCooldown".Translate() + $": {settings.SocialCooldownTicks / 60}s ({settings.SocialCooldownTicks} ticks)");
		settings.SocialCooldownTicks = (int)listing_Standard.Slider(settings.SocialCooldownTicks, 0f, 60000f);
		listing_Standard.Gap();
		listing_Standard.GapLine();
		string[] array = new string[3]
		{
			"EA_Settings_EffortLow".Translate().ToString(),
			"EA_Settings_EffortMedium".Translate().ToString(),
			"EA_Settings_EffortHigh".Translate().ToString()
		};
		listing_Standard.Label("EA_Settings_ActionEffort".Translate() + (": " + array[settings.ActionEffortLevel]));
		listing_Standard.Label("EA_Settings_ActionEffort_Desc".Translate());
		settings.ActionEffortLevel = (int)listing_Standard.Slider(settings.ActionEffortLevel, 0f, 2f);
		listing_Standard.Gap();
		listing_Standard.GapLine();
		listing_Standard.Label("EA_Settings_ActionConfig".Translate());
		listing_Standard.Gap();
		float num3 = listing_Standard.CurHeight;
		foreach (ActionCategory category in list2)
		{
			List<ActionDefinition> list3 = list.Where((ActionDefinition a) => a.Category == category).ToList();
			Rect rect = new Rect(0f, num3, viewRect.width, 26f);
			Text.Font = GameFont.Small;
			GUI.color = new Color(1f, 0.85f, 0.4f);
			Widgets.Label(rect, $"EA_Category_{category}".Translate());
			GUI.color = Color.white;
			num3 += 30f;
			foreach (ActionDefinition item in list3)
			{
				ActionConfig actionConfig = settings.GetActionConfig(item.Id);
				float x = 20f;
				bool flag = _expandedActions.Contains(item.Id);
				Rect rect2 = new Rect(x, num3, 56f, 24f);
				GUI.color = Color.gray;
				Widgets.Label(rect2, "toolcall");
				GUI.color = Color.white;
				x = rect2.xMax + 4f;
				Rect rect3 = new Rect(x, num3, 24f, 24f);
				Widgets.Checkbox(rect3.position, ref actionConfig.Enabled);
				x = rect3.xMax + 4f;
				Rect rect4 = new Rect(x, num3, 110f, 24f);
				Widgets.Label(rect4, item.DisplayName.Translate());
				x = rect4.xMax + 4f;
				List<string> promptKeywords = KeywordConfigManager.GetPromptKeywords(item.Id);
				string text = ((promptKeywords.Count > 0) ? string.Join(", ", promptKeywords) : "-");
				bool num4 = actionConfig.PromptKeywordOverrides != null && actionConfig.PromptKeywordOverrides.ContainsKey(KeywordConfigManager.GetCurrentLanguage());
				Rect rect5 = new Rect(x, num3, viewRect.width - x - 20f, 24f);
				GUI.color = (num4 ? new Color(0.5f, 1f, 0.5f) : new Color(0.7f, 0.7f, 0.7f));
				if (Widgets.ButtonText(rect5, (flag ? "▼ " : "▶ ") + text, drawBackground: false))
				{
					if (flag)
					{
						_expandedActions.Remove(item.Id);
					}
					else
					{
						_expandedActions.Add(item.Id);
					}
				}
				GUI.color = Color.white;
				num3 += 28f;
				if (!flag)
				{
					continue;
				}
				float x2 = 40f;
				string currentLanguage = KeywordConfigManager.GetCurrentLanguage();
				Rect rect6 = new Rect(x2, num3, 24f, 24f);
				Widgets.Checkbox(rect6.position, ref actionConfig.KeywordEnabled);
				x2 = rect6.xMax + 4f;
				Rect rect7 = new Rect(x2, num3, 30f, 24f);
				GUI.color = Color.gray;
				Widgets.Label(rect7, "[" + currentLanguage.Substring(0, Math.Min(2, currentLanguage.Length)) + "]");
				GUI.color = Color.white;
				x2 = rect7.xMax + 4f;
				if (!_keywordEditBuffers.ContainsKey(item.Id))
				{
					_keywordEditBuffers[item.Id] = string.Join(", ", promptKeywords);
				}
				float width = viewRect.width - x2 - 80f;
				Rect rect8 = new Rect(x2, num3, width, 24f);
				_keywordEditBuffers[item.Id] = Widgets.TextField(rect8, _keywordEditBuffers[item.Id]);
				x2 = rect8.xMax + 4f;
				Rect rect9 = new Rect(x2, num3, 30f, 24f);
				if (Widgets.ButtonText(rect9, "✓"))
				{
					if (actionConfig.PromptKeywordOverrides == null)
					{
						actionConfig.PromptKeywordOverrides = new Dictionary<string, string>();
					}
					actionConfig.PromptKeywordOverrides[currentLanguage] = _keywordEditBuffers[item.Id];
				}
				x2 = rect9.xMax + 4f;
				if (Widgets.ButtonText(new Rect(x2, num3, 30f, 24f), "↺"))
				{
					KeywordConfigManager.ResetToDefault(item.Id, currentLanguage);
					_keywordEditBuffers.Remove(item.Id);
				}
				num3 += 28f;
			}
		}
		listing_Standard.End();
		Widgets.EndScrollView();
	}
}
