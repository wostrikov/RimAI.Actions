using System;
using System.Linq;
using Ustas.RimAI.Actions.Mod;
using Ustas.RimAI.Actions.Util;
using Ustas.RimAI.Communication.API;
using Ustas.RimAI.Communication.Prompt;
using RimWorld;
using Verse;

namespace Ustas.RimAI.Actions.Integration;

public static class CommunicationIntegration
{
	private static bool _initialized;

	public static string BuildEaObservedSchema()
	{
		return "Описуючи дії у відповіді, можна додати поле 'ea_observed' зі списком спостережуваної поведінки.\nВАЖЛИВО: ea_observed призначене лише для ФІЗИЧНИХ ДІЙ (рух, слідування, напад, секс, робота тощо).\nНЕ додавай до ea_observed діалогові дії на кшталт 'Chat' або 'Flirt' — вони належать до поля 'act'.\n\nФормат:\n{\n  \"name\": \"...\",\n  \"text\": \"...\",\n  \"act\": \"...\",\n  \"target\": \"...\",\n  \"ea_observed\": [\n    \"ActorName keyword TargetName\",\n    \"Іван follow Олена\",\n    \"Іван attack Ворог\"\n  ]\n}\n\nФормат поведінки: \"ActorName keyword [TargetName]\"\nВикористовуй ключові слова дій зі списку нижче (НЕ діалогові дії на кшталт Chat/Flirt).\n\n{{ ea_keywords }}\n\nПравила:\n1. Використовуй точні імена персонажів як виконавців\n2. Використовуй action_id або ключові слова з підтримуваного списку вище\n3. Додавай лише ФІЗИЧНІ дії (рух, роботу, бій, секс/інтимність тощо)\n4. НЕ додавай думки, емоції, гіпотетичні дії або діалогові дії (Chat, Flirt тощо)\n\n{{ ea_act_effort }}";
	}

	public static void Initialize()
	{
		if (_initialized)
			return;
		try
		{
			RegisterPromptEntry();
			RegisterTemplateVariables();
			TalkLifecycleBridge.Register();
			_initialized = true;
			EALogger.Info("RimTalk integration initialized");
		}
		catch (Exception ex)
		{
			EALogger.Error("Failed to initialize RimTalk integration", ex);
		}
	}

	private static void RegisterPromptEntry()
	{
		RemoveLegacyEntryFromList("EA Action Schema");
		RemoveLegacyEntryFromList("Схема дій EA");
		var entry = new PromptEntry
		{
			SourceModId = "ustas.rimai.actions",
			Name = "Схема дій EA",
			Content = BuildEaObservedSchema(),
			Enabled = true,
			Role = PromptRole.User
		};
		bool placedBeforeJson = RimTalkPromptAPI.InsertPromptEntryBeforeName(entry, "JSON Format");
		EALogger.Info(placedBeforeJson
			? "Registered EA PromptEntry before JSON Format, ID: " + entry.Id
			: "Registered EA PromptEntry, ID: " + entry.Id);
	}

	private static void RemoveLegacyEntryFromList(string name)
	{
		var preset = PromptManager.Instance?.GetActivePreset();
		if (preset?.Entries == null)
			return;
		for (int num = preset.Entries.Count - 1; num >= 0; num--)
		{
			if (preset.Entries[num]?.Name == name)
			{
				preset.Entries.RemoveAt(num);
				EALogger.Info($"Removed legacy EA entry '{name}' from position {num}");
			}
		}
	}

	private static void RegisterTemplateVariables()
	{
		RimTalkPromptAPI.RegisterContextVariable("ustas.rimai.actions", "ea_keywords", ctx => WrapPawnAware(ctx, () => EAVariableProvider.GetKeywords()), "Підтримувані ключові слова EA", 50);
		RimTalkPromptAPI.RegisterContextVariable("ustas.rimai.actions", "ea_json_format", ctx => WrapPawnAware(ctx, EAVariableProvider.GetJsonFormat), "Формат JSON для EA", 50);
		RimTalkPromptAPI.RegisterContextVariable("ustas.rimai.actions", "ea_actions", ctx => WrapPawnAware(ctx, EAVariableProvider.GetActions), "Увімкнені дії EA", 50);
		RimTalkPromptAPI.RegisterContextVariable("ustas.rimai.actions", "ea_summary", ctx => WrapPawnAware(ctx, EAVariableProvider.GetSummary), "Повний опис EA", 50);
		RimTalkPromptAPI.RegisterContextVariable("ustas.rimai.actions", "ea_act_effort", ctx => WrapPawnAware(ctx, EAVariableProvider.GetActEffort), "Вказівки щодо рівня зусиль для дій EA", 50);
		EALogger.Info("EA template variables registered: {{ ea_keywords }}, {{ ea_json_format }}, {{ ea_actions }}, {{ ea_summary }}, {{ ea_act_effort }}");
	}

	private static string WrapPawnAware(PromptContext ctx, Func<string> valueFunc)
	{
		Pawn currentPawn = ctx?.CurrentPawn;
		if (currentPawn == null)
			return valueFunc();
		EASettings settings = EAModMain.Settings;
		if (settings == null || !settings.Enabled)
			return "";
		bool drafted = currentPawn.drafter?.Drafted ?? false;
		if (settings.SkipDraftedPawns && drafted)
			return "";
		TimeAssignmentDef timeAssignmentDef = currentPawn.timetable?.CurrentAssignment;
		if (settings.SkipWorkTimePawns && timeAssignmentDef == TimeAssignmentDefOf.Work)
			return "";
		return valueFunc();
	}
}
