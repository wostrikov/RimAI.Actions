using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using RimTalk.ExpandActions.Util;

namespace RimTalk.ExpandActions.Parsing;

public static class EaObservedParser
{
	public static List<string> Parse(string json)
	{
		List<string> list = new List<string>();
		if (string.IsNullOrWhiteSpace(json))
		{
			return list;
		}
		try
		{
			JToken jToken = JObject.Parse(json)["ea_observed"];
			if (jToken == null)
			{
				return list;
			}
			if (jToken.Type == JTokenType.Array)
			{
				foreach (JToken item in (IEnumerable<JToken>)jToken)
				{
					string text = item.ToString();
					if (!string.IsNullOrWhiteSpace(text))
					{
						list.Add(text.Trim());
					}
				}
			}
			else if (jToken.Type == JTokenType.String)
			{
				string text2 = jToken.ToString();
				if (!string.IsNullOrWhiteSpace(text2))
				{
					list.Add(text2.Trim());
				}
			}
		}
		catch (Exception ex)
		{
			EALogger.Debug("Failed to parse ea_observed: " + ex.Message);
		}
		return list;
	}

	public static string GetSpeakerName(string json)
	{
		if (string.IsNullOrWhiteSpace(json))
		{
			return null;
		}
		try
		{
			return JObject.Parse(json)["name"]?.ToString();
		}
		catch (Exception ex)
		{
			EALogger.Debug("ExtractSpeakerFromJson failed: " + ex.Message);
			return null;
		}
	}
}
