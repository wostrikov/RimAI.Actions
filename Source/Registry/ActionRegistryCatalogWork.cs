using System.Collections.Generic;

namespace Ustas.RimAI.Actions.Core;

internal static class ActionRegistryCatalogWork
{
	internal static void RegisterAll()
	{
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "job_start",
			Category = ActionCategory.Job,
			DisplayName = "EA_Action_job_start",
			Description = "EA_Action_job_start_Desc",
			DefaultPromptDesc = "Start a specific job (job: JobDef name)",
			RequiredParams = new List<string> { "actor", "job" },
			OptionalParams = new List<string> { "target", "cell" },
			RiskLevel = RiskLevel.Medium,
			DefaultEnabled = false,
			Handler = null,
			Keywords = new List<string> { "job", "work", "task", "завдання", "робота" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "job_queue_front",
			Category = ActionCategory.Job,
			DisplayName = "EA_Action_job_queue_front",
			Description = "EA_Action_job_queue_front_Desc",
			DefaultPromptDesc = "Queue job at front (job: JobDef name)",
			RequiredParams = new List<string> { "actor", "job" },
			OptionalParams = new List<string> { "target", "cell" },
			RiskLevel = RiskLevel.Medium,
			DefaultEnabled = false,
			Handler = null,
			Keywords = new List<string> { "queue", "черга", "послідовність" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "job_end",
			Category = ActionCategory.Job,
			DisplayName = "EA_Action_job_end",
			Description = "EA_Action_job_end_Desc",
			DefaultPromptDesc = "End current job",
			RequiredParams = new List<string> { "actor" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "end job", "cancel", "Завершити завдання", "скасувати" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "haul",
			Category = ActionCategory.Production,
			DisplayName = "EA_Action_haul",
			Description = "EA_Action_haul_Desc",
			DefaultPromptDesc = "Haul item to stockpile",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "haul", "carry", "переносити", "неси", "перевезення", "вези", "тягни" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "mine",
			Category = ActionCategory.Production,
			DisplayName = "EA_Action_mine",
			Description = "EA_Action_mine_Desc",
			DefaultPromptDesc = "Mine rock or mineral",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "mine", "dig", "видобуток", "копати", "копай" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "cut_plant",
			Category = ActionCategory.Production,
			DisplayName = "EA_Action_cut_plant",
			Description = "EA_Action_cut_plant_Desc",
			DefaultPromptDesc = "Cut plant",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "cut", "chop", "plant", "рубай", "рубати", "рослини" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "clean",
			Category = ActionCategory.Production,
			DisplayName = "EA_Action_clean",
			Description = "EA_Action_clean_Desc",
			DefaultPromptDesc = "Clean area",
			RequiredParams = new List<string> { "actor" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "clean", "sweep", "прибирання", "прибрати", "підмести" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "repair",
			Category = ActionCategory.Production,
			DisplayName = "EA_Action_repair",
			Description = "EA_Action_repair_Desc",
			DefaultPromptDesc = "Repair building or structure",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "repair", "fix", "ремонт", "полагодити" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "deconstruct",
			Category = ActionCategory.Production,
			DisplayName = "EA_Action_deconstruct",
			Description = "EA_Action_deconstruct_Desc",
			DefaultPromptDesc = "Deconstruct building",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Medium,
			Handler = null,
			Keywords = new List<string> { "deconstruct", "demolish", "розібрати", "ламай" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "sow",
			Category = ActionCategory.Production,
			DisplayName = "EA_Action_sow",
			Description = "EA_Action_sow_Desc",
			DefaultPromptDesc = "Sow seeds in growing zone",
			RequiredParams = new List<string> { "actor" },
			OptionalParams = new List<string> { "target", "cell" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "sow", "plant", "seed", "сіяти", "садити" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "harvest",
			Category = ActionCategory.Production,
			DisplayName = "EA_Action_harvest",
			Description = "EA_Action_harvest_Desc",
			DefaultPromptDesc = "Harvest mature plant",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "harvest", "reap", "жнива", "збирати врожай" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "research",
			Category = ActionCategory.Production,
			DisplayName = "EA_Action_research",
			Description = "EA_Action_research_Desc",
			DefaultPromptDesc = "Research at research bench",
			RequiredParams = new List<string> { "actor" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "research", "study", "дослідження", "наука" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "craft",
			Category = ActionCategory.Production,
			DisplayName = "EA_Action_craft",
			Description = "EA_Action_craft_Desc",
			DefaultPromptDesc = "Work at crafting bench",
			RequiredParams = new List<string> { "actor" },
			OptionalParams = new List<string> { "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "craft", "make", "build", "виготовити", "виробництво", "скувати" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "smooth_floor",
			Category = ActionCategory.Production,
			DisplayName = "EA_Action_smooth_floor",
			Description = "EA_Action_smooth_floor_Desc",
			DefaultPromptDesc = "Smooth rough floor",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "smooth", "floor", "полірувати", "підлога" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "build_roof",
			Category = ActionCategory.Production,
			DisplayName = "EA_Action_build_roof",
			Description = "EA_Action_build_roof_Desc",
			DefaultPromptDesc = "Build roof in area",
			RequiredParams = new List<string> { "cell" },
			OptionalParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "roof", "cover", "дах", "Збудувати дах" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "remove_roof",
			Category = ActionCategory.Production,
			DisplayName = "EA_Action_remove_roof",
			Description = "EA_Action_remove_roof_Desc",
			DefaultPromptDesc = "Remove roof from area",
			RequiredParams = new List<string> { "cell" },
			OptionalParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "remove roof", "Розібрати дах" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "uninstall",
			Category = ActionCategory.Production,
			DisplayName = "EA_Action_uninstall",
			Description = "EA_Action_uninstall_Desc",
			DefaultPromptDesc = "Uninstall building or furniture",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Medium,
			Handler = null,
			Keywords = new List<string> { "uninstall", "remove", "розвантажити", "демонтаж" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "fix_broken",
			Category = ActionCategory.Production,
			DisplayName = "EA_Action_fix_broken",
			Description = "EA_Action_fix_broken_Desc",
			DefaultPromptDesc = "Fix broken-down building",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "fix", "broken", "полагодити", "несправність" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "refuel",
			Category = ActionCategory.Production,
			DisplayName = "EA_Action_refuel",
			Description = "EA_Action_refuel_Desc",
			DefaultPromptDesc = "Refuel building",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "refuel", "fuel", "заправити", "паливо" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "flick",
			Category = ActionCategory.Production,
			DisplayName = "EA_Action_flick",
			Description = "EA_Action_flick_Desc",
			DefaultPromptDesc = "Toggle power switch on building",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "flick", "switch", "toggle", "вимикач", "перемкнути" }
		});
	}
}
