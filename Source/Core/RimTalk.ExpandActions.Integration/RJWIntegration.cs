using Ustas.RimAI.Actions.Core;

namespace Ustas.RimAI.Actions.Integration;

public static class RJWIntegration
{
	public static bool IsAvailable => ModuleRegistry.GetModule("rjw") != null;
}
