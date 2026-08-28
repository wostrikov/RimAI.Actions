using System.Collections.Generic;

namespace Ustas.RimAI.Actions.Core;

internal static class ActionRegistryCatalogPrisoner
{
	internal static void RegisterAll()
	{
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "capture",
			Category = ActionCategory.Prisoner,
			DisplayName = "EA_Action_capture",
			Description = "EA_Action_capture_Desc",
			DefaultPromptDesc = "Capture downed pawn as prisoner",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Medium,
			Handler = null,
			Keywords = new List<string> { "capture", "prisoner", "вхопити", "полонений", "захопити" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "release_prisoner",
			Category = ActionCategory.Prisoner,
			DisplayName = "EA_Action_release_prisoner",
			Description = "EA_Action_release_prisoner_Desc",
			DefaultPromptDesc = "Release prisoner",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Medium,
			Handler = null,
			Keywords = new List<string> { "release", "free", "звільнити", "відпустити" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "recruit_prisoner",
			Category = ActionCategory.Prisoner,
			DisplayName = "EA_Action_recruit_prisoner",
			Description = "EA_Action_recruit_prisoner_Desc",
			DefaultPromptDesc = "Attempt to recruit prisoner",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Medium,
			Handler = null,
			Keywords = new List<string> { "recruit prisoner", "Завербувати бранця", "Переконати бранця" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "execute",
			Category = ActionCategory.Prisoner,
			DisplayName = "EA_Action_execute",
			Description = "EA_Action_execute_Desc",
			DefaultPromptDesc = "Execute prisoner",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.High,
			DefaultEnabled = false,
			Handler = null,
			Keywords = new List<string> { "execute", "kill prisoner", "стратити", "страта" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "escort_to_bed",
			Category = ActionCategory.Prisoner,
			DisplayName = "EA_Action_escort_to_bed",
			Description = "EA_Action_escort_to_bed_Desc",
			DefaultPromptDesc = "Escort prisoner to bed",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "escort", "bed", "конвоювати", "привести" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "strip",
			Category = ActionCategory.Prisoner,
			DisplayName = "EA_Action_strip",
			Description = "EA_Action_strip_Desc",
			DefaultPromptDesc = "Strip apparel from target",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Medium,
			Handler = null,
			Keywords = new List<string> { "strip", "undress", "зняти", "роздягти" }
		});
	}
}
