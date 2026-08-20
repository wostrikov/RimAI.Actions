using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using HarmonyLib;
using RimAI.Core.Application;
using RimAI.RimWorld.Application;

namespace RimTalk.ExpandActions.Patches;

[HarmonyPatch]
public static class Patch_DecoratePromptLanguage
{
	private static readonly Regex CompetingReminder = new(
		@"\nRespond only in [^\n]*",
		RegexOptions.CultureInvariant);

	[HarmonyTargetMethod]
	public static MethodBase TargetMethod()
	{
		var assembly = AppDomain.CurrentDomain.GetAssemblies()
			.FirstOrDefault(item => item.GetName().Name == "RimTalk");
		var type = assembly?.GetType("RimTalk.Service.PromptService");
		return type?.GetMethod(
			"DecoratePrompt",
			BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
	}

	[HarmonyPostfix]
	public static void Postfix(object __0)
	{
		if (__0 is null)
			return;
		var promptProperty = __0.GetType().GetProperty("Prompt");
		if (promptProperty is null)
			return;
		var prompt = promptProperty.GetValue(__0) as string ?? string.Empty;
		var language = LanguageRuntime.Resolve(new LanguageSignals(Frontend: FrontendKind.RimTalk));
		var instruction = LanguagePromptContract.BuildHumanOutputInstruction(language);
		prompt = CompetingReminder.Replace(prompt, string.Empty);
		if (prompt.IndexOf("Keep capability IDs", StringComparison.Ordinal) < 0)
			prompt = prompt.TrimEnd() + "\n" + instruction;
		promptProperty.SetValue(__0, prompt);
	}
}

[HarmonyPatch]
public static class Patch_ConstantLangDisplay
{
	[HarmonyTargetMethod]
	public static MethodBase TargetMethod()
	{
		var assembly = AppDomain.CurrentDomain.GetAssemblies()
			.FirstOrDefault(item => item.GetName().Name == "RimTalk");
		var type = assembly?.GetType("RimTalk.Data.Constant");
		return type?.GetProperty("Lang", BindingFlags.Public | BindingFlags.Static)?.GetGetMethod();
	}

	[HarmonyPostfix]
	public static void Postfix(ref string __result)
	{
		var language = LanguageRuntime.Current;
		__result = LanguagePromptContract.DisplayName(language.OutputLanguage.Code);
	}
}
