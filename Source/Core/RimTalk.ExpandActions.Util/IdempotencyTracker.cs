using System.Collections.Generic;
using RimTalk.ExpandActions.Parsing;

namespace RimTalk.ExpandActions.Util;

public static class IdempotencyTracker
{
	private static readonly Dictionary<string, HashSet<string>> _executed = new Dictionary<string, HashSet<string>>();

	private static readonly Queue<string> _conversationOrder = new Queue<string>();

	private const int MaxConversations = 100;

	public static bool TryMarkExecuted(string conversationId, ActionCall action)
	{
		string signature = action.GetSignature();
		lock (_executed)
		{
			if (!_executed.TryGetValue(conversationId, out var value))
			{
				value = new HashSet<string>();
				_executed[conversationId] = value;
				_conversationOrder.Enqueue(conversationId);
				CleanOldEntries();
			}
			return value.Add(signature);
		}
	}

	public static bool WasExecuted(string conversationId, ActionCall action)
	{
		string signature = action.GetSignature();
		lock (_executed)
		{
			HashSet<string> value;
			return _executed.TryGetValue(conversationId, out value) && value.Contains(signature);
		}
	}

	public static void ClearConversation(string conversationId)
	{
		lock (_executed)
		{
			_executed.Remove(conversationId);
		}
	}

	private static void CleanOldEntries()
	{
		while (_conversationOrder.Count > 100 && _conversationOrder.Count > 0)
		{
			string key = _conversationOrder.Dequeue();
			_executed.Remove(key);
		}
	}

	public static void Reset()
	{
		lock (_executed)
		{
			_executed.Clear();
			_conversationOrder.Clear();
		}
	}
}
