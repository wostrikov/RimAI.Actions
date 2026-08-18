using System.Collections.Generic;

namespace Ustas.RimAI.Actions.Core;

internal static class ActionRegistryCatalogSocial
{
	internal static void RegisterAll()
	{
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "recruit",
			Category = ActionCategory.Social,
			DisplayName = "EA_Action_recruit",
			Description = "EA_Action_recruit_Desc",
			DefaultPromptDesc = "Attempt to recruit prisoner",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Medium,
			Handler = null,
			Keywords = new List<string> { "recruit", "persuade", "招募", "说服" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "romance_set",
			Category = ActionCategory.Social,
			DisplayName = "EA_Action_romance_set",
			Description = "EA_Action_romance_set_Desc",
			DefaultPromptDesc = "Set romantic relationship (args.mode: new_lover/breakup)",
			RequiredParams = new List<string> { "actor", "target", "args.mode" },
			RiskLevel = RiskLevel.High,
			DefaultEnabled = false,
			Handler = null,
			Keywords = new List<string> { "romance", "lover", "breakup", "恋爱", "分手", "告白" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "thought_add",
			Category = ActionCategory.Social,
			DisplayName = "EA_Action_thought_add",
			Description = "EA_Action_thought_add_Desc",
			DefaultPromptDesc = "Add memory thought (args.thought: ThoughtDef name)",
			RequiredParams = new List<string> { "actor", "args.thought" },
			RiskLevel = RiskLevel.Medium,
			Handler = null,
			Keywords = new List<string> { "thought", "memory", "mood", "思绪", "心情", "记忆" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "relation_set",
			Category = ActionCategory.Social,
			DisplayName = "EA_Action_relation_set",
			Description = "EA_Action_relation_set_Desc",
			DefaultPromptDesc = "Modify pawn relationship (args.relation, args.mode: add/remove)",
			RequiredParams = new List<string> { "actor", "target", "args.relation", "args.mode" },
			RiskLevel = RiskLevel.High,
			DefaultEnabled = false,
			Handler = null,
			Keywords = new List<string> { "relation", "friend", "enemy", "关系", "朋友", "敌人" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "give_inspiration",
			Category = ActionCategory.Social,
			DisplayName = "EA_Action_give_inspiration",
			Description = "EA_Action_give_inspiration_Desc",
			DefaultPromptDesc = "Grant inspiration (args.type: InspirationDef name)",
			RequiredParams = new List<string> { "actor", "args.type" },
			RiskLevel = RiskLevel.Medium,
			Handler = null,
			Keywords = new List<string> { "inspiration", "inspire", "灵感", "激励" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "visit_sick",
			Category = ActionCategory.Social,
			DisplayName = "EA_Action_visit_sick",
			Description = "EA_Action_visit_sick_Desc",
			DefaultPromptDesc = "Visit sick or bedridden pawn",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "visit", "sick", "ill", "探病", "看望", "探望" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "social_fight",
			Category = ActionCategory.Social,
			DisplayName = "EA_Action_social_fight",
			Description = "EA_Action_social_fight_Desc",
			DefaultPromptDesc = "Start a social fight with target",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Medium,
			DefaultEnabled = false,
			Handler = null,
			Keywords = new List<string> { "fight", "brawl", "social fight", "打架", "吵架", "斗殴" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "lovin",
			Category = ActionCategory.Social,
			DisplayName = "EA_Action_lovin",
			Description = "EA_Action_lovin_Desc",
			DefaultPromptDesc = "Lovin' with partner in bed",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "lovin", "亲热", "恩爱" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "insult",
			Category = ActionCategory.Social,
			DisplayName = "EA_Action_insult",
			Description = "EA_Action_insult_Desc",
			DefaultPromptDesc = "Insult target pawn",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Medium,
			DefaultEnabled = false,
			Handler = null,
			Keywords = new List<string> { "insult", "mock", "侮辱", "嘲笑", "骂" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "marry",
			Category = ActionCategory.Social,
			DisplayName = "EA_Action_marry",
			Description = "EA_Action_marry_Desc",
			DefaultPromptDesc = "Marry target pawn (ceremony)",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.High,
			DefaultEnabled = false,
			Handler = null,
			Keywords = new List<string> { "marry", "wedding", "marriage", "结婚", "婚礼" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "spectate",
			Category = ActionCategory.Social,
			DisplayName = "EA_Action_spectate",
			Description = "EA_Action_spectate_Desc",
			DefaultPromptDesc = "Watch spectacle or event",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "spectate", "watch", "observe", "观看", "观赏" }
		});
	}
}
