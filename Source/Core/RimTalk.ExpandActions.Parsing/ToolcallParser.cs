using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RimTalk.ExpandActions.Util;

namespace RimTalk.ExpandActions.Parsing;

public static class ToolcallParser
{
	public static ToolcallResponse Parse(string json)
	{
		ToolcallResponse toolcallResponse = new ToolcallResponse
		{
			RawJson = json
		};
		if (string.IsNullOrWhiteSpace(json))
		{
			toolcallResponse.ParseErrors.Add("Empty JSON response");
			return toolcallResponse;
		}
		try
		{
			json = ExtractJsonFromMarkdown(json);
			ToolcallResponse toolcallResponse2 = JsonConvert.DeserializeObject<ToolcallResponse>(json);
			if (toolcallResponse2?.Actions != null)
			{
				toolcallResponse.Actions = toolcallResponse2.Actions;
				ValidateActions(toolcallResponse);
				return toolcallResponse;
			}
			List<ActionCall> list = JsonConvert.DeserializeObject<List<ActionCall>>(json);
			if (list != null)
			{
				toolcallResponse.Actions = list;
				ValidateActions(toolcallResponse);
				return toolcallResponse;
			}
			toolcallResponse.ParseErrors.Add("Could not parse JSON as ToolcallResponse or ActionCall array");
		}
		catch (JsonException ex)
		{
			toolcallResponse.ParseErrors.Add("JSON parse error: " + ex.Message);
			EALogger.Debug("Toolcall parse error: " + ex.Message + "\nJSON: " + json);
		}
		catch (Exception ex2)
		{
			toolcallResponse.ParseErrors.Add("Unexpected error: " + ex2.Message);
			EALogger.Error("Toolcall parse exception", ex2);
		}
		return toolcallResponse;
	}

	private static string ExtractJsonFromMarkdown(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return text;
		}
		int num = text.IndexOf("```json", StringComparison.OrdinalIgnoreCase);
		if (num >= 0)
		{
			int num2 = text.IndexOf('\n', num);
			if (num2 >= 0)
			{
				int num3 = text.IndexOf("```", num2);
				if (num3 > num2)
				{
					return text.Substring(num2, num3 - num2).Trim();
				}
			}
		}
		int num4 = text.IndexOf("```");
		if (num4 >= 0)
		{
			int num5 = text.IndexOf('\n', num4);
			if (num5 >= 0)
			{
				int num6 = text.IndexOf("```", num5);
				if (num6 > num5)
				{
					string text2 = text.Substring(num5, num6 - num5).Trim();
					if (text2.StartsWith("{") || text2.StartsWith("["))
					{
						return text2;
					}
				}
			}
		}
		return text.Trim();
	}

	private static void ValidateActions(ToolcallResponse response)
	{
		List<ActionCall> list = new List<ActionCall>();
		foreach (ActionCall action in response.Actions)
		{
			List<string> list2 = ValidateAction(action);
			if (list2.Count == 0)
			{
				list.Add(action);
				continue;
			}
			foreach (string item in list2)
			{
				response.ParseErrors.Add(item);
			}
		}
		response.Actions = list;
	}

	private static List<string> ValidateAction(ActionCall action)
	{
		List<string> list = new List<string>();
		if (string.IsNullOrWhiteSpace(action.Id))
		{
			list.Add("Action missing 'id' field");
		}
		if (string.IsNullOrWhiteSpace(action.Actor))
		{
			list.Add("Action '" + action.Id + "' missing 'actor' field");
		}
		return list;
	}
}
