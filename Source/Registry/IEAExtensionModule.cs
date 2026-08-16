using System.Collections.Generic;

namespace Ustas.RimAI.Actions.Core;

public interface IEAExtensionModule
{
	string ModuleId { get; }

	string DisplayName { get; }

	bool IsAvailable();

	IEnumerable<ActionDefinition> GetActions();

	IEnumerable<string> GetJobWhitelistEntries();

	IEnumerable<IEAVariableContributor> GetVariableContributors();
}
