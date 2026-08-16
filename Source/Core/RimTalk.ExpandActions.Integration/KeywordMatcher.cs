using System.Collections.Generic;
using System.Linq;
using Ustas.RimAI.Actions.Core;

namespace Ustas.RimAI.Actions.Integration;

public static class KeywordMatcher
{
	private static readonly HashSet<string> DefaultActionIds = new HashSet<string> { "move_to", "stop", "wait", "follow", "rescue", "tend", "visit_sick" };

	public static List<ActionDefinition> Match(string text, IEnumerable<ActionDefinition> allActions)
	{
		if (string.IsNullOrEmpty(text))
		{
			return GetDefaultActions(allActions);
		}
		string text2 = text.ToLowerInvariant();
		Dictionary<string, ActionDefinition> dictionary = new Dictionary<string, ActionDefinition>();
		foreach (ActionDefinition allAction in allActions)
		{
			if (DefaultActionIds.Contains(allAction.Id))
			{
				dictionary[allAction.Id] = allAction;
				continue;
			}
			List<string> allMatchKeywords = KeywordConfigManager.GetAllMatchKeywords(allAction.Id);
			if (allMatchKeywords.Count == 0)
			{
				continue;
			}
			foreach (string item in allMatchKeywords)
			{
				if (text2.Contains(item.ToLowerInvariant()))
				{
					dictionary[allAction.Id] = allAction;
					break;
				}
			}
		}
		return dictionary.Values.ToList();
	}

	public static List<ActionDefinition> GetDefaultActions(IEnumerable<ActionDefinition> allActions)
	{
		return allActions.Where((ActionDefinition a) => DefaultActionIds.Contains(a.Id)).ToList();
	}
}
