using System.Linq;
using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Actions.Production;

public class CleanHandler : IActionHandler
{
	public string ActionId => "clean";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn pawn = context.ResolvedActor;
		if (!context.CanActorAct())
		{
			return ExecutionResult.Failed(context, ErrorCode.ActorIncapable, "Actor cannot act");
		}
		Thing thing = (from f in pawn.Map.listerFilthInHomeArea.FilthInHomeArea
			where pawn.CanReserveAndReach(f, PathEndMode.Touch, Danger.None)
			orderby f.Position.DistanceTo(pawn.Position)
			select f).FirstOrDefault();
		if (thing == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.TargetNotFound, "No filth to clean nearby");
		}
		Verse.AI.Job job = JobMaker.MakeJob(JobDefOf.Clean, thing);
		context.StartOrQueueJob(job);
		EALogger.Debug(pawn.Name.ToStringShort + " cleaning");
		return ExecutionResult.Succeeded(context);
	}
}
