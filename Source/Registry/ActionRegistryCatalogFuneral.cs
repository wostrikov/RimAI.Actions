using System.Collections.Generic;

namespace Ustas.RimAI.Actions.Core;

internal static class ActionRegistryCatalogFuneral
{
	internal static void RegisterAll()
	{
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "bury",
			Category = ActionCategory.Funeral,
			DisplayName = "EA_Action_bury",
			Description = "EA_Action_bury_Desc",
			DefaultPromptDesc = "Bury corpse in grave",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "bury", "grave", "funeral", "埋葬", "坟墓", "葬礼" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "cremate",
			Category = ActionCategory.Funeral,
			DisplayName = "EA_Action_cremate",
			Description = "EA_Action_cremate_Desc",
			DefaultPromptDesc = "Cremate corpse at crematorium",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "cremate", "burn", "火化", "焚烧" }
		});
	}
}
