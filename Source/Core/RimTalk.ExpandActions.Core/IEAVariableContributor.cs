namespace RimTalk.ExpandActions.Core;

public interface IEAVariableContributor
{
	string VariableName { get; }

	string GetValue(object pawnContext);
}
