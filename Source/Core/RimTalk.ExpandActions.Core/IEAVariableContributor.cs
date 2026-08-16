namespace Ustas.RimAI.Actions.Core;

public interface IEAVariableContributor
{
	string VariableName { get; }

	string GetValue(object pawnContext);
}
