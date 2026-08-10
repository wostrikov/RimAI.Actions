using System.Linq;
using RimTalk.ExpandActions.Mod;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Util;

public static class NearbyTargetFinder
{
	private const float MaxSearchRadius = 50f;

	public static Thing FindMineable(Pawn pawn)
	{
		Map map = pawn.Map;
		Thing thing = (from d in map.designationManager.SpawnedDesignationsOfDef(DesignationDefOf.Mine)
			select d.target.Thing into t
			where t != null && t.def.mineable && t.Position.DistanceTo(pawn.Position) < 50f && pawn.CanReserveAndReach(t, PathEndMode.Touch, Danger.Some)
			orderby t.Position.DistanceTo(pawn.Position)
			select t).FirstOrDefault();
		if (thing != null)
		{
			return thing;
		}
		if (!EAModMain.Settings.AllowUndesignatedTargets)
		{
			EALogger.Debug("[NearbyTargetFinder] No designated mineable found, undesignated disabled");
			return null;
		}
		return GenClosest.ClosestThingReachable(pawn.Position, map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.Touch, TraverseParms.For(pawn, Danger.Some), 50f, (Thing t) => t.def.mineable && pawn.CanReserve(t));
	}

	public static Plant FindCuttablePlant(Pawn pawn)
	{
		Map map = pawn.Map;
		Plant plant = (from p in (from d in map.designationManager.SpawnedDesignationsOfDef(DesignationDefOf.CutPlant)
				select d.target.Thing).OfType<Plant>()
			where p.Position.DistanceTo(pawn.Position) < 50f && pawn.CanReserveAndReach(p, PathEndMode.Touch, Danger.Some)
			orderby p.Position.DistanceTo(pawn.Position)
			select p).FirstOrDefault();
		if (plant != null)
		{
			return plant;
		}
		if (!EAModMain.Settings.AllowUndesignatedTargets)
		{
			EALogger.Debug("[NearbyTargetFinder] No designated cuttable plant found, undesignated disabled");
			return null;
		}
		Plant plant2 = (from p in map.listerThings.ThingsInGroup(ThingRequestGroup.Plant).OfType<Plant>().Where(delegate(Plant p)
			{
				PlantProperties plant3 = p.def.plant;
				return plant3 != null && plant3.IsTree && p.Position.DistanceTo(pawn.Position) < 50f && pawn.CanReserveAndReach(p, PathEndMode.Touch, Danger.Some);
			})
			orderby p.Position.DistanceTo(pawn.Position)
			select p).FirstOrDefault();
		if (plant2 != null)
		{
			return plant2;
		}
		return (from p in map.listerThings.ThingsInGroup(ThingRequestGroup.Plant).OfType<Plant>()
			where p.Position.DistanceTo(pawn.Position) < 50f && pawn.CanReserveAndReach(p, PathEndMode.Touch, Danger.Some)
			orderby p.Position.DistanceTo(pawn.Position)
			select p).FirstOrDefault();
	}

	public static Plant FindHarvestable(Pawn pawn)
	{
		Map map = pawn.Map;
		DesignationManager designationManager = map.designationManager;
		DesignationDef namedSilentFail = DefDatabase<DesignationDef>.GetNamedSilentFail("HarvestPlant");
		if (namedSilentFail != null)
		{
			Plant plant = (from p in (from d in designationManager.SpawnedDesignationsOfDef(namedSilentFail)
					select d.target.Thing).OfType<Plant>()
				where p.HarvestableNow && p.Position.DistanceTo(pawn.Position) < 50f && pawn.CanReserveAndReach(p, PathEndMode.Touch, Danger.Some)
				orderby p.Position.DistanceTo(pawn.Position)
				select p).FirstOrDefault();
			if (plant != null)
			{
				return plant;
			}
		}
		if (!EAModMain.Settings.AllowUndesignatedTargets)
		{
			EALogger.Debug("[NearbyTargetFinder] No designated harvestable found, undesignated disabled");
			return null;
		}
		return (from p in map.listerThings.ThingsInGroup(ThingRequestGroup.Plant).OfType<Plant>()
			where p.HarvestableNow && p.Position.DistanceTo(pawn.Position) < 50f && pawn.CanReserveAndReach(p, PathEndMode.Touch, Danger.Some)
			orderby p.Position.DistanceTo(pawn.Position)
			select p).FirstOrDefault();
	}

	public static Thing FindHaulable(Pawn pawn)
	{
		return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.HaulableAlways), PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Some), 50f, (Thing t) => !t.IsInValidStorage() && HaulAIUtility.PawnCanAutomaticallyHaulFast(pawn, t, forced: false));
	}

	public static Building FindRepairable(Pawn pawn)
	{
		return (from b in pawn.Map.listerBuildingsRepairable.RepairableBuildings(pawn.Faction).OfType<Building>()
			where b.HitPoints < b.MaxHitPoints && b.Position.DistanceTo(pawn.Position) < 50f && pawn.CanReserveAndReach(b, PathEndMode.Touch, Danger.Some)
			orderby b.Position.DistanceTo(pawn.Position)
			select b).FirstOrDefault();
	}

	public static Thing FindRefuelable(Pawn pawn)
	{
		return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Refuelable), PathEndMode.Touch, TraverseParms.For(pawn, Danger.Some), 50f, delegate(Thing t)
		{
			CompRefuelable compRefuelable = t.TryGetComp<CompRefuelable>();
			return compRefuelable != null && !compRefuelable.IsFull && pawn.CanReserve(t);
		});
	}

	public static Thing FindBrokenDown(Pawn pawn)
	{
		return (from t in pawn.Map.listerThings.AllThings.Where(delegate(Thing t)
			{
				CompBreakdownable compBreakdownable = t.TryGetComp<CompBreakdownable>();
				return compBreakdownable != null && compBreakdownable.BrokenDown && t.Position.DistanceTo(pawn.Position) < 50f && pawn.CanReserveAndReach(t, PathEndMode.Touch, Danger.Some);
			})
			orderby t.Position.DistanceTo(pawn.Position)
			select t).FirstOrDefault();
	}

	public static IntVec3? FindSowableCell(Pawn pawn)
	{
		foreach (Zone_Growing zone in pawn.Map.zoneManager.AllZones.OfType<Zone_Growing>())
		{
			IntVec3 intVec = (from c in zone.cells
				where c.GetPlant(pawn.Map) == null && zone.GetPlantDefToGrow() != null && c.DistanceTo(pawn.Position) < 50f && pawn.CanReach(c, PathEndMode.Touch, Danger.Some)
				orderby c.DistanceTo(pawn.Position)
				select c).FirstOrDefault();
			if (intVec != default(IntVec3))
			{
				return intVec;
			}
		}
		return null;
	}
}
