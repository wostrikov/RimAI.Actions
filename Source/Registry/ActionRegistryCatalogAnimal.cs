using System.Collections.Generic;

namespace Ustas.RimAI.Actions.Core;

internal static class ActionRegistryCatalogAnimal
{
	internal static void RegisterAll()
	{
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "tame",
			Category = ActionCategory.Animal,
			DisplayName = "EA_Action_tame",
			Description = "EA_Action_tame_Desc",
			DefaultPromptDesc = "Attempt to tame animal",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "tame", "domesticate", "приручити", "одомашнити" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "train",
			Category = ActionCategory.Animal,
			DisplayName = "EA_Action_train",
			Description = "EA_Action_train_Desc",
			DefaultPromptDesc = "Train animal",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "train", "тренувати", "навчати" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "hunt",
			Category = ActionCategory.Animal,
			DisplayName = "EA_Action_hunt",
			Description = "EA_Action_hunt_Desc",
			DefaultPromptDesc = "Hunt animal",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Medium,
			Handler = null,
			Keywords = new List<string> { "hunt", "полювання", "вполювати", "полювати" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "slaughter",
			Category = ActionCategory.Animal,
			DisplayName = "EA_Action_slaughter",
			Description = "EA_Action_slaughter_Desc",
			DefaultPromptDesc = "Slaughter animal",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Medium,
			Handler = null,
			Keywords = new List<string> { "slaughter", "butcher", "забій", "зарізати" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "milk",
			Category = ActionCategory.Animal,
			DisplayName = "EA_Action_milk",
			Description = "EA_Action_milk_Desc",
			DefaultPromptDesc = "Milk animal",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "milk", "доїти" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "shear",
			Category = ActionCategory.Animal,
			DisplayName = "EA_Action_shear",
			Description = "EA_Action_shear_Desc",
			DefaultPromptDesc = "Shear animal",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "shear", "wool", "стригти" }
		});
	}
}
