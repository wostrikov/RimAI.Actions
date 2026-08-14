using System.Collections.Generic;
using System.Linq;
using RimAI.Core.Catalog;
using RimAI.Core.Execution;
using RimTalk.ExpandActions.CapabilityRuntime;

namespace RimTalk.ExpandActions.Core;

public static class CapabilityCatalogBridge
{
 public static RimAI.Core.Catalog.ICapabilityCatalog BuildEnabledCatalog()
 {
  return new CapabilityCatalogFacade(ActionRegistry.GetEnabledActions().Select(ToRimAIDescriptor));
 }

 public static RimAI.Core.Catalog.CapabilityDescriptor ToRimAIDescriptor(ActionDefinition action)
 {
  if (CapabilityOwnershipRegistry.TryResolve(action.Id, out var ownership)
      && ownership is not null
      && ownership.Owner == CapabilityExecutionOwner.RimAI)
  {
   return Stage61CapabilityBootstrap.CreateFoundationCatalog()
    .GetCapability(ownership.CapabilityId);
  }
  return new RimAI.Core.Catalog.CapabilityDescriptor(
   action.Id,
   action.DisplayName,
   "ExpandActions",
   MapFamily(action.Category),
   CapabilityAvailability.Executable,
   ExecutionKind.ExpandActionsHandler,
   new CapabilityParameterSchema(action.RequiredParams, action.OptionalParams),
   adapterId: action.Handler?.GetType().FullName,
   provenance: "ActionRegistry",
   sourcePackageId: "zruic.expand.action");
 }

 public static string BuildMachineContract()
 {
  var schema=BuildEnabledCatalog().ListCapabilities();
  return Newtonsoft.Json.JsonConvert.SerializeObject(new
  {
   schema_version=2,
   capabilities=schema.Select(x=>new
   {
    id=x.CapabilityId,
    required=x.Parameters.Required,
    optional=x.Parameters.Optional,
    family=x.Family.ToString(),
    owner=x.Source,
    adapter=x.AdapterId
   })
  });
 }

 private static CapabilityFamily MapFamily(ActionCategory category)
  => System.Enum.TryParse<CapabilityFamily>(category.ToString(), out var family)
   ? family
   : CapabilityFamily.Unknown;
}
