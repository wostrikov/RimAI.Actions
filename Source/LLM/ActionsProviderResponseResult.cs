using System.Collections.Generic;
using RimAI.Core.Runtime;
using Ustas.RimAI.Actions.Parsing;

namespace Ustas.RimAI.Actions.LLM;

/// <summary>
/// One post-provider pass: the Runtime verdict plus whatever conversion the
/// caller may keep. The parse counts are the original decode, not the
/// discarded leftover after a reject, so a fixture can report why the
/// payload was refused.
/// </summary>
public sealed class ActionsProviderResponseResult
{
	public ActionsProviderResponseResult(
		FrontendStructuredConversion conversion,
		ActionsResponseVerdict verdict,
		int parsedActionCount,
		int parseErrorCount,
		IReadOnlyList<string> parseErrors)
	{
		Conversion = conversion;
		Verdict = verdict;
		ParsedActionCount = parsedActionCount;
		ParseErrorCount = parseErrorCount;
		ParseErrors = parseErrors ?? System.Array.Empty<string>();
	}

	public FrontendStructuredConversion Conversion { get; }

	public ActionsResponseVerdict Verdict { get; }

	public int ParsedActionCount { get; }

	public int ParseErrorCount { get; }

	public IReadOnlyList<string> ParseErrors { get; }
}
