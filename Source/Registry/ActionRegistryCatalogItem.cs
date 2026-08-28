using System.Collections.Generic;

namespace Ustas.RimAI.Actions.Core;

internal static class ActionRegistryCatalogItem
{
	internal static void RegisterAll()
	{
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "take_inventory",
			Category = ActionCategory.Item,
			DisplayName = "EA_Action_take_inventory",
			Description = "EA_Action_take_inventory_Desc",
			DefaultPromptDesc = "Pick up an item from the ground into personal inventory (thing; args.quantity, partial stacks are allowed)",
			RequiredParams = new List<string> { "actor", "thing" },
			OptionalParams = new List<string> { "target", "args.quantity" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "take inventory", "pick up", "pickup", "put in pocket", "візьми", "підійми", "підбери", "у кишеню", "інвентар", "підняти", "Покласти в рюкзак" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "drop_item",
			Category = ActionCategory.Item,
			DisplayName = "EA_Action_drop_item",
			Description = "EA_Action_drop_item_Desc",
			DefaultPromptDesc = "Drop held item",
			RequiredParams = new List<string> { "actor" },
			OptionalParams = new List<string> { "thing" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "drop", "викинути", "покласти", "кинь" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "give_item",
			Category = ActionCategory.Item,
			DisplayName = "EA_Action_give_item",
			Description = "EA_Action_give_item_Desc",
			DefaultPromptDesc = "Give item to target pawn",
			RequiredParams = new List<string> { "actor", "target" },
			OptionalParams = new List<string> { "thing" },
			RiskLevel = RiskLevel.Medium,
			Handler = null,
			Keywords = new List<string> { "give", "hand", "подарувати", "дай", "передати" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "equip",
			Category = ActionCategory.Item,
			DisplayName = "EA_Action_equip",
			Description = "EA_Action_equip_Desc",
			DefaultPromptDesc = "Equip weapon or apparel",
			RequiredParams = new List<string> { "actor", "thing" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "equip", "wear", "спорядження", "вдягнути", "озброїти" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "force_eat",
			Category = ActionCategory.Item,
			DisplayName = "EA_Action_force_eat",
			Description = "EA_Action_force_eat_Desc",
			DefaultPromptDesc = "Force pawn to eat food",
			RequiredParams = new List<string> { "actor" },
			OptionalParams = new List<string> { "thing" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "eat", "food", "їж", "їсти", "поїсти", "трапеза" }
		});
	}
}
