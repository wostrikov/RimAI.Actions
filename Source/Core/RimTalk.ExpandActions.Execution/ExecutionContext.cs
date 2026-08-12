using System.Collections.Generic;
using RimTalk.ExpandActions.Parsing;
using RimTalk.ExpandActions.Util;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Execution;

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
		failure = null;
		if (ResolvedActor == null || job == null)
		{
			failure = "Actor or job is null";
			return false;
		}
		int thingIDNumber = ResolvedActor.thingIDNumber;
		if (!EAJobTracker.IsSameConversation(thingIDNumber, ConversationId) && IsFirstActionForActor)
		{
			EAJobTracker.ClearPawnEntries(thingIDNumber, ResolvedActor);
		}
		if (ActionPriority >= 5)
		{
			EALogger.Debug($"[EA] {ResolvedActor.LabelShort}: priority {ActionPriority} (critical), force override");
			ResolvedActor.jobs?.jobQueue?.Clear(ResolvedActor, canReturnToPool: true);
			ResolvedActor.jobs.StartJob(job, JobCondition.InterruptForced);
			EAJobTracker.ClearProtection(thingIDNumber);
			EAJobTracker.RecordJob(thingIDNumber, ConversationId);
			return true;
		}
		bool flag = false;
		if (IsFirstActionForActor && !EAJobTracker.IsSameConversation(thingIDNumber, ConversationId))
		{
			if (EAJobTracker.IsProtected(thingIDNumber))
			{
				if (ActionPriority < 3)
				{
					EALogger.Debug($"[EA] {ResolvedActor.LabelShort}: new conv, priority {ActionPriority} (leisure), pawn busy -> skip");
					failure = "Pawn is protected by a current job; leisure action was not queued";
					return false;
				}
			}
			else
			{
				flag = true;
			}
		}
		if (flag)
		{
			EALogger.Debug($"[EA] {ResolvedActor.LabelShort}: new conv, priority {ActionPriority}, pawn idle -> override");
			ResolvedActor.jobs?.jobQueue?.Clear(ResolvedActor, canReturnToPool: true);
			ResolvedActor.jobs.StartJob(job, JobCondition.InterruptForced);
			EAJobTracker.RecordJob(thingIDNumber, ConversationId);
			return true;
		}
		EAJobTracker.EvictIfFull(thingIDNumber, ResolvedActor);
		if (EAJobTracker.IsQueueFull(ResolvedActor))
		{
			EALogger.Debug("[EA] " + ResolvedActor.LabelShort + ": queue full after eviction, skipping");
			failure = "EA action queue is full";
			return false;
		}
		int num = EAJobTracker.FindInsertPositionInJobQueue(ResolvedActor, ActionPriority);
		int count = ResolvedActor.jobs.jobQueue.Count;
		if (num >= count)
		{
			ResolvedActor.jobs.jobQueue.EnqueueLast(job);
		}
		else
		{
			List<QueuedJob> list = new List<QueuedJob>();
			for (int num2 = count - 1; num2 >= num; num2--)
			{
				list.Add(ResolvedActor.jobs.jobQueue[num2]);
				ResolvedActor.jobs.jobQueue.Dequeue();
			}
			list.Reverse();
			ResolvedActor.jobs.jobQueue.EnqueueLast(job);
			foreach (QueuedJob item in list)
			{
				ResolvedActor.jobs.jobQueue.EnqueueLast(item.job);
			}
		}
		EALogger.Info($"[EA] Priority insert: job {ActionCall?.Id}(P{ActionPriority}) at position {num} in queue of {count + 1}");
		EAJobTracker.RecordJobWithPriority(thingIDNumber, job, ActionPriority, ConversationId, SentenceId);
		EAJobTracker.RecordJob(thingIDNumber, ConversationId);
		return true;
	}
}
