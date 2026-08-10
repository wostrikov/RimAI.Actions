using System.Collections.Generic;

namespace RimTalk.ExpandActions.Core;

public interface IEAExtensionModule
{
	string ModuleId { get; }

	string DisplayName { get; }

	bool IsAvailable();

	IEnumerable<ActionDefinition> GetActions();

	IEnumerable<string> GetJobWhitelistEntries();

	IEnumerable<IEAVariableContributor> GetVariableContributors();
}
