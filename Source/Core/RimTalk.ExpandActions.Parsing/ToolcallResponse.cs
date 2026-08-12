using System.Collections.Generic;
using Newtonsoft.Json;

namespace RimTalk.ExpandActions.Parsing;

public class ToolcallResponse
{
	[JsonProperty("actions")]
	public List<ActionCall> Actions { get; set; } = new List<ActionCall>();

	[JsonIgnore]
	public List<string> ParseErrors { get; set; } = new List<string>();

	[JsonIgnore]
	public List<string> ValidationErrors { get; set; } = new List<string>();

	[JsonIgnore]
	public string RawJson { get; set; }

	[JsonIgnore]
	public bool IsValid
	{
		get
		{
			if (ParseErrors.Count == 0)
			{
				return Actions != null;
			}
			return false;
		}
	}
}
