using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using HarmonyLib;
using RimAI.Core.Application;
using RimAI.RimWorld.Application;

namespace Ustas.RimAI.Actions.Patches;

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
			.FirstOrDefault(item => item.GetName().Name == "Ustas.RimAI.Communication");
		var type = assembly?.GetType("Ustas.RimAI.Communication.Service.PromptService");
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
		var language = LanguageRuntime.Current;
		var instruction = LanguagePromptContract.BuildPawnDialogueInstruction(language);
		prompt = CompetingReminder.Replace(prompt, string.Empty);
		if (prompt.IndexOf("All spoken pawn dialogue must be written in", StringComparison.Ordinal) < 0)
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
			.FirstOrDefault(item => item.GetName().Name == "Ustas.RimAI.Communication");
		var type = assembly?.GetType("Ustas.RimAI.Communication.Data.Constant");
		return type?.GetProperty("Lang", BindingFlags.Public | BindingFlags.Static)?.GetGetMethod();
	}

	[HarmonyPostfix]
	public static void Postfix(ref string __result)
	{
		var language = LanguageRuntime.Current;
		__result = LanguagePromptContract.NativePromptName(language.OutputLanguage.Code);
	}
}
