using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace RimTalk.ExpandActions.CapabilityRuntime;

public static class SemanticTargetSelector
{
    public static string Select(string target, string thing) =>
        !string.IsNullOrWhiteSpace(target) ? target : thing;
}

public enum CapabilityKind { ExplicitCapability, DiscoveredCapability, GenericAdapterCapability, ExplicitIntegrationRequired }
public enum Availability { Available, Unavailable, UnsafeOrInternal }
public enum PlanState { Received, Parsed, Validated, Resolved, Queued, Started, Completed, Failed, Rejected, Unsupported, ReplanRequired }
public enum QuantityMode { Exact, UpTo, AtLeast, All }
public enum DestinationKind { Implicit, PawnInventory, Stockpile, Cell, Container, Equipment, TargetPawn }
public enum ObservationStatus { Accepted, Queued, Started, Completed, Failed, Interrupted, PartiallyCompleted }

public sealed class CapabilityParameter { public string Name { get; set; } public bool Required { get; set; } public string Type { get; set; } = "string"; }
public sealed class CapabilityDescriptor
{
 public string CapabilityId { get; set; } public CapabilityKind Kind { get; set; } public string Source { get; set; } public string SourceMod { get; set; }
 public string Category { get; set; } public string ActorType { get; set; }="Pawn"; public IReadOnlyList<string> TargetTypes { get; set; }=Array.Empty<string>();
 public IReadOnlyList<CapabilityParameter> Parameters { get; set; }=Array.Empty<CapabilityParameter>(); public IReadOnlyList<string> Preconditions { get; set; }=Array.Empty<string>();
 public string ExecutionAdapter { get; set; } public bool MutatesGameState { get; set; } public Availability Availability { get; set; }=Availability.Available;
 public decimal Confidence { get; set; }=1m; public string Provenance { get; set; } public string Description { get; set; }
}
public interface ICapabilityCatalog { IReadOnlyList<CapabilityDescriptor> All { get; } CapabilityDescriptor Find(string id); }
public sealed class UnifiedCapabilityCatalog : ICapabilityCatalog
{
 readonly Dictionary<string,CapabilityDescriptor> entries=new(StringComparer.Ordinal);
 public UnifiedCapabilityCatalog(IEnumerable<CapabilityDescriptor> descriptors) { foreach(var d in descriptors.OrderBy(x=>x.CapabilityId,StringComparer.Ordinal)){ if(d==null||string.IsNullOrWhiteSpace(d.CapabilityId)) throw new ArgumentException("Capability ID is required"); if(entries.ContainsKey(d.CapabilityId)) throw new InvalidOperationException("Duplicate capability ID: "+d.CapabilityId); entries.Add(d.CapabilityId,d); } }
 public IReadOnlyList<CapabilityDescriptor> All=>entries.Values.OrderBy(x=>x.CapabilityId,StringComparer.Ordinal).ToArray(); public CapabilityDescriptor Find(string id)=>id!=null&&entries.TryGetValue(id,out var d)?d:null;
}
public abstract class WorldRef { public string SemanticName { get; set; } }
public sealed class PawnRef:WorldRef{} public sealed class ThingDefRef:WorldRef{} public sealed class ThingRef:WorldRef{public ThingDefRef Definition{get;set;}}
public sealed class CellRef:WorldRef{public int? X{get;set;} public int? Z{get;set;}} public sealed class AreaRef:WorldRef{}
public sealed class DestinationRef:WorldRef{public DestinationKind Kind{get;set;} public CellRef Cell{get;set;} public PawnRef Pawn{get;set;}}
public sealed class QuantityConstraint{public QuantityMode Mode{get;set;} public int? Amount{get;set;}}
public sealed class Plan { public int Version{get;set;}=1; public string PlanId{get;set;} public PawnRef Actor{get;set;} public string Goal{get;set;} public List<PlanStep> Steps{get;set;}=new(); public List<string> SuccessConditions{get;set;}=new(); }
public sealed class PlanStep { public string StepId{get;set;} public string CapabilityId{get;set;} public Dictionary<string,object> Inputs{get;set;}=new(StringComparer.Ordinal); public List<string> DependsOn{get;set;}=new(); public string ExpectedResult{get;set;} }
[DataContract] public sealed class CanonicalCallEnvelope{[DataMember(Name="actions")]public List<CanonicalCall>Actions{get;set;}=new();}
[DataContract] public sealed class CanonicalCall{[DataMember(Name="id")]public string Id{get;set;}[DataMember(Name="actor")]public string Actor{get;set;}}
public static class CanonicalCallParser{public static CanonicalCallEnvelope Parse(string json){using(var stream=new MemoryStream(Encoding.UTF8.GetBytes(json))){return (CanonicalCallEnvelope)new DataContractJsonSerializer(typeof(CanonicalCallEnvelope)).ReadObject(stream);}}}
public sealed class ValidationIssue{public string Code{get;set;} public string StepId{get;set;} public string Message{get;set;}}
public sealed class ValidationResult{public bool IsValid=>Issues.Count==0; public List<ValidationIssue> Issues{get;}=new();}
public static class PlanValidator
{
 public static ValidationResult Validate(Plan plan,ICapabilityCatalog catalog){var r=new ValidationResult(); if(plan==null){r.Issues.Add(new(){Code="INVALID_PLAN"});return r;} var ids=new HashSet<string>(StringComparer.Ordinal); foreach(var s in plan.Steps){if(string.IsNullOrWhiteSpace(s.StepId)||!ids.Add(s.StepId))r.Issues.Add(new(){Code="DUPLICATE_OR_MISSING_STEP_ID",StepId=s.StepId});var c=catalog.Find(s.CapabilityId);if(c==null){r.Issues.Add(new(){Code="UNSUPPORTED_CAPABILITY_ID",StepId=s.StepId,Message=s.CapabilityId});continue;}if(c.Availability!=Availability.Available)r.Issues.Add(new(){Code="CAPABILITY_UNAVAILABLE",StepId=s.StepId});foreach(var p in c.Parameters.Where(x=>x.Required))if(!s.Inputs.ContainsKey(p.Name)||s.Inputs[p.Name]==null)r.Issues.Add(new(){Code="MISSING_REQUIRED_PARAMETER",StepId=s.StepId,Message=p.Name});}foreach(var s in plan.Steps)foreach(var d in s.DependsOn)if(!ids.Contains(d))r.Issues.Add(new(){Code="UNKNOWN_DEPENDENCY",StepId=s.StepId,Message=d});return r;}
}
public sealed class CapabilitySchemaEntry{public string Id{get;set;} public IReadOnlyList<string> Required{get;set;} public IReadOnlyList<string> Optional{get;set;} public string Description{get;set;}}
public static class CapabilitySchemaGenerator{public static IReadOnlyList<CapabilitySchemaEntry> Generate(ICapabilityCatalog c)=>c.All.Where(x=>x.Availability==Availability.Available).Select(x=>new CapabilitySchemaEntry{Id=x.CapabilityId,Required=x.Parameters.Where(p=>p.Required).Select(p=>p.Name).OrderBy(v=>v,StringComparer.Ordinal).ToArray(),Optional=x.Parameters.Where(p=>!p.Required).Select(p=>p.Name).OrderBy(v=>v,StringComparer.Ordinal).ToArray(),Description=x.Description}).ToArray();}
public sealed class PlanStateMachine{static readonly Dictionary<PlanState,PlanState[]> Allowed=new(){{PlanState.Received,new[]{PlanState.Parsed,PlanState.Rejected}},{PlanState.Parsed,new[]{PlanState.Validated,PlanState.Unsupported,PlanState.Rejected}},{PlanState.Validated,new[]{PlanState.Resolved,PlanState.Failed}},{PlanState.Resolved,new[]{PlanState.Queued,PlanState.Started,PlanState.Failed}},{PlanState.Queued,new[]{PlanState.Started,PlanState.Failed,PlanState.ReplanRequired}},{PlanState.Started,new[]{PlanState.Completed,PlanState.Failed,PlanState.ReplanRequired}}};public PlanState State{get;private set;}=PlanState.Received;public void Transition(PlanState next){if(!Allowed.TryGetValue(State,out var a)||!a.Contains(next))throw new InvalidOperationException($"Invalid plan transition {State} -> {next}");State=next;}}
public sealed class ReplanPolicy{public int MaximumReplans{get;}public int MaximumPlanSteps{get;}public ReplanPolicy(int replans=2,int steps=12){MaximumReplans=replans;MaximumPlanSteps=steps;}public bool MayReplan(int used,int steps)=>used<MaximumReplans&&steps<=MaximumPlanSteps;}
public sealed class ItemStack{public string StackId{get;set;}public string ThingDef{get;set;}public int Count{get;set;}public bool Reachable{get;set;}=true;}
public sealed class TransferSlice{public string StackId{get;set;}public int Count{get;set;}}
public sealed class QuantityResolution{public bool Success{get;set;}public string ErrorCode{get;set;}public int Requested{get;set;}public int Resolved{get;set;}public IReadOnlyList<TransferSlice>Slices{get;set;}=Array.Empty<TransferSlice>();}
public static class QuantityResolver{public static QuantityResolution Resolve(IEnumerable<ItemStack> stacks,string def,QuantityConstraint q){var matching=stacks.Where(x=>x.ThingDef==def&&x.Count>0).OrderBy(x=>x.StackId,StringComparer.Ordinal).ToArray();var all=matching.Where(x=>x.Reachable).ToArray();var available=all.Sum(x=>x.Count);var total=matching.Sum(x=>x.Count);var requested=q.Mode==QuantityMode.All?available:q.Amount.GetValueOrDefault();var target=q.Mode==QuantityMode.UpTo?Math.Min(requested,available):requested;if(total>=requested&&available<requested)return new(){Requested=requested,Resolved=available,ErrorCode="TARGET_UNREACHABLE"};if((q.Mode==QuantityMode.Exact||q.Mode==QuantityMode.AtLeast)&&available<requested)return new(){Requested=requested,Resolved=available,ErrorCode="INSUFFICIENT_QUANTITY"};var left=target;var slices=new List<TransferSlice>();foreach(var s in all){var take=Math.Min(s.Count,left);if(take>0)slices.Add(new(){StackId=s.StackId,Count=take});left-=take;if(left==0)break;}return new(){Success=true,Requested=requested,Resolved=target,Slices=slices};}}
public sealed class ExecutionObservation{public string PlanId{get;set;}public string StepId{get;set;}public ObservationStatus Status{get;set;}public int CompletedQuantity{get;set;}public string ErrorCode{get;set;}}
public interface IActionObserver{void Record(ExecutionObservation observation);} public interface IPlanClock{long CurrentTick{get;}} public interface ICapabilityExecutor{ExecutionObservation Execute(Plan plan,PlanStep step);}
public sealed class RecordingObserver:IActionObserver{public List<ExecutionObservation>Observations{get;}=new();public void Record(ExecutionObservation observation)=>Observations.Add(observation);}
public sealed class FakeCapabilityExecutor:ICapabilityExecutor{readonly Func<Plan,PlanStep,ExecutionObservation>run;public int InvocationCount{get;private set;}public FakeCapabilityExecutor(Func<Plan,PlanStep,ExecutionObservation>run){this.run=run;}public ExecutionObservation Execute(Plan p,PlanStep s){InvocationCount++;return run(p,s);}}
public sealed class PlanRunner{readonly ICapabilityCatalog catalog;readonly ICapabilityExecutor executor;readonly IActionObserver observer;public PlanRunner(ICapabilityCatalog c,ICapabilityExecutor e,IActionObserver o){catalog=c;executor=e;observer=o;}public ValidationResult ValidateAndRun(Plan p){var v=PlanValidator.Validate(p,catalog);if(!v.IsValid)return v;foreach(var s in p.Steps){var outcome=executor.Execute(p,s);observer.Record(outcome);if(outcome.Status is ObservationStatus.Failed or ObservationStatus.Interrupted or ObservationStatus.PartiallyCompleted)break;}return v;}}
public sealed class FakeInventoryWorld
{public List<ItemStack>GroundStacks{get;}=new();public Dictionary<string,int>Inventory{get;}=new(StringComparer.Ordinal);public int Capacity{get;set;}=int.MaxValue;public int FailAfterSlices{get;set;}=int.MaxValue;
 public ExecutionObservation Transfer(string p,string s,string def,QuantityConstraint q,DestinationRef d){if(d==null||d.Kind!=DestinationKind.PawnInventory)return Fail(p,s,"UNSUPPORTED_DESTINATION");var r=QuantityResolver.Resolve(GroundStacks,def,q);if(!r.Success)return Fail(p,s,r.ErrorCode,r.Resolved);if(r.Resolved>Capacity)return Fail(p,s,"INVENTORY_CAPACITY_INSUFFICIENT");var done=0;var i=0;foreach(var x in r.Slices){if(i++>=FailAfterSlices)return Fail(p,s,"PARTIAL_TRANSFER",done,ObservationStatus.PartiallyCompleted);GroundStacks.Single(v=>v.StackId==x.StackId).Count-=x.Count;done+=x.Count;}Inventory[def]=Inventory.TryGetValue(def,out var old)?old+done:done;return new(){PlanId=p,StepId=s,Status=ObservationStatus.Completed,CompletedQuantity=done};}
 static ExecutionObservation Fail(string p,string s,string e,int n=0,ObservationStatus status=ObservationStatus.Failed)=>new(){PlanId=p,StepId=s,Status=status,ErrorCode=e,CompletedQuantity=n};}
public sealed class DiscoveryCandidate{public string StableId{get;set;}public string Source{get;set;}public string SourceMod{get;set;}public string Category{get;set;}public bool IsGameplayOperation{get;set;}public bool HasSafeGenericAdapter{get;set;}public bool RequiresExplicitIntegration{get;set;}public bool IsHostOrInternal{get;set;}public string Adapter{get;set;}}
public sealed class CapabilityDiscoveryEngine{public IReadOnlyList<CapabilityDescriptor>Discover(IEnumerable<DiscoveryCandidate> items)=>items.OrderBy(x=>x.StableId,StringComparer.Ordinal).Select(x=>new CapabilityDescriptor{CapabilityId=x.StableId,Source=x.Source,SourceMod=x.SourceMod,Category=x.Category,Kind=x.RequiresExplicitIntegration?CapabilityKind.ExplicitIntegrationRequired:x.HasSafeGenericAdapter?CapabilityKind.GenericAdapterCapability:CapabilityKind.DiscoveredCapability,ExecutionAdapter=x.Adapter,MutatesGameState=x.IsGameplayOperation,Availability=!x.IsGameplayOperation||x.IsHostOrInternal?Availability.UnsafeOrInternal:x.RequiresExplicitIntegration?Availability.Unavailable:Availability.Available,Confidence=x.HasSafeGenericAdapter?0.9m:0.6m,Provenance="deterministic-discovery"}).ToArray();}
