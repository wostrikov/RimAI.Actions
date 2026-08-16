using RimAI.RimWorld.Jobs;
using Ustas.RimAI.Actions.Parsing;
using Ustas.RimAI.Actions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace Ustas.RimAI.Actions.Execution;

public class ExecutionContext
{
	public string ConversationId { get; set; }

	public ActionCall ActionCall { get; set; }

	public Pawn ResolvedActor { get; set; }

	public Thing ResolvedTarget { get; set; }

	public IntVec3? ResolvedCell { get; set; }

	public Map Map { get; set; }

	public int StartTick { get; set; }

	public bool IsFirstActionForActor { get; set; } = true;

	public int ActionPriority { get; set; } = 3;

	public Pawn TargetPawn => ResolvedTarget as Pawn;

	public string SentenceId { get; set; }

	public bool IsActorValid()
	{
		if (ResolvedActor != null && !ResolvedActor.Dead)
		{
			return !ResolvedActor.Destroyed;
		}
		return false;
	}

	public bool CanActorAct()
	{
		if (!IsActorValid())
		{
			return false;
		}
		if (!ResolvedActor.Downed)
		{
			return !ResolvedActor.InMentalState;
		}
		return false;
	}

	public bool IsActorDrafted()
	{
		if (!IsActorValid())
		{
			return false;
		}
		return ResolvedActor.drafter?.Drafted ?? false;
	}

	public bool IsActorOnWorkSchedule()
	{
		if (!IsActorValid())
		{
			return false;
		}
		return ResolvedActor.timetable?.CurrentAssignment == TimeAssignmentDefOf.Work;
	}

	public void StartOrQueueJob(Job job)
	{
		TryStartOrQueueJob(job, out _);
	}

	public bool TryStartOrQueueJob(Job job, out string failure)
	{
		return RimAIJobQueue.TrySubmit(
			ResolvedActor,
			job,
			new JobDispatchMetadata(
				ConversationId,
				SentenceId,
				ActionCall?.Id,
				IsFirstActionForActor,
				ActionPriority,
				Ustas.RimAI.Actions.Mod.EAModMain.Settings.JobProtectionTicks),
			out failure);
	}
}
