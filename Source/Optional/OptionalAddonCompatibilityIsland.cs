using Ustas.RimAI.Actions.Core;

namespace Ustas.RimAI.Actions.OptionalAddons;

public static class OptionalAddonCompatibilityIsland
{
	public const string Status = "ISOLATED";

	public static bool IsOptionalAddon(string actionId)
	{
		if (string.IsNullOrWhiteSpace(actionId))
			return false;
		var definition = ActionRegistry.GetById(actionId);
		return definition?.Handler != null
		       && definition.Category == ActionCategory.RJW;
	}
}
