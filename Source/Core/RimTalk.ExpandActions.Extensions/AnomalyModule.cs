using System.Collections.Generic;
using Ustas.RimAI.Actions.Core;
using Verse;

namespace Ustas.RimAI.Actions.Extensions;

public class AnomalyModule : IEAExtensionModule
{
	public string ModuleId => "anomaly";

	public string DisplayName => "EA_Module_Anomaly";

	public bool IsAvailable()
	{
		return ModsConfig.AnomalyActive;
	}

	public IEnumerable<ActionDefinition> GetActions()
	{
		yield return new ActionDefinition
		{
			Id = "study_entity",
			Category = ActionCategory.DLC,
			DisplayName = "EA_Action_study_entity",
			Description = "EA_Action_study_entity_Desc",
			DefaultPromptDesc = "Study anomalous entity",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Medium,
			DefaultEnabled = true,
			Handler = null,
			Keywords = new List<string> { "study", "anomaly", "entity", "research", "研究", "异常", "实体" }
		};
		yield return new ActionDefinition
		{
			Id = "suppress_entity",
			Category = ActionCategory.DLC,
			DisplayName = "EA_Action_suppress_entity",
			Description = "EA_Action_suppress_entity_Desc",
			DefaultPromptDesc = "Suppress anomalous entity to prevent escape",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Medium,
			DefaultEnabled = true,
			Handler = null,
			Keywords = new List<string> { "suppress", "contain", "anomaly", "压制", "抑制", "遏制" }
		};
	}

	public IEnumerable<string> GetJobWhitelistEntries()
	{
		yield return "StudyInteract";
		yield return "ActivitySuppression";
	}

	public IEnumerable<IEAVariableContributor> GetVariableContributors()
	{
		yield break;
	}
}
