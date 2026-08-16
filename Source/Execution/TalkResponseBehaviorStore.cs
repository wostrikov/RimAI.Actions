using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Ustas.RimAI.Actions.Util;

namespace Ustas.RimAI.Actions.Execution;

public static class TalkResponseBehaviorStore
{
	public class BehaviorEntry
	{
		public List<string> Behaviors { get; set; }

		public string ConversationId { get; set; }

		public string SpeakerName { get; set; }

		public DateTime CreatedAt { get; set; }
	}

	private static readonly ConcurrentDictionary<string, BehaviorEntry> _store = new ConcurrentDictionary<string, BehaviorEntry>(StringComparer.OrdinalIgnoreCase);

	private static readonly TimeSpan ExpiryTimeout = TimeSpan.FromSeconds(120.0);

	public static void Store(string speakerName, List<string> behaviors, string conversationId)
	{
		if (!string.IsNullOrEmpty(speakerName) && behaviors != null && behaviors.Count != 0)
		{
			BehaviorEntry value = new BehaviorEntry
			{
				Behaviors = behaviors,
				ConversationId = conversationId,
				SpeakerName = speakerName,
				CreatedAt = DateTime.UtcNow
			};
			_store[speakerName] = value;
			EALogger.Debug($"Stored {behaviors.Count} behaviors for {speakerName}");
			CleanExpired();
		}
	}

	public static BehaviorEntry TryGetAndRemove(string speakerName)
	{
		if (string.IsNullOrEmpty(speakerName))
		{
			return null;
		}
		if (_store.TryRemove(speakerName, out var value))
		{
			if (DateTime.UtcNow - value.CreatedAt > ExpiryTimeout)
			{
				EALogger.Debug($"Expired behavior entry for {value.SpeakerName} (age: {(DateTime.UtcNow - value.CreatedAt).TotalSeconds:F0}s)");
				return null;
			}
			return value;
		}
		return null;
	}

	public static void CleanExpired()
	{
		DateTime utcNow = DateTime.UtcNow;
		foreach (KeyValuePair<string, BehaviorEntry> item in _store)
		{
			if (utcNow - item.Value.CreatedAt > ExpiryTimeout)
			{
				_store.TryRemove(item.Key, out var _);
			}
		}
	}
}
