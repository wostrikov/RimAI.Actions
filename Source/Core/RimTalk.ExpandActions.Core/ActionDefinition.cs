using System.Collections.Generic;
using RimTalk.ExpandActions.Actions;

namespace RimTalk.ExpandActions.Core;

public class ActionDefinition
{
	public string Id { get; set; }

	public ActionCategory Category { get; set; }

	public string DisplayName { get; set; }

	public string Description { get; set; }

	public string DefaultPromptDesc { get; set; }

	public List<string> RequiredParams { get; set; } = new List<string>();

	public List<string> OptionalParams { get; set; } = new List<string>();

	public bool DefaultEnabled { get; set; } = true;

	public RiskLevel RiskLevel { get; set; }

	public IActionHandler Handler { get; set; }

	public string SourceModule { get; set; }

	public int? Priority { get; set; }

	public List<string> Keywords { get; set; } = new List<string>();

	public bool GetEffectiveDefaultEnabled()
	{
		return RiskLevel switch
		{
			RiskLevel.Low => true,
			RiskLevel.Medium => true,
			RiskLevel.High => false,
			RiskLevel.Critical => false,
			_ => DefaultEnabled,
		};
	}
}
