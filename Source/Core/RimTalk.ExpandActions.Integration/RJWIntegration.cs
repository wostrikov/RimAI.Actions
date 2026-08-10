using RimTalk.ExpandActions.Core;

namespace RimTalk.ExpandActions.Integration;

public static class RJWIntegration
{
	public static bool IsAvailable => ModuleRegistry.GetModule("rjw") != null;
}
