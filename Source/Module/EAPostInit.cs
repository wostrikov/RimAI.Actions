using System.Collections.Generic;
using System.Linq;
using RimAI.Core.Application;
using RimAI.RimWorld.Application;
using Ustas.RimAI.Actions.Core;
using Ustas.RimAI.Actions.Util;
using Verse;

namespace Ustas.RimAI.Actions.Mod;

[StaticConstructorOnStartup]
public static class EAPostInit
{
	static EAPostInit()
	{
		ModuleRegistry.DiscoverAndRegisterModules();
		KeywordConfigManager.Reload();
		InjectFallbackTranslations("EA_");
		int executable = CapabilityPromptContract.ListFrontendCapabilities(RimAIApplicationHost.Catalog).Count;
		EALogger.Info($"[RIMAI_FRONTEND] catalog=canonical executable={executable} path=direct");
		_ = LanguageRuntime.Current;
		EALogger.Info($"EA post-init complete. Total actions: {ActionRegistry.GetAll().Count()}");
	}

	private static void InjectFallbackTranslations(string prefix)
	{
		LoadedLanguage defaultLanguage = LanguageDatabase.defaultLanguage;
		if (defaultLanguage == null)
		{
			return;
		}
		defaultLanguage.TryGetTextFromKey(prefix + "ModName", out var translated);
		if (defaultLanguage.keyedReplacements == null || defaultLanguage.keyedReplacements.Count == 0)
		{
			return;
		}
		List<KeyValuePair<string, LoadedLanguage.KeyedReplacement>> list = new List<KeyValuePair<string, LoadedLanguage.KeyedReplacement>>();
		foreach (KeyValuePair<string, LoadedLanguage.KeyedReplacement> keyedReplacement in defaultLanguage.keyedReplacements)
		{
			if (keyedReplacement.Key.StartsWith(prefix))
			{
				list.Add(keyedReplacement);
			}
		}
		if (list.Count == 0)
		{
			return;
		}
		int num = 0;
		foreach (LoadedLanguage allLoadedLanguage in LanguageDatabase.AllLoadedLanguages)
		{
			if (allLoadedLanguage == defaultLanguage || allLoadedLanguage.keyedReplacements == null)
			{
				continue;
			}
			allLoadedLanguage.TryGetTextFromKey(prefix + "ModName", out translated);
			foreach (KeyValuePair<string, LoadedLanguage.KeyedReplacement> item in list)
			{
				if (!allLoadedLanguage.keyedReplacements.ContainsKey(item.Key))
				{
					allLoadedLanguage.keyedReplacements[item.Key] = item.Value;
					num++;
				}
			}
		}
		if (num > 0)
		{
			EALogger.Info($"Injected {num} English fallback translations for {prefix}* keys");
		}
	}
}
