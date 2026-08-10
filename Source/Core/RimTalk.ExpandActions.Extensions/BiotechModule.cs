using System.Collections.Generic;
using RimTalk.ExpandActions.Actions.DLC;
using RimTalk.ExpandActions.Core;
using Verse;

namespace RimTalk.ExpandActions.Extensions;

public class BiotechModule : IEAExtensionModule
{
	public string ModuleId => "biotech";

	public string DisplayName => "EA_Module_Biotech";

	public bool IsAvailable()
	{
		return ModsConfig.BiotechActive;
	}

	public IEnumerable<ActionDefinition> GetActions()
	{
		yield return new ActionDefinition
		{
			Id = "command_mechanoid",
			Category = ActionCategory.DLC,
			DisplayName = "EA_Action_command_mechanoid",
			Description = "EA_Action_command_mechanoid_Desc",
			DefaultPromptDesc = "Command mechanoid to perform action (args.command: go_to/attack/follow)",
			RequiredParams = new List<string> { "actor", "target", "args.command" },
			RiskLevel = RiskLevel.Medium,
			DefaultEnabled = true,
			Handler = new CommandMechanoidHandler(),
			Keywords = new List<string> { "mechanoid", "mech", "command", "机械体", "机械族", "命令" }
		};
		yield return new ActionDefinition
		{
			Id = "enter_biosculpter",
			Category = ActionCategory.DLC,
			DisplayName = "EA_Action_enter_biosculpter",
			Description = "EA_Action_enter_biosculpter_Desc",
			DefaultPromptDesc = "Enter biosculpter pod for enhancement",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Low,
			DefaultEnabled = true,
			Handler = new EnterBiosculpterHandler(),
			Keywords = new List<string> { "biosculpter", "pod", "sculpt", "生物塑型", "塑型舱" }
		};
	}

	public IEnumerable<string> GetJobWhitelistEntries()
	{
		yield return "MechCommand";
		yield return "EnterBiosculpterPod";
	}

	public IEnumerable<IEAVariableContributor> GetVariableContributors()
	{
		yield break;
	}
}
