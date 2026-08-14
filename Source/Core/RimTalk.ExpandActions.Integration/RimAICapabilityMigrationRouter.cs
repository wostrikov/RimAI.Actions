using RimAI.Core.Adapters;
using RimAI.Core.Execution;
using RimAI.Core.Observation;
using RimAI.RimWorld.Animals;
using RimAI.RimWorld.Equipment;
using RimAI.RimWorld.Inventory;
using RimAI.RimWorld.Jobs;
using RimAI.RimWorld.Medical;
using RimAI.RimWorld.Movement;
using RimAI.RimWorld.Prisoner;
using RimAI.RimWorld.Combat;
using RimAI.RimWorld.Construction;
using RimAI.RimWorld.Funeral;
using RimAI.RimWorld.Recreation;
using RimAI.RimWorld.Ingest;
using RimAI.RimWorld.Work;
using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;

namespace RimTalk.ExpandActions.Integration;

/// <summary>
/// TEMPORARY_MIGRATION_ROUTER. It contains no gameplay algorithm and is driven
/// only by RimAI ownership metadata. Delete when the ownership map reports zero
/// remaining EA capabilities and the conversation frontend calls RimAI directly.
/// </summary>
public static class RimAICapabilityMigrationRouter
{
    public static bool TryExecute(ExecutionContext context, out ExecutionResult result)
    {
        result = null;
        var requestedId = context.ActionCall.Id;
        if (!CapabilityOwnershipRegistry.TryResolve(requestedId, out var ownership)
            || ownership is null
            || ownership.Owner != CapabilityExecutionOwner.RimAI)
            return false;

        if (ownership.CapabilityId == RimAI.Core.Catalog.Stage61CapabilityBootstrap.AcquireThingId)
        {
            var thingName = context.ActionCall.Thing ?? context.ActionCall.Target;
            if (string.IsNullOrWhiteSpace(thingName))
            {
                result = ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "Item name required");
                return true;
            }
            var quantity = context.ActionCall.GetArg("quantity", 1);
            result = Map(
                context,
                RimAIInventoryCapabilities.Acquire(
                    context.Map,
                    context.ResolvedActor,
                    thingName,
                    QuantityConstraint.Exact(System.Math.Max(1, quantity)),
                    Metadata(context, ownership.CapabilityId)),
                thingName);
            return true;
        }

        if (ownership.CapabilityId == RimAI.Core.Catalog.Stage61CapabilityBootstrap.TransferToStorageId)
        {
            if (context.ResolvedTarget is null)
            {
                result = ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "Haul target required");
                return true;
            }
            result = Map(
                context,
                RimAIInventoryCapabilities.Haul(
                    context.Map,
                    context.ResolvedActor,
                    context.ResolvedTarget,
                    QuantityConstraint.All(),
                    Metadata(context, ownership.CapabilityId)),
                context.ResolvedTarget.LabelCap);
            return true;
        }

        if (WorkCapabilityCatalog.TryResolve(ownership.CapabilityId, out _))
        {
            result = Map(
                context,
                RimAIWorkCapabilities.Execute(
                    context.Map,
                    context.ResolvedActor,
                    context.ResolvedTarget,
                    ownership.CapabilityId,
                    Metadata(context, ownership.CapabilityId),
                    RimTalk.ExpandActions.Mod.EAModMain.Settings.AllowUndesignatedTargets,
                    context.ResolvedCell),
                ownership.CapabilityId);
            return true;
        }

        if (MovementCapabilityCatalog.TryResolve(ownership.CapabilityId, out var movement)
            && movement is not null)
        {
            int? duration = movement.Operation switch
            {
                MovementOperation.Wait => context.ActionCall.GetArg(
                    "ticks",
                    MovementExecutionService.DefaultWaitTicks),
                MovementOperation.Follow => context.ActionCall.GetArg(
                    "duration",
                    MovementExecutionService.DefaultFollowTicks),
                _ => null
            };
            result = Map(
                context,
                RimAIMovementCapabilities.Execute(
                    context.Map,
                    context.ResolvedActor,
                    context.ResolvedTarget,
                    context.ResolvedCell,
                    ownership.CapabilityId,
                    duration,
                    Metadata(context, ownership.CapabilityId)),
                ownership.CapabilityId);
            return true;
        }

        if (AnimalWorkCapabilityCatalog.TryResolve(ownership.CapabilityId, out _))
        {
            result = Map(
                context,
                RimAIAnimalWorkCapabilities.Execute(
                    context.ResolvedActor,
                    context.ResolvedTarget,
                    ownership.CapabilityId,
                    Metadata(context, ownership.CapabilityId))
                    .ToAdapterResult(),
                ownership.CapabilityId);
            return true;
        }

        if (EquipmentCapabilityCatalog.TryResolve(ownership.CapabilityId, out _))
        {
            result = Map(
                context,
                RimAIEquipmentCapabilities.Execute(
                    context.Map,
                    context.ResolvedActor,
                    context.ResolvedTarget,
                    context.ActionCall.Thing ?? context.ActionCall.Target,
                    ownership.CapabilityId,
                    Metadata(context, ownership.CapabilityId)),
                ownership.CapabilityId);
            return true;
        }

        if (MedicalCareCapabilityCatalog.TryResolve(ownership.CapabilityId, out _))
        {
            result = Map(
                context,
                RimAIMedicalCapabilities.Execute(
                    context.ResolvedActor,
                    context.ResolvedTarget,
                    ownership.CapabilityId,
                    Metadata(context, ownership.CapabilityId))
                    .ToAdapterResult(),
                ownership.CapabilityId);
            return true;
        }

        if (PrisonerTransportCapabilityCatalog.TryResolve(ownership.CapabilityId, out _))
        {
            result = Map(
                context,
                RimAIPrisonerCapabilities.Execute(
                    context.ResolvedActor,
                    context.ResolvedTarget,
                    ownership.CapabilityId,
                    Metadata(context, ownership.CapabilityId))
                    .ToAdapterResult(),
                ownership.CapabilityId);
            return true;
        }

        if (PrisonerSocialCapabilityCatalog.TryResolve(ownership.CapabilityId, out _))
        {
            result = Map(
                context,
                RimAIPrisonerSocialCapabilities.Execute(
                    context.ResolvedActor,
                    context.ResolvedTarget,
                    ownership.CapabilityId,
                    Metadata(context, ownership.CapabilityId))
                    .ToAdapterResult(),
                ownership.CapabilityId);
            return true;
        }

        if (CombatCapabilityCatalog.TryResolve(ownership.CapabilityId, out var combat)
            && combat is not null)
        {
            bool? drafted = combat.Operation == CombatOperation.SetDrafted
                ? context.ActionCall.GetArg("drafted", CombatExecutionService.DefaultDrafted)
                : null;
            result = Map(
                context,
                RimAICombatCapabilities.Execute(
                    context.ResolvedActor,
                    context.ResolvedTarget,
                    ownership.CapabilityId,
                    Metadata(context, ownership.CapabilityId),
                    drafted)
                    .ToAdapterResult(),
                ownership.CapabilityId);
            return true;
        }

        if (IngestCapabilityCatalog.TryResolve(ownership.CapabilityId, out _))
        {
            result = Map(
                context,
                RimAIIngestCapabilities.Execute(
                    context.ResolvedActor,
                    context.ResolvedTarget,
                    context.ActionCall.Thing,
                    ownership.CapabilityId,
                    Metadata(context, ownership.CapabilityId))
                    .ToAdapterResult(),
                ownership.CapabilityId);
            return true;
        }

        if (ItemTransferCapabilityCatalog.TryResolve(ownership.CapabilityId, out _))
        {
            result = Map(
                context,
                RimAIItemTransferCapabilities.Execute(
                    context.ResolvedActor,
                    context.ResolvedTarget,
                    context.ActionCall.Thing,
                    ownership.CapabilityId,
                    Metadata(context, ownership.CapabilityId))
                    .ToAdapterResult(),
                ownership.CapabilityId);
            return true;
        }

        if (ConstructionDesignationCatalog.TryResolve(ownership.CapabilityId, out _))
        {
            var cell = context.ResolvedCell ?? context.ResolvedTarget?.Position;
            if (!cell.HasValue)
            {
                result = ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "Target location not found");
                return true;
            }
            result = Map(
                context,
                RimAIConstructionDesignationCapabilities.Execute(
                    context.Map,
                    cell.Value,
                    ownership.CapabilityId,
                    context.ResolvedActor)
                    .ToAdapterResult(),
                ownership.CapabilityId);
            return true;
        }

        if (FuneralCapabilityCatalog.TryResolve(ownership.CapabilityId, out _))
        {
            result = Map(
                context,
                RimAIFuneralCapabilities.Execute(
                    context.ResolvedActor,
                    context.ResolvedTarget,
                    ownership.CapabilityId,
                    Metadata(context, ownership.CapabilityId))
                    .ToAdapterResult(),
                ownership.CapabilityId);
            return true;
        }

        if (RecreationCapabilityCatalog.TryResolve(ownership.CapabilityId, out _))
        {
            result = Map(
                context,
                RimAIRecreationCapabilities.Execute(
                    context.ResolvedActor,
                    context.ResolvedTarget,
                    ownership.CapabilityId,
                    Metadata(context, ownership.CapabilityId))
                    .ToAdapterResult(),
                ownership.CapabilityId);
            return true;
        }

        result = ExecutionResult.Failed(
            context,
            ErrorCode.ExecutionException,
            "RimAI ownership has no adapter: " + ownership.CapabilityId);
        return true;
    }

    public static string DefinitionId(string requestedId) =>
        CapabilityOwnershipRegistry.TryResolve(requestedId, out var ownership) && ownership is not null
            ? ownership.LegacyActionId
            : requestedId;

    private static JobDispatchMetadata Metadata(ExecutionContext context, string capabilityId)
    {
        var planId = $"ea:{context.ConversationId}";
        var stepId = context.SentenceId ?? capabilityId;
        return new JobDispatchMetadata(
            context.ConversationId,
            context.SentenceId,
            capabilityId,
            context.IsFirstActionForActor,
            context.ActionPriority,
            RimTalk.ExpandActions.Mod.EAModMain.Settings.JobProtectionTicks,
            PlanId: planId,
            StepId: stepId,
            AttemptId: ExecutionAttemptId.Create(planId, stepId, 1));
    }

    private static ExecutionResult Map(ExecutionContext context, AdapterResult adapter, string target)
    {
        var description = $"{adapter.Completed}/{adapter.Requested} {target}";
        if (adapter.Code == FailureCodes.Completed)
            return ExecutionResult.Completed(context, description);
        if (adapter.Code == FailureCodes.Queued)
            return ExecutionResult.Queued(context, description);
        if (adapter.Code == FailureCodes.PartialAvailability)
            return ExecutionResult.Failed(context, ErrorCode.PartialAvailability, description);

        var code = adapter.Code switch
        {
            FailureCodes.TargetNotFound => ErrorCode.TargetNotFound,
            FailureCodes.Unreachable => ErrorCode.TargetUnreachable,
            FailureCodes.ActorIncapable => ErrorCode.ActorIncapable,
            FailureCodes.JobNotQueued => ErrorCode.JobNotQueued,
            FailureCodes.InvalidQuantity => ErrorCode.InvalidParameters,
            FailureCodes.InvalidPlan => ErrorCode.InvalidParameters,
            FailureCodes.TargetNotReady => ErrorCode.InvalidParameters,
            FailureCodes.InvalidTarget => ErrorCode.TargetNotFound,
            FailureCodes.InsufficientSkill => ErrorCode.ActorIncapable,
            FailureCodes.ReservationFailed => ErrorCode.TargetUnreachable,
            FailureCodes.CapabilityUnavailable => ErrorCode.ActionNotInWhitelist,
            FailureCodes.EquipmentUnavailable => ErrorCode.InvalidParameters,
            FailureCodes.DestinationUnavailable => ErrorCode.TargetNotFound,
            FailureCodes.NoValidFood => ErrorCode.TargetNotFound,
            FailureCodes.NoValidFuel => ErrorCode.TargetNotFound,
            FailureCodes.NotRefuelable => ErrorCode.InvalidParameters,
            FailureCodes.NotSowable => ErrorCode.TargetNotFound,
            FailureCodes.NotRoofable => ErrorCode.InvalidParameters,
            FailureCodes.NoRoofPresent => ErrorCode.TargetNotFound,
            FailureCodes.RoofNotRemovable => ErrorCode.InvalidParameters,
            FailureCodes.CorpseInvalid => ErrorCode.InvalidParameters,
            FailureCodes.NoValidBurialContainer => ErrorCode.TargetNotFound,
            FailureCodes.NoValidCrematorium => ErrorCode.TargetNotFound,
            FailureCodes.NoValidRecreationSpot => ErrorCode.TargetNotFound,
            FailureCodes.NoActiveResearch => ErrorCode.InvalidParameters,
            FailureCodes.NotOpenable => ErrorCode.InvalidParameters,
            FailureCodes.UnknownCapability => ErrorCode.ActionNotInWhitelist,
            _ => ErrorCode.ExecutionException
        };
        return ExecutionResult.Failed(
            context,
            code,
            adapter.Detail is null ? adapter.Code : $"{adapter.Code}: {adapter.Detail}");
    }
}
