using System.Collections.Generic;

namespace Ustas.RimAI.Actions.Core;

internal static class ActionRegistryCatalogFacility
{
	internal static void RegisterAll()
	{
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "use_comms",
			Category = ActionCategory.Facility,
			DisplayName = "EA_Action_use_comms",
			Description = "EA_Action_use_comms_Desc",
			DefaultPromptDesc = "Use communications console",
			RequiredParams = new List<string> { "actor" },
			OptionalParams = new List<string> { "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "comms", "communicate", "radio", "звʼязок", "контакт" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "trade",
			Category = ActionCategory.Facility,
			DisplayName = "EA_Action_trade",
			Description = "EA_Action_trade_Desc",
			DefaultPromptDesc = "Trade with visitor or caravan",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Medium,
			DefaultEnabled = false,
			Handler = null,
			Keywords = new List<string> { "trade", "buy", "sell", "торгівля", "купівля-продаж", "торг" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "enter_cryptosleep",
			Category = ActionCategory.Facility,
			DisplayName = "EA_Action_enter_cryptosleep",
			Description = "EA_Action_enter_cryptosleep_Desc",
			DefaultPromptDesc = "Enter cryptosleep casket",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Medium,
			Handler = null,
			Keywords = new List<string> { "cryptosleep", "casket", "hibernate", "Кріосон", "сплячка" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "reload",
			Category = ActionCategory.Facility,
			DisplayName = "EA_Action_reload",
			Description = "EA_Action_reload_Desc",
			DefaultPromptDesc = "Reload weapon or turret",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			DefaultEnabled = false,
			Handler = null,
			Keywords = new List<string> { "reload", "ammo", "зарядити", "боєприпаси" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "open_container",
			Category = ActionCategory.Facility,
			DisplayName = "EA_Action_open_container",
			Description = "EA_Action_open_container_Desc",
			DefaultPromptDesc = "Open container or casket",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "open", "container", "відкрити", "контейнер" }
		});
	}
}
