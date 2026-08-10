using System.Linq;
using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Job;

public class JobStartHandler : IActionHandler
{
	public string ActionId => "job_start";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn resolvedActor = context.ResolvedActor;
		Map map = context.Map;
		string job = context.ActionCall.Job;
		if (string.IsNullOrEmpty(job))
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "No job specified");
		}
		if (!ActionExecutor.IsJobWhitelisted(job))
		{
			return ExecutionResult.Failed(context, ErrorCode.JobNotInWhitelist, "Job not whitelisted: " + job);
		}
		JobDef namedSilentFail = DefDatabase<JobDef>.GetNamedSilentFail(job);
		if (namedSilentFail == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.InvalidParameters, "JobDef not found: " + job);
		}
		Verse.AI.Job job2 = JobMaker.MakeJob(namedSilentFail);
		if (context.ResolvedTarget != null)
		{
			job2.targetA = context.ResolvedTarget;
		}
		else if (context.ResolvedCell.HasValue)
		{
			job2.targetA = context.ResolvedCell.Value;
		}
		else
		{
			EALogger.Debug("Searching for auto-target for job: " + job);
			LocalTargetInfo localTargetInfo = FindAutoTarget(resolvedActor, map, job);
			if (!(localTargetInfo != null))
			{
				EALogger.Debug($"No target found for {job} - pawn: {resolvedActor.Name.ToStringShort}, map has {(map?.listerThings?.AllThings?.Count()).GetValueOrDefault()} things");
				return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "No suitable target found for job: " + job);
			}
			job2.targetA = localTargetInfo;
			EALogger.Debug($"Auto-found target for {job}: {localTargetInfo}");
		}
		context.StartOrQueueJob(job2);
		EALogger.Debug($"{resolvedActor.Name.ToStringShort} started job: {job} on {job2.targetA}");
		return ExecutionResult.Succeeded(context);
	}

	private LocalTargetInfo FindAutoTarget(Pawn pawn, Map map, string jobDefName)
	{
		if (map == null)
		{
			return null;
		}
		switch (jobDefName)
		{
		case "CutPlant":
		case "Harvest":
		{
			Thing thing2 = (from d in map.designationManager.SpawnedDesignationsOfDef(DesignationDefOf.CutPlant)
				select d.target.Thing into t
				where t != null && pawn.CanReserveAndReach(t, PathEndMode.Touch, Danger.Some)
				orderby t.Position.DistanceTo(pawn.Position)
				select t).FirstOrDefault();
			if (thing2 != null)
			{
				return thing2;
			}
			return (from p in map.listerThings.AllThings.OfType<Plant>().Where(delegate(Plant p)
				{
					PlantProperties plant = p.def.plant;
					return plant != null && plant.IsTree && pawn.CanReserveAndReach(p, PathEndMode.Touch, Danger.Some);
				})
				orderby p.Position.DistanceTo(pawn.Position)
				select p).FirstOrDefault();
		}
		case "Mine":
		{
			Thing thing = (from d in map.designationManager.SpawnedDesignationsOfDef(DesignationDefOf.Mine)
				select d.target.Thing into t
				where t != null && pawn.CanReserveAndReach(t, PathEndMode.Touch, Danger.Some)
				orderby t.Position.DistanceTo(pawn.Position)
				select t).FirstOrDefault();
			if (thing != null)
			{
				return thing;
			}
			return (from t in map.listerThings.AllThings
				where t.def.mineable && pawn.CanReserveAndReach(t, PathEndMode.Touch, Danger.Some)
				orderby t.Position.DistanceTo(pawn.Position)
				select t).FirstOrDefault();
		}
		case "Clean":
			return (from f in map.listerThings.ThingsInGroup(ThingRequestGroup.Filth)
				where pawn.CanReserveAndReach(f, PathEndMode.Touch, Danger.Some)
				orderby f.Position.DistanceTo(pawn.Position)
				select f).FirstOrDefault();
		case "FinishFrame":
			return (from f in map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingFrame)
				where pawn.CanReserveAndReach(f, PathEndMode.Touch, Danger.Some)
				orderby f.Position.DistanceTo(pawn.Position)
				select f).FirstOrDefault();
		case "Haul":
			return (from t in map.listerHaulables.ThingsPotentiallyNeedingHauling()
				where pawn.CanReserveAndReach(t, PathEndMode.Touch, Danger.Some)
				orderby t.Position.DistanceTo(pawn.Position)
				select t).FirstOrDefault();
		default:
			return null;
		}
	}
}
