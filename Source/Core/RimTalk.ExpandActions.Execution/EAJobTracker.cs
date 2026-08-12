using System.Collections.Generic;
using System.Linq;
using RimTalk.ExpandActions.Mod;
using RimTalk.ExpandActions.Util;
using Verse;
using Verse.AI;

namespace RimTalk.ExpandActions.Execution;

public static class EAJobTracker
{
	public class PawnActionQueueEntry
	{
		public int PawnId;

		public Job Job;

		public int Priority;

		public int EnqueuedAt;

		public string ConversationId;

		public string SentenceId;

		public string ActionId;
	}

	private struct PawnJobState
	{
		public int LastTick;

		public string ConversationId;
	}

	private static readonly Dictionary<int, PawnJobState> _pawnState = new Dictionary<int, PawnJobState>();

	private static readonly Dictionary<int, List<PawnActionQueueEntry>> _pawnJobEntries = new Dictionary<int, List<PawnActionQueueEntry>>();

	private static int _lastCleanupTick = 0;

	private const int CleanupInterval = 15000;

	private const int MaxEAQueueSize = 10;

	public static void RecordJob(int pawnThingId, string conversationId)
	{
		_pawnState[pawnThingId] = new PawnJobState
		{
			LastTick = (Find.TickManager?.TicksGame ?? 0),
			ConversationId = conversationId
		};
		MaybeCleanup();
	}

	public static bool IsSameConversation(int pawnThingId, string conversationId)
	{
		if (string.IsNullOrEmpty(conversationId))
		{
			return false;
		}
		if (!_pawnState.TryGetValue(pawnThingId, out var value))
		{
			return false;
		}
		return value.ConversationId == conversationId;
	}

	public static bool IsProtected(int pawnThingId)
	{
		EASettings settings = EAModMain.Settings;
		if (settings.JobProtectionTicks <= 0)
		{
			return false;
		}
		if (!_pawnState.TryGetValue(pawnThingId, out var value))
		{
			return false;
		}
		return (Find.TickManager?.TicksGame ?? 0) - value.LastTick < settings.JobProtectionTicks;
	}

	public static bool IsQueueFull(Pawn pawn)
	{
		if (pawn?.jobs?.jobQueue == null)
		{
			return false;
		}
		int thingIDNumber = pawn.thingIDNumber;
		if (!_pawnJobEntries.TryGetValue(thingIDNumber, out var value))
		{
			return false;
		}
		return value.Count >= 10;
	}

	public static List<PawnActionQueueEntry> GetEntriesForPawn(int pawnThingId)
	{
		if (_pawnJobEntries.TryGetValue(pawnThingId, out var value))
		{
			return value;
		}
		return new List<PawnActionQueueEntry>();
	}

	public static void RecordJobWithPriority(int pawnThingId, Job job, int priority, string conversationId, string sentenceId, string actionId = null)
	{
		if (!_pawnJobEntries.ContainsKey(pawnThingId))
		{
			_pawnJobEntries[pawnThingId] = new List<PawnActionQueueEntry>();
		}
		List<PawnActionQueueEntry> list = _pawnJobEntries[pawnThingId];
		PawnActionQueueEntry item = new PawnActionQueueEntry
		{
			PawnId = pawnThingId,
			Job = job,
			Priority = priority,
			EnqueuedAt = (Find.TickManager?.TicksGame ?? 0),
			ConversationId = conversationId,
			SentenceId = sentenceId
			,ActionId = actionId
		};
		int num = 0;
		for (int num2 = list.Count - 1; num2 >= 0; num2--)
		{
			if (list[num2].Priority >= priority)
			{
				num = num2 + 1;
				break;
			}
		}
		list.Insert(num, item);
		EALogger.Debug($"[EA] RecordJobWithPriority: pawn {pawnThingId}, P{priority}, pos {num}/{list.Count}");
	}

	public static PawnActionQueueEntry FindEntry(int pawnThingId, Job job)
	{
		return _pawnJobEntries.TryGetValue(pawnThingId, out var entries)
			? entries.FirstOrDefault(e => e.Job == job)
			: null;
	}

	public static void CompleteEntry(int pawnThingId, Job job, JobCondition condition)
	{
		PawnActionQueueEntry entry = FindEntry(pawnThingId, job);
		if (entry == null) return;
		EALogger.Info($"[EA_TRACE] conv={entry.ConversationId} action={entry.ActionId ?? job.def?.defName} pawn={pawnThingId} state={(condition == JobCondition.Succeeded ? "COMPLETED" : "FAILED")} condition={condition}");
		_pawnJobEntries[pawnThingId].Remove(entry);
	}

	public static int FindInsertPositionInJobQueue(Pawn pawn, int priority)
	{
		int thingIDNumber = pawn.thingIDNumber;
		if (!_pawnJobEntries.TryGetValue(thingIDNumber, out var value) || value.Count == 0)
		{
			return pawn.jobs.jobQueue.Count;
		}
		int num = -1;
		JobQueue jobQueue = pawn.jobs.jobQueue;
		for (int i = 0; i < jobQueue.Count; i++)
		{
			QueuedJob queuedJob = jobQueue[i];
			PawnActionQueueEntry pawnActionQueueEntry = value.FirstOrDefault((PawnActionQueueEntry e) => e.Job == queuedJob.job);
			if (pawnActionQueueEntry != null && pawnActionQueueEntry.Priority >= priority)
			{
				num = i;
			}
		}
		return num + 1;
	}

	public static void RemoveExpiredEntries(int pawnThingId, string currentSentenceId, Pawn pawn)
	{
		if (!_pawnJobEntries.TryGetValue(pawnThingId, out var value))
		{
			return;
		}
		List<PawnActionQueueEntry> list = value.Where((PawnActionQueueEntry e) => !string.IsNullOrEmpty(e.SentenceId) && e.SentenceId != currentSentenceId).ToList();
		if (list.Count == 0)
		{
			return;
		}
		if (pawn?.jobs?.jobQueue != null)
		{
			HashSet<Job> jobsToRemove = new HashSet<Job>(list.Select((PawnActionQueueEntry e) => e.Job));
			RemoveJobsFromQueue(pawn, jobsToRemove);
		}
		foreach (PawnActionQueueEntry item in list)
		{
			value.Remove(item);
		}
		EALogger.Debug($"[EA] Removed {list.Count} expired entries for pawn {pawnThingId}");
	}

	public static bool EvictIfFull(int pawnThingId, Pawn pawn)
	{
		if (!_pawnJobEntries.TryGetValue(pawnThingId, out var value))
		{
			return false;
		}
		if (value.Count < 10)
		{
			return false;
		}
		PawnActionQueueEntry pawnActionQueueEntry = null;
		foreach (PawnActionQueueEntry item in value)
		{
			if (pawnActionQueueEntry == null || item.Priority < pawnActionQueueEntry.Priority || (item.Priority == pawnActionQueueEntry.Priority && item.EnqueuedAt < pawnActionQueueEntry.EnqueuedAt))
			{
				pawnActionQueueEntry = item;
			}
		}
		if (pawnActionQueueEntry == null)
		{
			return false;
		}
		if (pawn?.jobs?.jobQueue != null)
		{
			RemoveJobsFromQueue(pawn, new HashSet<Job> { pawnActionQueueEntry.Job });
		}
		value.Remove(pawnActionQueueEntry);
		EALogger.Debug($"[EA] Evicted P{pawnActionQueueEntry.Priority} job for pawn {pawnThingId} (queue was full)");
		return true;
	}

	public static void ClearPawnEntries(int pawnThingId, Pawn pawn)
	{
		if (!_pawnJobEntries.TryGetValue(pawnThingId, out var value) || value.Count == 0)
		{
			return;
		}
		if (pawn?.jobs?.jobQueue != null)
		{
			HashSet<Job> jobsToRemove = new HashSet<Job>(value.Select((PawnActionQueueEntry e) => e.Job));
			RemoveJobsFromQueue(pawn, jobsToRemove);
		}
		int count = value.Count;
		value.Clear();
		EALogger.Debug($"[EA] Cleared {count} EA entries for pawn {pawnThingId} (new conversation)");
	}

	private static void RemoveJobsFromQueue(Pawn pawn, HashSet<Job> jobsToRemove)
	{
		JobQueue jobQueue = pawn.jobs.jobQueue;
		if (jobQueue.Count == 0)
		{
			return;
		}
		List<QueuedJob> list = new List<QueuedJob>();
		while (jobQueue.Count > 0)
		{
			list.Add(jobQueue[0]);
			jobQueue.Dequeue();
		}
		foreach (QueuedJob item in list)
		{
			if (!jobsToRemove.Contains(item.job))
			{
				jobQueue.EnqueueLast(item.job);
			}
		}
	}

	public static void ClearProtection(int pawnThingId)
	{
		_pawnState.Remove(pawnThingId);
	}

	private static void MaybeCleanup()
	{
		int currentTick = Find.TickManager?.TicksGame ?? 0;
		if (currentTick - _lastCleanupTick < 15000)
		{
			return;
		}
		_lastCleanupTick = currentTick;
		int jobProtectionTicks = EAModMain.Settings.JobProtectionTicks;
		int expireThreshold = jobProtectionTicks * 2;
		foreach (int item in (from kv in _pawnState
			where currentTick - kv.Value.LastTick > expireThreshold
			select kv.Key).ToList())
		{
			_pawnState.Remove(item);
		}
	}
}
