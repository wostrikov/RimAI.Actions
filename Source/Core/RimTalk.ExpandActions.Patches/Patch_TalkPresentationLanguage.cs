using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using RimAI.Core.Application;
using RimAI.RimWorld.Application;
using RimTalk.ExpandActions.LLM;
using RimTalk.ExpandActions.Mod;
using RimTalk.ExpandActions.Util;

namespace RimTalk.ExpandActions.Patches;

[HarmonyPatch]
public static class Patch_TalkPresentationLanguage
{
	private static readonly PresentationLanguageGuard Guard = new();
	private static readonly HashSet<int> InFlight = new();

	[HarmonyTargetMethod]
	public static MethodBase TargetMethod()
	{
		var assembly = AppDomain.CurrentDomain.GetAssemblies()
			.FirstOrDefault(item => item.GetName().Name == "RimTalk");
		var type = assembly?.GetType("RimTalk.Data.PawnState");
		return type?.GetMethod(
			"QueueIncomingResponse",
			BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
	}

	[HarmonyPostfix]
	public static void Postfix(object __0)
	{
		if (__0 is null)
			return;
		var textProperty = __0.GetType().GetProperty("Text");
		if (textProperty is null)
			return;
		var original = textProperty.GetValue(__0) as string;
		var language = LanguageRuntime.Current;
		var first = Guard.Validator.Validate(original, language);
		if (first.Verdict != OutputLanguageVerdict.ClearlyWrongLanguage)
			return;

		var key = RuntimeHelpers.GetHashCode(__0);
		lock (InFlight)
		{
			if (!InFlight.Add(key))
				return;
		}

		_ = Task.Run(async () =>
		{
			try
			{
				var result = await Guard.EnsureHumanTextAsync(
					original,
					language,
					new RimTalkHumanTextRewriter()).ConfigureAwait(false);
				textProperty.SetValue(__0, result.Text);
				if (result.CorrectiveRetryUsed)
					EALogger.Info(
						"[RIMAI_LANGUAGE] output_check=retry verdict=" + result.Validation.Verdict +
						" output=" + language.OutputLanguage.Code);
			}
			catch (Exception ex)
			{
				EALogger.Debug("Language correction skipped: " + ex.Message);
			}
			finally
			{
				lock (InFlight)
					InFlight.Remove(key);
			}
		});
	}
}
