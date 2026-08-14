using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RimAI.Core.Application;

namespace RimTalk.ExpandActions.Parsing;

/// <summary>
/// Frontend JSON decode into RimAI-owned LegacyStructuredAction.
/// Gameplay validation belongs to RimAI application, not this parser.
/// </summary>
public static class StructuredCapabilityJsonParser
{
	public static FrontendStructuredConversion Parse(string json)
	{
		var result = new FrontendStructuredConversion();
		if (string.IsNullOrWhiteSpace(json))
		{
			result.Errors.Add("Empty JSON response");
			return result;
		}

		try
		{
			json = ExtractJsonFromMarkdown(json);
			var token = JToken.Parse(json);
			IEnumerable<JToken> items;
			if (token is JObject obj && obj["actions"] is JArray array)
				items = array;
			else if (token is JArray root)
				items = root;
			else
			{
				result.Errors.Add("Could not parse structured capability JSON");
				return result;
			}

			foreach (var item in items)
			{
				if (item is not JObject row)
					continue;
				var action = ToAction(row);
				if (string.IsNullOrWhiteSpace(action.Id) || string.IsNullOrWhiteSpace(action.Actor))
				{
					result.Errors.Add("Structured action missing id or actor");
					continue;
				}
				result.Actions.Add(action);
			}
		}
		catch (JsonException ex)
		{
			result.Errors.Add("JSON parse error: " + ex.Message);
		}
		catch (Exception ex)
		{
			result.Errors.Add("Unexpected error: " + ex.Message);
		}

		return result;
	}

	private static LegacyStructuredAction ToAction(JObject row)
	{
		var args = row["args"] as JObject;
		return new LegacyStructuredAction(
			ReadString(row, "id"),
			ReadString(row, "actor"),
			ReadString(row, "target"),
			ReadString(row, "cell"),
			ReadString(row, "thing"),
			ReadInt(args, "quantity"),
			ReadInt(args, "ticks") ?? ReadInt(args, "duration"),
			ReadBool(args, "drafted"),
			ReadString(args, "ability"),
			ReadString(args, "thought"),
			ReadString(args, "state"),
			ReadString(args, "type"),
			ReadString(args, "relation"),
			ReadString(args, "mode"),
			ReadString(row, "reason"));
	}

	private static string ExtractJsonFromMarkdown(string text)
	{
		int fence = text.IndexOf("```json", StringComparison.OrdinalIgnoreCase);
		if (fence < 0)
			fence = text.IndexOf("```", StringComparison.Ordinal);
		if (fence < 0)
			return text.Trim();
		int start = text.IndexOf('\n', fence);
		if (start < 0)
			return text.Trim();
		int end = text.IndexOf("```", start + 1, StringComparison.Ordinal);
		if (end < 0)
			return text.Substring(start).Trim();
		return text.Substring(start, end - start).Trim();
	}

	private static string ReadString(JToken token, string name)
	{
		var value = token?[name]?.ToString();
		return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
	}

	private static int? ReadInt(JObject args, string name)
	{
		if (args?[name] is not JValue value)
			return null;
		try
		{
			return Convert.ToInt32(value.Value);
		}
		catch
		{
			return null;
		}
	}

	private static bool? ReadBool(JObject args, string name)
	{
		if (args?[name] is not JValue value)
			return null;
		try
		{
			return Convert.ToBoolean(value.Value);
		}
		catch
		{
			return null;
		}
	}
}
