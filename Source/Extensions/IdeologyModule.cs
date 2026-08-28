using System.Collections.Generic;
using Ustas.RimAI.Actions.Core;
using Verse;

namespace Ustas.RimAI.Actions.Extensions;

public class IdeologyModule : IEAExtensionModule
{
	public string ModuleId => "ideology";

	public string DisplayName => "EA_Module_Ideology";

	public bool IsAvailable()
	{
		return ModsConfig.IdeologyActive;
	}

	public IEnumerable<ActionDefinition> GetActions()
	{
		yield return new ActionDefinition
		{
			Id = "ritual",
			Category = ActionCategory.DLC,
			DisplayName = "EA_Action_ritual",
			Description = "EA_Action_ritual_Desc",
			DefaultPromptDesc = "Start a ritual (args.ritual_type: ritual name)",
			RequiredParams = new List<string> { "actor", "args.ritual_type" },
			RiskLevel = RiskLevel.Medium,
			DefaultEnabled = false,
			Handler = null,
			Keywords = new List<string> { "ritual", "ceremony", "ритуал", "церемонія", "обряд" }
		};
		yield return new ActionDefinition
		{
			Id = "convert",
			Category = ActionCategory.DLC,
			DisplayName = "EA_Action_convert",
			Description = "EA_Action_convert_Desc",
			DefaultPromptDesc = "Attempt to convert target to your ideology",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Medium,
			DefaultEnabled = true,
			Handler = null,
			Keywords = new List<string> { "convert", "ideology", "belief", "навернути", "віра", "переконання" }
		};
	}

	public IEnumerable<string> GetJobWhitelistEntries()
	{
		yield break;
	}

	public IEnumerable<IEAVariableContributor> GetVariableContributors()
	{
		yield break;
	}
}
