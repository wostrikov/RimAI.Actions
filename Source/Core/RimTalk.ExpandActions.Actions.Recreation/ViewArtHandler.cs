using System.Linq;
using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Recreation;

public class ViewArtHandler : IActionHandler
{
	public string ActionId => "view_art";

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
			thing = (from t in pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Art)
				where pawn.CanReserveAndReach(t, PathEndMode.Touch, Danger.None)
				orderby t.Position.DistanceTo(pawn.Position)
				select t).FirstOrDefault();
		}
		if (thing == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "No art piece found");
		}
		JobDef namedSilentFail = DefDatabase<JobDef>.GetNamedSilentFail("ViewArt");
		if (namedSilentFail == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.ExecutionException, "ViewArt JobDef not found");
		}
		Verse.AI.Job job = JobMaker.MakeJob(namedSilentFail, thing);
		context.StartOrQueueJob(job);
		EALogger.Debug(pawn.Name.ToStringShort + " viewing art " + thing.LabelCap);
		return ExecutionResult.Succeeded(context);
	}
}
