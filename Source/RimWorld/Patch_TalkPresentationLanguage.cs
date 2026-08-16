using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using RimAI.Core.Application;
using RimAI.RimWorld.Application;
using Ustas.RimAI.Actions.Execution;
using Ustas.RimAI.Actions.LLM;
using Ustas.RimAI.Actions.Util;

namespace Ustas.RimAI.Actions.Patches;

[HarmonyPatch]
public static class Patch_TalkPresentationLanguage
{
	private static readonly PresentationLanguageGuard Guard = new();
	private static readonly HashSet<int> Allowed = new();
	private static readonly HashSet<int> InFlight = new();
	private static MethodInfo? _queueMethod;

	[HarmonyTargetMethod]
	public static MethodBase TargetMethod()
	{
		var assembly = AppDomain.CurrentDomain.GetAssemblies()
			.FirstOrDefault(item => item.GetName().Name == "Ustas.RimAI.Communication");
		var type = assembly?.GetType("Ustas.RimAI.Communication.Data.PawnState");
		_queueMethod = type?.GetMethod(
			"QueueIncomingResponse",
			BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
		return _queueMethod;
	}

	[HarmonyPrefix]
	public static bool Prefix(object __instance, object __0)
	{
		if (__instance is null || __0 is null)
			return true;
		var textProperty = __0.GetType().GetProperty("Text");
		if (textProperty is null)
			return true;

		var key = RuntimeHelpers.GetHashCode(__0);
		lock (Allowed)
		{
			if (Allowed.Contains(key))
				return true;
		}

		var original = textProperty.GetValue(__0) as string;
		var language = LanguageRuntime.Current;
		var first = Guard.Validator.Validate(original, language);
		if (first.Verdict != OutputLanguageVerdict.ClearlyWrongLanguage)
			return true;

		lock (InFlight)
		{
			if (!InFlight.Add(key))
				return false;
		}

		_ = Task.Run(async () =>
		{
			try
			{
				var result = await Guard.EnsureHumanTextAsync(
					original,
					language,
					new ActionsHumanTextRewriter()).ConfigureAwait(false);
				textProperty.SetValue(__0, result.Text);
				lock (Allowed)
					Allowed.Add(key);
				MainThreadDispatcher.Enqueue(() => _queueMethod?.Invoke(__instance, new[] { __0 }));
				EALogger.Info(
					"[RIMAI_LANGUAGE] output_check=held_until_corrected first=" + first.Verdict +
					" after=" + result.Validation.Verdict +
					" retry=" + result.CorrectiveRetryUsed +
					" output=" + language.OutputLanguage.Code);
			}
			catch (Exception ex)
			{
				EALogger.Debug("Language correction skipped: " + ex.Message);
				lock (Allowed)
					Allowed.Add(key);
				MainThreadDispatcher.Enqueue(() => _queueMethod?.Invoke(__instance, new[] { __0 }));
			}
			finally
			{
				lock (InFlight)
					InFlight.Remove(key);
			}
		});
		return false;
	}
}
