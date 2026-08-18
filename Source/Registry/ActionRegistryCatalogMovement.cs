using System.Collections.Generic;

namespace Ustas.RimAI.Actions.Core;

internal static class ActionRegistryCatalogMovement
{
	internal static void RegisterAll()
	{
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "move_to",
			Category = ActionCategory.Movement,
			DisplayName = "EA_Action_move_to",
			Description = "EA_Action_move_to_Desc",
			DefaultPromptDesc = "Move to target location",
			RequiredParams = new List<string> { "actor" },
			OptionalParams = new List<string> { "target", "cell" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "move", "go", "walk", "走", "移动", "前往", "过去" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "stop",
			Category = ActionCategory.Movement,
			DisplayName = "EA_Action_stop",
			Description = "EA_Action_stop_Desc",
			DefaultPromptDesc = "Immediately stop current action",
			RequiredParams = new List<string> { "actor" },
			RiskLevel = RiskLevel.Low,
			DefaultEnabled = false,
			Handler = null,
			Keywords = new List<string> { "stop", "halt", "cease", "停", "停止", "停下" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "wait",
			Category = ActionCategory.Movement,
			DisplayName = "EA_Action_wait",
			Description = "EA_Action_wait_Desc",
			DefaultPromptDesc = "Wait at current location for specified ticks",
			RequiredParams = new List<string> { "actor" },
			OptionalParams = new List<string> { "args.ticks" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "wait", "stay", "stand", "等", "等待", "待命", "站立", "站" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "follow",
			Category = ActionCategory.Movement,
			DisplayName = "EA_Action_follow",
			Description = "EA_Action_follow_Desc",
			DefaultPromptDesc = "Follow target pawn",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "follow", "跟随", "跟着", "跟上" }
		});
		ActionRegistry.Register(new ActionDefinition
		{
			Id = "flee",
			Category = ActionCategory.Movement,
			DisplayName = "EA_Action_flee",
			Description = "EA_Action_flee_Desc",
			DefaultPromptDesc = "Flee from danger or target",
			RequiredParams = new List<string> { "actor" },
			OptionalParams = new List<string> { "target" },
			RiskLevel = RiskLevel.Low,
			Handler = null,
			Keywords = new List<string> { "flee", "run", "escape", "逃", "逃跑", "逃离" }
		});
	}
}
