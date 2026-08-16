using System;
using System.Collections.Generic;
using System.Linq;
using Ustas.RimAI.Actions.Mod;
using Ustas.RimAI.Actions.Util;
using Verse;

namespace Ustas.RimAI.Actions.Core;

public static class KeywordConfigManager
{
	private static Dictionary<string, Dictionary<string, List<string>>> _defaults;

	private static Dictionary<string, HashSet<string>> _movementVerbs;

	private static Dictionary<string, Dictionary<string, string>> _targetNounKeywords;

	private static HashSet<string> _knownLanguages;

	private static bool _loaded = false;

	private static readonly object _lock = new object();

	private static readonly string[] TargetNounActionIds = new string[8] { "mine", "cut_plant", "sow", "research", "clean", "hunt", "repair", "craft" };

	private static void EnsureLoaded()
	{
		if (_loaded)
		{
			return;
		}
		lock (_lock)
		{
			if (!_loaded)
			{
				LoadDefaults();
				_loaded = true;
			}
		}
	}

	public static void Reload()
	{
		lock (_lock)
		{
			_loaded = false;
			_defaults = null;
			_movementVerbs = null;
			_targetNounKeywords = null;
			_knownLanguages = null;
		}
		EnsureLoaded();
	}

	public static string GetCurrentLanguage()
	{
		return LanguageDatabase.activeLanguage?.folderName ?? "English";
	}

	public static List<string> GetPromptKeywords(string actionId, string lang = null)
	{
		EnsureLoaded();
		if (string.IsNullOrEmpty(actionId))
		{
			return new List<string>();
		}
		lang = lang ?? GetCurrentLanguage();
		ActionConfig actionConfig = EAModMain.Settings?.GetActionConfig(actionId);
		if (actionConfig != null && !actionConfig.KeywordEnabled)
		{
			return new List<string>();
		}
		if (actionConfig?.PromptKeywordOverrides != null && actionConfig.PromptKeywordOverrides.TryGetValue(lang, out var value) && !string.IsNullOrEmpty(value))
		{
			return ParseKeywordString(value);
		}
		if (_defaults != null && _defaults.TryGetValue(actionId, out var value2) && value2.TryGetValue(lang, out var value3) && value3.Count > 0)
		{
			return new List<string>(value3);
		}
		ActionDefinition byId = ActionRegistry.GetById(actionId);
		if (byId?.Keywords != null && byId.Keywords.Count > 0)
		{
			return new List<string>(byId.Keywords);
		}
		return new List<string>();
	}

	public static List<string> GetAllMatchKeywords(string actionId)
	{
		EnsureLoaded();
		if (string.IsNullOrEmpty(actionId))
		{
			return new List<string>();
		}
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		hashSet.Add(actionId);
		ActionConfig actionConfig = EAModMain.Settings?.GetActionConfig(actionId);
		if (actionConfig != null && !actionConfig.KeywordEnabled)
		{
			return hashSet.ToList();
		}
		if (_defaults != null && _defaults.TryGetValue(actionId, out var value))
		{
			foreach (KeyValuePair<string, List<string>> item in value)
			{
				foreach (string item2 in item.Value)
				{
					hashSet.Add(item2);
				}
			}
		}
		ActionDefinition byId = ActionRegistry.GetById(actionId);
		if (byId?.Keywords != null)
		{
			foreach (string keyword in byId.Keywords)
			{
				hashSet.Add(keyword);
			}
		}
		if (actionConfig?.PromptKeywordOverrides != null)
		{
			foreach (KeyValuePair<string, string> promptKeywordOverride in actionConfig.PromptKeywordOverrides)
			{
				if (string.IsNullOrEmpty(promptKeywordOverride.Value))
				{
					continue;
				}
				foreach (string item3 in ParseKeywordString(promptKeywordOverride.Value))
				{
					hashSet.Add(item3);
				}
			}
		}
		return hashSet.ToList();
	}

	public static HashSet<string> GetMovementVerbs(string lang = null)
	{
		EnsureLoaded();
		lang = lang ?? GetCurrentLanguage();
		if (_movementVerbs != null && _movementVerbs.TryGetValue(lang, out var value))
		{
			return new HashSet<string>(value);
		}
		if (_movementVerbs != null && _movementVerbs.TryGetValue("English", out var value2))
		{
			return new HashSet<string>(value2);
		}
		return new HashSet<string>();
	}

	public static HashSet<string> GetAllMovementVerbs()
	{
		EnsureLoaded();
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		if (_movementVerbs != null)
		{
			foreach (KeyValuePair<string, HashSet<string>> movementVerb in _movementVerbs)
			{
				foreach (string item in movementVerb.Value)
				{
					hashSet.Add(item);
				}
			}
		}
		return hashSet;
	}

	public static Dictionary<string, string> GetTargetNounKeywords(string lang = null)
	{
		EnsureLoaded();
		lang = lang ?? GetCurrentLanguage();
		if (_targetNounKeywords != null && _targetNounKeywords.TryGetValue(lang, out var value))
		{
			return new Dictionary<string, string>(value);
		}
		if (_targetNounKeywords != null && _targetNounKeywords.TryGetValue("English", out var value2))
		{
			return new Dictionary<string, string>(value2);
		}
		return new Dictionary<string, string>();
	}

	public static Dictionary<string, string> GetAllTargetNounKeywords()
	{
		EnsureLoaded();
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		if (_targetNounKeywords != null)
		{
			foreach (KeyValuePair<string, Dictionary<string, string>> targetNounKeyword in _targetNounKeywords)
			{
				foreach (KeyValuePair<string, string> item in targetNounKeyword.Value)
				{
					dictionary[item.Key] = item.Value;
				}
			}
		}
		return dictionary;
	}

	public static void ResetToDefault(string actionId, string lang = null)
	{
		lang = lang ?? GetCurrentLanguage();
		ActionConfig actionConfig = EAModMain.Settings?.GetActionConfig(actionId);
		if (actionConfig?.PromptKeywordOverrides != null)
		{
			actionConfig.PromptKeywordOverrides.Remove(lang);
			if (actionConfig.PromptKeywordOverrides.Count == 0)
			{
				actionConfig.PromptKeywordOverrides = null;
			}
		}
	}

	public static IEnumerable<string> GetKnownLanguages()
	{
		EnsureLoaded();
		IEnumerable<string> knownLanguages = _knownLanguages;
		return knownLanguages ?? Enumerable.Empty<string>();
	}

	private static void LoadDefaults()
	{
		_defaults = new Dictionary<string, Dictionary<string, List<string>>>();
		_movementVerbs = new Dictionary<string, HashSet<string>>();
		_targetNounKeywords = new Dictionary<string, Dictionary<string, string>>();
		_knownLanguages = new HashSet<string>();
		foreach (LoadedLanguage allLoadedLanguage in LanguageDatabase.AllLoadedLanguages)
		{
			string folderName = allLoadedLanguage.folderName;
			if (!string.IsNullOrEmpty(folderName))
			{
				LoadLanguageKeywords(folderName);
				LoadMovementVerbs(folderName);
				LoadTargetNounKeywords(folderName);
			}
		}
		int count = _defaults.Count;
		int count2 = _knownLanguages.Count;
		EALogger.Info(string.Format("KeywordConfigManager loaded: {0} actions, {1} languages ({2})", count, count2, string.Join(", ", _knownLanguages)));
	}

	private static void LoadLanguageKeywords(string langFolderName)
	{
		foreach (ActionDefinition item in ActionRegistry.GetAll())
		{
			string text = TryTranslateForLanguage("EA_KW_" + item.Id, langFolderName);
			if (text != null)
			{
				if (!_defaults.ContainsKey(item.Id))
				{
					_defaults[item.Id] = new Dictionary<string, List<string>>();
				}
				_defaults[item.Id][langFolderName] = ParseKeywordString(text);
				_knownLanguages.Add(langFolderName);
			}
		}
	}

	private static void LoadMovementVerbs(string langFolderName)
	{
		string text = TryTranslateForLanguage("EA_MovementVerbs", langFolderName);
		if (text != null)
		{
			_movementVerbs[langFolderName] = new HashSet<string>(ParseKeywordString(text), StringComparer.OrdinalIgnoreCase);
			_knownLanguages.Add(langFolderName);
		}
	}

	private static void LoadTargetNounKeywords(string langFolderName)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		string[] targetNounActionIds = TargetNounActionIds;
		foreach (string text in targetNounActionIds)
		{
			string text2 = TryTranslateForLanguage("EA_TargetNoun_" + text, langFolderName);
			if (text2 == null)
			{
				continue;
			}
			foreach (string item in ParseKeywordString(text2))
			{
				dictionary[item] = text;
			}
		}
		if (dictionary.Count > 0)
		{
			_targetNounKeywords[langFolderName] = dictionary;
			_knownLanguages.Add(langFolderName);
		}
	}

	private static string TryTranslateForLanguage(string key, string langFolderName)
	{
		try
		{
			LoadedLanguage loadedLanguage = LanguageDatabase.AllLoadedLanguages.FirstOrDefault((LoadedLanguage l) => l.folderName == langFolderName);
			if (loadedLanguage == null)
			{
				return null;
			}
			if (loadedLanguage.TryGetTextFromKey(key, out var translated))
			{
				return translated;
			}
			return null;
		}
		catch (Exception ex)
		{
			EALogger.Debug("Failed to translate " + key + " for " + langFolderName + ": " + ex.Message);
			return null;
		}
	}

	public static List<string> ParseKeywordString(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return new List<string>();
		}
		return (from s in input.Split(new[] { ',' }, StringSplitOptions.None)
			select s.Trim() into s
			where !string.IsNullOrEmpty(s)
			select s).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
	}

	public static string FormatKeywordString(IEnumerable<string> keywords)
	{
		return string.Join(", ", keywords);
	}
}
