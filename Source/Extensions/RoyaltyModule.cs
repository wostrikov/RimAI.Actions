using System.Collections.Generic;
using Ustas.RimAI.Actions.Core;
using Verse;

namespace Ustas.RimAI.Actions.Extensions;

public class RoyaltyModule : IEAExtensionModule
{
	public string ModuleId => "royalty";

	public string DisplayName => "EA_Module_Royalty";

	public bool IsAvailable()
	{
		return ModsConfig.RoyaltyActive;
	}

	public IEnumerable<ActionDefinition> GetActions()
	{
		yield return new ActionDefinition
		{
			Id = "meditate",
			Category = ActionCategory.DLC,
			DisplayName = "EA_Action_meditate",
			Description = "EA_Action_meditate_Desc",
			DefaultPromptDesc = "Meditate at meditation spot or throne",
			RequiredParams = new List<string> { "actor" },
			RiskLevel = RiskLevel.Low,
			DefaultEnabled = true,
			Handler = null,
			Keywords = new List<string> { "meditate", "meditation", "psyfocus", "медитація", "Псіфокус" }
		};
		yield return new ActionDefinition
		{
			Id = "psycast",
			Category = ActionCategory.DLC,
			DisplayName = "EA_Action_psycast",
			Description = "EA_Action_psycast_Desc",
			DefaultPromptDesc = "Use psycast ability on target (args.ability: ability name)",
			RequiredParams = new List<string> { "actor", "target", "args.ability" },
			RiskLevel = RiskLevel.Medium,
			DefaultEnabled = true,
			Handler = null,
			Keywords = new List<string> { "psycast", "ability", "psychic", "псіоніка", "надздібності", "розум" }
		};
		yield return new ActionDefinition
		{
			Id = "reign",
			Category = ActionCategory.DLC,
			DisplayName = "EA_Action_reign",
			Description = "EA_Action_reign_Desc",
			DefaultPromptDesc = "Sit on throne to fulfill reign duty",
			RequiredParams = new List<string> { "actor" },
			RiskLevel = RiskLevel.Low,
			DefaultEnabled = true,
			Handler = null,
			Keywords = new List<string> { "reign", "throne", "royal", "правління", "трон", "королівський" }
		};
	}

	public IEnumerable<string> GetJobWhitelistEntries()
	{
		yield return "Meditate";
		yield return "UseAbility";
		yield return "Reign";
	}

	public IEnumerable<IEAVariableContributor> GetVariableContributors()
	{
		yield break;
	}
}
