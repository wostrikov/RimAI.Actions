using System.Collections.Generic;
using RimAI.Core.Application;

namespace Ustas.RimAI.Actions.Parsing;

public sealed class FrontendStructuredConversion
{
	public List<LegacyStructuredAction> Actions { get; } = new List<LegacyStructuredAction>();

	public List<string> Errors { get; } = new List<string>();
}
