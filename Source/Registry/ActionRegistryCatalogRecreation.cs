using System.Collections.Generic;

namespace Ustas.RimAI.Actions.Core;

internal static class ActionRegistryCatalogRecreation
{
	internal static void RegisterAll()
	{
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "stargaze",
			Category = ActionCategory.Recreation,
			DisplayName = "EA_Action_stargaze",
			Description = "EA_Action_stargaze_Desc",
			DefaultPromptDesc = "Go stargazing outdoors",
			RequiredParams = new List<string> { "actor" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "stargaze", "stars", "sky", "дивитися на зорі", "дивитися на зорі" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "go_for_walk",
			Category = ActionCategory.Recreation,
			DisplayName = "EA_Action_go_for_walk",
			Description = "EA_Action_go_for_walk_Desc",
			DefaultPromptDesc = "Take a leisurely walk",
			RequiredParams = new List<string> { "actor" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "walk", "stroll", "wander", "прогулянка", "тинятися", "гуляй", "блукати" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "play_music",
			Category = ActionCategory.Recreation,
			DisplayName = "EA_Action_play_music",
			Description = "EA_Action_play_music_Desc",
			DefaultPromptDesc = "Play musical instrument",
			RequiredParams = new List<string> { "actor" },
			OptionalParams = new List<string> { "target" },
			RiskLevel = RiskLevel.Low,
			DefaultEnabled = false,
			Handler = null,
			Keywords = new List<string> { "music", "play", "instrument", "музика", "грати", "інструмент" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "use_drug",
			Category = ActionCategory.Recreation,
			DisplayName = "EA_Action_use_drug",
			Description = "EA_Action_use_drug_Desc",
			DefaultPromptDesc = "Use drug or consumable",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Medium,
			Handler = null,
			Keywords = new List<string> { "drug", "smoke", "drink", "ліки", "наркотик", "вживати" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "view_art",
			Category = ActionCategory.Recreation,
			DisplayName = "EA_Action_view_art",
			Description = "EA_Action_view_art_Desc",
			DefaultPromptDesc = "View art piece for joy",
			RequiredParams = new List<string> { "actor" },
			OptionalParams = new List<string> { "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "art", "sculpture", "painting", "мистецтво", "скульптура", "милуватися" }
		});
	}
}
