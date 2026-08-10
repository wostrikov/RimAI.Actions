using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using Verse;

namespace RimTalk.ExpandActions.Actions.Item;

public class DropItemHandler : IActionHandler
{
	public string ActionId => "drop_item";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn resolvedActor = context.ResolvedActor;
		if (resolvedActor.carryTracker?.CarriedThing != null)
		{
			resolvedActor.carryTracker.TryDropCarriedThing(resolvedActor.Position, ThingPlaceMode.Near, out var _);
			EALogger.Debug(resolvedActor.Name.ToStringShort + " dropped carried item");
			return ExecutionResult.Succeeded(context);
		}
		if (resolvedActor.equipment?.Primary != null)
		{
			resolvedActor.equipment.TryDropEquipment(resolvedActor.equipment.Primary, out var _, resolvedActor.Position);
			EALogger.Debug(resolvedActor.Name.ToStringShort + " dropped weapon");
			return ExecutionResult.Succeeded(context);
		}
		return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Nothing to drop");
	}
}
