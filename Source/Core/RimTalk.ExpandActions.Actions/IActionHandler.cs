using RimTalk.ExpandActions.Execution;

namespace RimTalk.ExpandActions.Actions;

public interface IActionHandler
{
	string ActionId { get; }

	ExecutionResult Execute(ExecutionContext context);
}
