using System.Collections.Generic;

namespace Ustas.RimAI.Actions.Core;

internal static class ActionRegistryCatalogMedical
{
	internal static void RegisterAll()
	{
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "rescue",
			Category = ActionCategory.Medical,
			DisplayName = "EA_Action_rescue",
			Description = "EA_Action_rescue_Desc",
			DefaultPromptDesc = "Rescue downed pawn to bed",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "rescue", "save", "carry", "救援", "营救", "搬运" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "tend",
			Category = ActionCategory.Medical,
			DisplayName = "EA_Action_tend",
			Description = "EA_Action_tend_Desc",
			DefaultPromptDesc = "Tend to injured pawn",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "tend", "heal", "treat", "治疗", "包扎", "医治" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "feed_patient",
			Category = ActionCategory.Medical,
			DisplayName = "EA_Action_feed_patient",
			Description = "EA_Action_feed_patient_Desc",
			DefaultPromptDesc = "Feed bedridden patient",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "feed", "food", "patient", "喂食", "喂饭", "病人" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "force_sleep",
			Category = ActionCategory.Medical,
			DisplayName = "EA_Action_force_sleep",
			Description = "EA_Action_force_sleep_Desc",
			DefaultPromptDesc = "Force pawn to rest",
			RequiredParams = new List<string> { "actor" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "sleep", "nap", "rest", "睡觉", "睡", "躺下", "休息", "歇" }
		});
	}
}
