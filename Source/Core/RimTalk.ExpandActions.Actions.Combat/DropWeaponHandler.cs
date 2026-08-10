using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using Verse;

namespace RimTalk.ExpandActions.Actions.Combat;

public class DropWeaponHandler : IActionHandler
{
	public string ActionId => "drop_weapon";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn resolvedActor = context.ResolvedActor;
		if (resolvedActor.equipment?.Primary == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "No weapon equipped");
		}
		resolvedActor.equipment.TryDropEquipment(resolvedActor.equipment.Primary, out var _, resolvedActor.Position);
		EALogger.Debug(resolvedActor.Name.ToStringShort + " dropped weapon");
		return ExecutionResult.Succeeded(context);
	}
}
