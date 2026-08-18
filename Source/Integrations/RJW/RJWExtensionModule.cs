using System;
using System.Collections.Generic;
using System.Reflection;
using Ustas.RimAI.Actions.Core;
using Ustas.RimAI.Actions.RJW.Handlers;
using Ustas.RimAI.Actions.RJW.Handlers.RJWHighRisk;
using Ustas.RimAI.Actions.RJW.State;
using Ustas.RimAI.Core.Diagnostics;
using Verse;

namespace Ustas.RimAI.Actions.RJW;

public class RJWExtensionModule : IEAExtensionModule
{
	public string ModuleId => "rjw";

	public string DisplayName => "EA_Module_RJW";

	public bool IsAvailable()
	{
		try
		{
			if (ModLister.GetActiveModWithIdentifier("rim.job.world") != null)
			{
				return true;
			}
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				try
				{
					if (assembly.GetType("rjw.SexUtility") != null)
					{
						return true;
					}
				}
				catch (Exception ex)
				{
					RimAiLog.Warning(RimAiLogCategory.Actions, "[EA-RJW] IsAvailable skipped assembly " + assembly.GetName().Name, ex);
				}
			}
		}
		catch (Exception ex2)
		{
			RimAiLog.Error(RimAiLogCategory.Actions, "[EA-RJW] IsAvailable check failed", ex2);
		}
		return false;
	}

	public IEnumerable<ActionDefinition> GetActions()
	{
		yield return new ActionDefinition
		{
			Id = "rjw_sex",
			Category = ActionCategory.RJW,
			DisplayName = "EA_Action_rjw_sex",
			Description = "EA_Action_rjw_sex_Desc",
			DefaultPromptDesc = "Consensual sex with partner in bed",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Medium,
			DefaultEnabled = true,
			Handler = new RJWSexHandler(),
			Keywords = new List<string> { "sex", "lovin", "bed", "intimacy", "性", "做爱", "亲密", "上床" }
		};
		yield return new ActionDefinition
		{
			Id = "rjw_quickie",
			Category = ActionCategory.RJW,
			DisplayName = "EA_Action_rjw_quickie",
			Description = "EA_Action_rjw_quickie_Desc",
			DefaultPromptDesc = "Quick sex with partner (not in bed)",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Medium,
			DefaultEnabled = true,
			Handler = new RJWQuickieHandler(),
			Keywords = new List<string> { "quickie", "quick sex", "快速", "性", "做爱" }
		};
		yield return new ActionDefinition
		{
			Id = "rjw_masturbate",
			Category = ActionCategory.RJW,
			DisplayName = "EA_Action_rjw_masturbate",
			Description = "EA_Action_rjw_masturbate_Desc",
			DefaultPromptDesc = "Masturbate",
			RequiredParams = new List<string> { "actor" },
			RiskLevel = RiskLevel.Medium,
			DefaultEnabled = true,
			Handler = new RJWMasturbateHandler(),
			Keywords = new List<string> { "masturbate", "自慰", "手淫" }
		};
		yield return new ActionDefinition
		{
			Id = "rjw_bondage_give",
			Category = ActionCategory.RJW,
			DisplayName = "EA_Action_rjw_bondage_give",
			Description = "EA_Action_rjw_bondage_give_Desc",
			DefaultPromptDesc = "Give bondage gear to target",
			RequiredParams = new List<string> { "actor", "target", "thing" },
			RiskLevel = RiskLevel.Medium,
			DefaultEnabled = false,
			Handler = new RJWBondageGiveHandler(),
			Keywords = new List<string> { "bondage", "束缚", "绑缚" }
		};
		yield return new ActionDefinition
		{
			Id = "rjw_bondage_unlock",
			Category = ActionCategory.RJW,
			DisplayName = "EA_Action_rjw_bondage_unlock",
			Description = "EA_Action_rjw_bondage_unlock_Desc",
			DefaultPromptDesc = "Unlock bondage gear from target",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Medium,
			DefaultEnabled = false,
			Handler = new RJWBondageUnlockHandler(),
			Keywords = new List<string> { "bondage", "unlock", "束缚", "解锁" }
		};
		yield return new ActionDefinition
		{
			Id = "rjw_rape",
			Category = ActionCategory.RJW,
			DisplayName = "EA_Action_rjw_rape",
			Description = "EA_Action_rjw_rape_Desc",
			DefaultPromptDesc = "Force sexual action on target (requires explicit opt-in)",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Critical,
			DefaultEnabled = false,
			Handler = new RJWRapeHandler(),
			Keywords = new List<string> { "rape", "force", "强制", "强奸" }
		};
		yield return new ActionDefinition
		{
			Id = "rjw_bestiality",
			Category = ActionCategory.RJW,
			DisplayName = "EA_Action_rjw_bestiality",
			Description = "EA_Action_rjw_bestiality_Desc",
			DefaultPromptDesc = "Sexual action with animal (requires explicit opt-in)",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Critical,
			DefaultEnabled = false,
			Handler = new RJWBestialityHandler(),
			Keywords = new List<string> { "bestiality", "animal", "兽交" }
		};
		yield return new ActionDefinition
		{
			Id = "rjw_necro",
			Category = ActionCategory.RJW,
			DisplayName = "EA_Action_rjw_necro",
			Description = "EA_Action_rjw_necro_Desc",
			DefaultPromptDesc = "Necrophilia action (requires explicit opt-in)",
			RequiredParams = new List<string> { "actor", "target" },
			RiskLevel = RiskLevel.Critical,
			DefaultEnabled = false,
			Handler = new RJWNecroHandler(),
			Keywords = new List<string> { "necro", "corpse", "恋尸", "尸体" }
		};
	}

	public IEnumerable<string> GetJobWhitelistEntries()
	{
		yield return "JoinInBed";
		yield return "Quickie";
		yield return "RJW_Masturbate";
		yield return "GiveBondageGear";
		yield return "UnlockBondageGear";
	}

	public IEnumerable<IEAVariableContributor> GetVariableContributors()
	{
		yield return new RJWStatusProvider();
	}
}
