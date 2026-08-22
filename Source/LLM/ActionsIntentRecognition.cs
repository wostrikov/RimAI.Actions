using RimAI.Core.Application;
using RimAI.Core.Runtime;

namespace Ustas.RimAI.Actions.LLM;

/// <summary>
/// One recognition pass: the Runtime verdict plus the typed action it accepted.
/// A rejected tier carries no action, so it cannot reach the executor.
/// </summary>
public readonly struct ActionsIntentRecognition
{
	public ActionsIntentRecognition(
		ActionsIntentVerdict verdict,
		LegacyStructuredAction action,
		int keywordScore = 0)
	{
		Verdict = verdict;
		Action = action;
		KeywordScore = keywordScore;
	}

	public ActionsIntentVerdict Verdict { get; }

	public LegacyStructuredAction Action { get; }

	public int KeywordScore { get; }
}
