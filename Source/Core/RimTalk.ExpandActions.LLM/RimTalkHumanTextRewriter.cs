#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using RimAI.Core.Application;
using RimTalk.ExpandActions.Mod;

namespace RimTalk.ExpandActions.LLM;

/// <summary>
/// Rewrites human-facing talk text only. Does not convert or execute capabilities.
/// </summary>
public sealed class RimTalkHumanTextRewriter : IHumanTextRewriter
{
	public async Task<string?> RewriteAsync(
		string humanText,
		LanguageContext language,
		string instruction,
		CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(humanText))
			return humanText;
		cancellationToken.ThrowIfCancellationRequested();
		var timeout = TimeSpan.FromSeconds(Math.Max(5, EAModMain.Settings.SecondaryLLMTimeout));
		var user = "Rewrite only the human-facing dialogue below. Do not output JSON, capability IDs, or actions.\n\n" + humanText;
		return await SecondaryLLMCaller.CompleteOnceAsync(instruction, user, timeout).ConfigureAwait(false);
	}
}
