using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using RimTalk.ExpandActions.Execution;
using RimTalk.ExpandActions.Mod;
using RimTalk.ExpandActions.Util;

namespace RimTalk.ExpandActions.Patches;

[HarmonyPatch]
public static class Patch_DeserializeFromJson
{
	private static Type _talkResponseType;

	[HarmonyTargetMethod]
	public static MethodBase TargetMethod()
	{
		Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((Assembly a) => a.GetName().Name == "RimTalk");
		if (assembly == null)
		{
			EALogger.Warn("RimTalk assembly not found for Sanitize patch");
			return null;
		}
		Type type = assembly.GetType("RimTalk.Util.JsonUtil");
		if (type == null)
		{
			EALogger.Warn("JsonUtil type not found");
			return null;
		}
		_talkResponseType = assembly.GetType("RimTalk.Data.TalkResponse");
		if (_talkResponseType == null)
		{
			EALogger.Warn("TalkResponse type not found — cannot patch Sanitize");
			return null;
		}
		MethodInfo method = type.GetMethod("Sanitize", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[2]
		{
			typeof(string),
			typeof(Type)
		}, null);
		if (method == null)
		{
			EALogger.Warn("Sanitize method not found");
			return null;
		}
		EALogger.Info("Found JsonUtil.Sanitize for patching (non-generic, safe from Mono sharing)");
		return method;
	}

	[HarmonyPrefix]
	public static void Prefix(string text, Type targetType)
	{
		if (!EAModMain.Settings.Enabled || _talkResponseType == null || targetType != _talkResponseType || string.IsNullOrEmpty(text) || !text.Contains("ea_observed"))
		{
			return;
		}
		try
		{
			ExtractAndStore(text);
		}
		catch (Exception ex)
		{
			EALogger.Debug("Error extracting ea_observed: " + ex.Message);
		}
	}

	private static void ExtractAndStore(string json)
	{
		JObject jObject;
		try
		{
			jObject = JObject.Parse(json);
		}
		catch
		{
			return;
		}
		JToken jToken = jObject["ea_observed"];
		if (jToken == null || jToken.Type != JTokenType.Array)
		{
			return;
		}
		JArray jArray = (JArray)jToken;
		if (jArray.Count == 0)
		{
			return;
		}
		string text = jObject["name"]?.ToString();
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		List<string> list = new List<string>();
		foreach (JToken item in jArray)
		{
			string text2 = item.ToString();
			if (!string.IsNullOrWhiteSpace(text2))
			{
				list.Add(text2);
			}
		}
		if (list.Count != 0)
		{
			string speakerPrefix = text + " ";
			List<string> list2 = list.Where((string b) => b.StartsWith(speakerPrefix, StringComparison.OrdinalIgnoreCase)).ToList();
			if (list2.Count == 0)
			{
				EALogger.Debug("Sanitize: " + text + " — no speaker-own behaviors in ea_observed (skipped)");
				return;
			}
			string conversationId = $"conv_{DateTime.UtcNow.Ticks}";
			TalkResponseBehaviorStore.Store(text, list2, conversationId);
			EALogger.Info($"Sanitize: stored {list2.Count} behaviors for {text}");
		}
	}
}
