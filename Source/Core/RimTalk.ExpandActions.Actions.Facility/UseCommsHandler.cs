using System.Linq;
using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Facility;

public class UseCommsHandler : IActionHandler
{
	public string ActionId => "use_comms";

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
			thing = (from b in pawn.Map.listerBuildings.AllBuildingsColonistOfClass<Building_CommsConsole>()
				where pawn.CanReserveAndReach(b, PathEndMode.InteractionCell, Danger.None)
				orderby b.Position.DistanceTo(pawn.Position)
				select b).FirstOrDefault();
		}
		if (thing == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "No comms console found");
		}
		if (!pawn.CanReserveAndReach(thing, PathEndMode.InteractionCell, Danger.None))
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetUnreachable, "Cannot reach comms console");
		}
		Verse.AI.Job job = JobMaker.MakeJob(JobDefOf.UseCommsConsole, thing);
		context.StartOrQueueJob(job);
		EALogger.Debug(pawn.Name.ToStringShort + " using comms console");
		return ExecutionResult.Succeeded(context);
	}
}
