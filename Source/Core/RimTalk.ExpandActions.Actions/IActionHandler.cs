using Ustas.RimAI.Actions.Execution;

namespace Ustas.RimAI.Actions.Actions;

public interface IActionHandler
{
	string ActionId { get; }

	ExecutionResult Execute(ExecutionContext context);
}
