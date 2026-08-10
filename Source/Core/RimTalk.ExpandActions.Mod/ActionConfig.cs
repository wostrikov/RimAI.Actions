using System.Collections.Generic;
using Verse;

namespace RimTalk.ExpandActions.Mod;

public class ActionConfig : IExposable
{
	public bool Enabled = true;

	public string CustomPromptDesc;

	public bool KeywordEnabled = true;

	public Dictionary<string, string> PromptKeywordOverrides;

	public void ExposeData()
	{
		Scribe_Values.Look(ref Enabled, "enabled", defaultValue: true);
		Scribe_Values.Look(ref CustomPromptDesc, "customPromptDesc");
		Scribe_Values.Look(ref KeywordEnabled, "keywordEnabled", defaultValue: true);
		Scribe_Collections.Look(ref PromptKeywordOverrides, "promptKeywordOverrides", LookMode.Value, LookMode.Value);
	}

	public void Reset()
	{
		Enabled = true;
		CustomPromptDesc = null;
		KeywordEnabled = true;
		PromptKeywordOverrides = null;
	}
}
