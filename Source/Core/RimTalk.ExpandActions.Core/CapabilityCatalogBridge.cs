using System.Collections.Generic;
using System.Linq;
using RimTalk.ExpandActions.CapabilityRuntime;

namespace RimTalk.ExpandActions.Core;

public static class CapabilityCatalogBridge
{
 public static ICapabilityCatalog BuildEnabledCatalog()
 {
  return new UnifiedCapabilityCatalog(ActionRegistry.GetEnabledActions().Select(ToDescriptor));
 }
 public static CapabilityDescriptor ToDescriptor(ActionDefinition action)
 {
  var parameters=new List<CapabilityParameter>();
  parameters.AddRange(action.RequiredParams.Select(x=>new CapabilityParameter{Name=x,Required=true}));
  parameters.AddRange(action.OptionalParams.Select(x=>new CapabilityParameter{Name=x,Required=false}));
  return new CapabilityDescriptor{CapabilityId=action.Id,Kind=CapabilityKind.ExplicitCapability,Source="ActionRegistry",SourceMod=action.SourceModule??"core",Category=action.Category.ToString(),Parameters=parameters,ExecutionAdapter=action.Handler?.GetType().FullName,MutatesGameState=true,Availability=Availability.Available,Confidence=1m,Provenance="ActionDefinition",Description=action.DefaultPromptDesc};
 }
 public static string BuildMachineContract()
 {
  var schema=CapabilitySchemaGenerator.Generate(BuildEnabledCatalog());
  return Newtonsoft.Json.JsonConvert.SerializeObject(new{schema_version=1,capabilities=schema.Select(x=>new{id=x.Id,required=x.Required,optional=x.Optional,description=x.Description})});
 }
}
