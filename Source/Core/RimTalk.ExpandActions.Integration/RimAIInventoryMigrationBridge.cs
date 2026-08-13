using RimAI.Core.Adapters;
using RimAI.RimWorld.Inventory;
using RimAI.RimWorld.Jobs;
using RimTalk.ExpandActions.Actions;
using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using Verse;

namespace RimTalk.ExpandActions.Integration;

/// <summary>
/// TEMPORARY_MIGRATION_BRIDGE.
/// Contains no gameplay algorithm. Removal condition: RimTalk's current Harmony
/// action caller invokes RimAI capabilities directly instead of IActionHandler.
/// </summary>
public sealed class RimAITakeInventoryBridgeHandler : IActionHandler
{
    public string ActionId => "take_inventory";

    public ExecutionResult Execute(ExecutionContext context)
    {
        var thingName = context.ActionCall.Thing ?? context.ActionCall.Target;
        if (string.IsNullOrWhiteSpace(thingName))
            return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Item name required");

        var quantity = context.ActionCall.GetArg("quantity", 1);
        var result = RimAIInventoryCapabilities.Acquire(
            context.Map,
            context.ResolvedActor,
            thingName,
            QuantityConstraint.Exact(System.Math.Max(1, quantity)),
            Metadata(context),
            looseNotStoredOnly: false);
        return Map(context, result, thingName);
    }

    internal static JobDispatchMetadata Metadata(ExecutionContext context) => new(
        context.ConversationId,
        context.SentenceId,
        context.ActionCall?.Id,
        context.IsFirstActionForActor,
        context.ActionPriority,
        RimTalk.ExpandActions.Mod.EAModMain.Settings.JobProtectionTicks);

    internal static ExecutionResult Map(ExecutionContext context, AdapterResult result, string target)
    {
        var description = $"{result.Completed}/{result.Requested} {target}";
        if (result.Code == FailureCodes.Completed)
            return ExecutionResult.Queued(context, description);
        if (result.Code == FailureCodes.PartialAvailability)
            return ExecutionResult.Failed(context, ErrorCode.PartialAvailability, description);

        var code = result.Code switch
        {
            FailureCodes.TargetNotFound => ErrorCode.TargetNotFound,
            FailureCodes.Unreachable => ErrorCode.TargetUnreachable,
            FailureCodes.ActorIncapable => ErrorCode.ActorIncapable,
            FailureCodes.JobNotQueued => ErrorCode.JobNotQueued,
            FailureCodes.InvalidQuantity => ErrorCode.InvalidParameters,
            _ => ErrorCode.ExecutionException
        };
        return ExecutionResult.Failed(context, code, result.Detail ?? result.Code);
    }
}

/// <summary>
/// TEMPORARY_MIGRATION_BRIDGE with the same removal condition as
/// RimAITakeInventoryBridgeHandler.
/// </summary>
public sealed class RimAIHaulBridgeHandler : IActionHandler
{
    public string ActionId => "haul";

    public ExecutionResult Execute(ExecutionContext context)
    {
        var target = context.ResolvedTarget;
        if (target is null)
            return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "Haul target required");

        var result = RimAIInventoryCapabilities.Haul(
            context.Map,
            context.ResolvedActor,
            target,
            QuantityConstraint.All(),
            RimAITakeInventoryBridgeHandler.Metadata(context));
        return RimAITakeInventoryBridgeHandler.Map(context, result, target.LabelCap);
    }
}
