using System.Collections.Generic;

namespace Ustas.RimAI.Actions.Core;

internal static class ActionRegistryCatalogCombat
{
	internal static void RegisterAll()
	{
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "draft_set",
			Category = ActionCategory.Combat,
			DisplayName = "EA_Action_draft_set",
			Description = "EA_Action_draft_set_Desc",
			DefaultPromptDesc = "Set pawn draft status (args.drafted: true/false)",
			RequiredParams = new List<string> { "actor" },
			OptionalParams = new List<string> { "args.drafted" },
			RiskLevel = RiskLevel.Medium,
			Handler = null,
			Keywords = new List<string> { "draft", "undraft", "征召", "解除征召", "备战" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "attack_melee",
			Category = ActionCategory.Combat,
			DisplayName = "EA_Action_attack_melee",
			Description = "EA_Action_attack_melee_Desc",
			DefaultPromptDesc = "Attack target with melee weapon",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Medium,
			Handler = null,
			Keywords = new List<string> { "attack", "melee", "hit", "punch", "近战", "攻击", "打" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "attack_ranged",
			Category = ActionCategory.Combat,
			DisplayName = "EA_Action_attack_ranged",
			Description = "EA_Action_attack_ranged_Desc",
			DefaultPromptDesc = "Attack target with ranged weapon",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Medium,
			Handler = null,
			Keywords = new List<string> { "shoot", "ranged", "fire", "射击", "远程", "开枪", "射" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "arrest",
			Category = ActionCategory.Combat,
			DisplayName = "EA_Action_arrest",
			Description = "EA_Action_arrest_Desc",
			DefaultPromptDesc = "Attempt to arrest target pawn",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Medium,
			Handler = null,
			Keywords = new List<string> { "arrest", "逮捕", "抓捕" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "drop_weapon",
			Category = ActionCategory.Combat,
			DisplayName = "EA_Action_drop_weapon",
			Description = "EA_Action_drop_weapon_Desc",
			DefaultPromptDesc = "Drop currently equipped weapon",
			RequiredParams = new List<string> { "actor" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "drop weapon", "disarm", "丢弃武器", "放下武器" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "mental_break_start",
			Category = ActionCategory.Combat,
			DisplayName = "EA_Action_mental_break_start",
			Description = "EA_Action_mental_break_start_Desc",
			DefaultPromptDesc = "Trigger mental state (args.state: MentalStateDef name)",
			RequiredParams = new List<string> { "actor", "args.state" },
			RiskLevel = RiskLevel.High,
			DefaultEnabled = false,
			Handler = null,
			Keywords = new List<string> { "mental", "break", "berserk", "精神崩溃", "发狂" }
		});
	}
}
