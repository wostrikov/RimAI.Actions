using System.Linq;
using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Recreation;

public class PlayMusicHandler : IActionHandler
{
	public string ActionId => "play_music";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn pawn = context.ResolvedActor;
		Thing thing = context.ResolvedTarget;
		if (!context.CanActorAct())
		{
			return ExecutionResult.Failed(context, ErrorCode.ActorIncapable, "Actor cannot act");
		}
		if (thing == null)
		{
			thing = (from b in pawn.Map.listerBuildings.AllBuildingsColonistOfClass<Building>()
				where b.def.building?.joyKind?.defName == "Music" || b.def.defName.Contains("Harp") || b.def.defName.Contains("Piano") || b.def.defName.Contains("Guitar")
				where pawn.CanReserveAndReach(b, PathEndMode.InteractionCell, Danger.None)
				orderby b.Position.DistanceTo(pawn.Position)
				select b).FirstOrDefault();
		}
		if (thing == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "No musical instrument found");
		}
		JobDef namedSilentFail = DefDatabase<JobDef>.GetNamedSilentFail("Play_MusicalInstrument");
		if (namedSilentFail == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.ExecutionException, "Play_MusicalInstrument JobDef not found");
		}
		Verse.AI.Job job = JobMaker.MakeJob(namedSilentFail, thing);
		context.StartOrQueueJob(job);
		EALogger.Debug(pawn.Name.ToStringShort + " playing music on " + thing.LabelCap);
		return ExecutionResult.Succeeded(context);
	}
}
