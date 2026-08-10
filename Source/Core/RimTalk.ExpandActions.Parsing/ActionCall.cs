using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RimTalk.ExpandActions.Util;

namespace RimTalk.ExpandActions.Parsing;

public class ActionCall
{
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("actor")]
	public string Actor { get; set; }

	[JsonProperty("target")]
	public string Target { get; set; }

	[JsonProperty("cell")]
	public string Cell { get; set; }

	[JsonProperty("job")]
	public string Job { get; set; }

	[JsonProperty("thing")]
	public string Thing { get; set; }

	[JsonProperty("args")]
	public Dictionary<string, object> Args { get; set; } = new Dictionary<string, object>();

	[JsonProperty("priority")]
	public int Priority { get; set; }

	[JsonProperty("reason")]
	public string Reason { get; set; }

	public T GetArg<T>(string key, T defaultValue = default(T))
	{
		if (Args == null || !Args.TryGetValue(key, out var value))
		{
			return defaultValue;
		}
		if (value is T)
		{
			return (T)value;
		}
		try
		{
			return (T)Convert.ChangeType(value, typeof(T));
		}
		catch (Exception ex)
		{
			EALogger.Debug("ActionCall.GetArg<" + typeof(T).Name + ">(" + key + ") conversion failed: " + ex.Message);
			return defaultValue;
		}
	}

	public string GetSignature()
	{
		return Id + "|" + Actor + "|" + Target;
	}
}
