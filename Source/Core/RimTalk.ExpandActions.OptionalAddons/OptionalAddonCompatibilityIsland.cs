using Ustas.RimAI.Actions.Core;

namespace Ustas.RimAI.Actions.OptionalAddons;

/// <summary>
/// OPTIONAL_ADDON_COMPATIBILITY_ISLAND.
/// RJW/XXX handlers remain registered on ActionRegistry for settings and
/// accounting. They are not invoked by the RimTalk -> RimAI primary path.
/// </summary>
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
