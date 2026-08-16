using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Ustas.RimAI.Actions.Util;

namespace Ustas.RimAI.Actions.Execution;

public static class MainThreadDispatcher
{
	private static readonly ConcurrentQueue<Action> _actions = new ConcurrentQueue<Action>();

	private const int MaxActionsPerTick = 5;

	public static int PendingCount => _actions.Count;

	public static void Enqueue(Action action)
	{
		if (action != null)
		{
			_actions.Enqueue(action);
		}
	}

	public static void ProcessQueue()
	{
		List<Action> list = new List<Action>();
		Action result;
		while (list.Count < 5 && _actions.TryDequeue(out result))
		{
			list.Add(result);
		}
		foreach (Action item in list)
		{
			try
			{
				item();
			}
			catch (Exception ex)
			{
				EALogger.Error("Error executing queued action", ex);
			}
		}
		if (list.Count > 0 && _actions.Count > 0)
		{
			EALogger.Debug($"Processed {list.Count} actions, remaining: {_actions.Count}");
		}
	}

	public static void Clear()
	{
		Action result;
		while (_actions.TryDequeue(out result))
		{
		}
		EALogger.Debug("Cleared all pending actions");
	}
}
