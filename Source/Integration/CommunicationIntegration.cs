using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Ustas.RimAI.Actions.Mod;
using Ustas.RimAI.Actions.Util;
using RimWorld;
using Verse;

namespace Ustas.RimAI.Actions.Integration;

public static class CommunicationIntegration
{
	private static bool _initialized;

	private static Type _rimTalkPromptAPIType;

	private static MethodInfo _addPromptEntryMethod;

	private static MethodInfo _insertBeforeNameMethod;

	private static MethodInfo _registerContextVariableMethod;

	public static string BuildEaObservedSchema()
	{
		return "Описуючи дії у відповіді, можна додати поле 'ea_observed' зі списком спостережуваної поведінки.\nВАЖЛИВО: ea_observed призначене лише для ФІЗИЧНИХ ДІЙ (рух, слідування, напад, секс, робота тощо).\nНЕ додавай до ea_observed діалогові дії на кшталт 'Chat' або 'Flirt' — вони належать до поля 'act'.\n\nФормат:\n{\n  \"name\": \"...\",\n  \"text\": \"...\",\n  \"act\": \"...\",\n  \"target\": \"...\",\n  \"ea_observed\": [\n    \"ActorName keyword TargetName\",\n    \"Іван follow Олена\",\n    \"Іван attack Ворог\"\n  ]\n}\n\nФормат поведінки: \"ActorName keyword [TargetName]\"\nВикористовуй ключові слова дій зі списку нижче (НЕ діалогові дії на кшталт Chat/Flirt).\n\n{{ ea_keywords }}\n\nПравила:\n1. Використовуй точні імена персонажів як виконавців\n2. Використовуй action_id або ключові слова з підтримуваного списку вище\n3. Додавай лише ФІЗИЧНІ дії (рух, роботу, бій, секс/інтимність тощо)\n4. НЕ додавай думки, емоції, гіпотетичні дії або діалогові дії (Chat, Flirt тощо)\n\n{{ ea_act_effort }}";
	}

	public static void Initialize()
	{
		if (_initialized)
		{
			return;
		}
		try
		{
			_rimTalkPromptAPIType = AccessTools.TypeByName("Ustas.RimAI.Communication.API.RimTalkPromptAPI");
			if (_rimTalkPromptAPIType == null)
			{
				EALogger.Warn("RimTalkPromptAPI not found - prompt injection disabled");
				return;
			}
			_addPromptEntryMethod = AccessTools.Method(_rimTalkPromptAPIType, "AddPromptEntry");
			if (_addPromptEntryMethod == null)
			{
				EALogger.Warn("AddPromptEntry method not found");
				return;
			}
			_insertBeforeNameMethod = AccessTools.Method(_rimTalkPromptAPIType, "InsertPromptEntryBeforeName");
			_registerContextVariableMethod = AccessTools.Method(_rimTalkPromptAPIType, "RegisterContextVariable");
			RegisterPromptEntry();
			RegisterTemplateVariables();
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
		try
		{
			Type type = AccessTools.TypeByName("Ustas.RimAI.Communication.Prompt.PromptEntry");
			if (type == null)
			{
				EALogger.Warn("PromptEntry type not found");
				return;
			}
			Type type2 = AccessTools.TypeByName("Ustas.RimAI.Communication.Prompt.PromptRole");
			object obj = ((type2 != null) ? Enum.Parse(type2, "User") : null);
			RemoveLegacyEntryFromList("EA Action Schema");
			RemoveLegacyEntryFromList("Схема дій EA");
			object obj2 = Activator.CreateInstance(type);
			SetProperty(obj2, "SourceModId", "ustas.rimai.actions");
			SetProperty(obj2, "Name", "Схема дій EA");
			SetField(obj2, "Content", BuildEaObservedSchema());
			SetField(obj2, "Enabled", true);
			if (obj != null)
			{
				SetField(obj2, "Role", obj);
			}
			if (_insertBeforeNameMethod != null)
			{
				object arg = _insertBeforeNameMethod.Invoke(null, new object[2] { obj2, "JSON Format" });
				string arg2 = (GetPropertyValue(obj2, "Id") as string) ?? "(unknown)";
				EALogger.Info($"Registered EA PromptEntry before JSON Format (found: {arg}), ID: {arg2}");
			}
			else if (_addPromptEntryMethod?.Invoke(null, new object[1] { obj2 }) is bool flag)
			{
				string text = (GetPropertyValue(obj2, "Id") as string) ?? "(unknown)";
				if (flag)
				{
					EALogger.Info("Registered EA PromptEntry (appended), ID: " + text);
				}
				else
				{
					EALogger.Info("EA PromptEntry was deleted by user (blacklisted), skipping injection");
				}
			}
		}
		catch (Exception ex)
		{
			EALogger.Error("Failed to register prompt entry", ex);
		}
	}

	private static void RemoveLegacyEntryFromList(string name)
	{
		try
		{
			Type type = AccessTools.TypeByName("Ustas.RimAI.Communication.Prompt.PromptManager");
			if (type == null)
			{
				return;
			}
			object obj = type.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public)?.GetValue(null);
			if (obj == null)
			{
				return;
			}
			object obj2 = type.GetMethod("GetActivePreset")?.Invoke(obj, null);
			if (obj2 == null || !(obj2.GetType().GetField("Entries")?.GetValue(obj2) is IList list))
			{
				return;
			}
			for (int num = list.Count - 1; num >= 0; num--)
			{
				if (GetPropertyValue(list[num], "Name") as string == name)
				{
					list.RemoveAt(num);
					EALogger.Info($"Removed legacy EA entry '{name}' from position {num}");
				}
			}
		}
		catch (Exception ex)
		{
			EALogger.Debug("RemoveLegacyEntryFromList failed: " + ex.Message);
		}
	}

	private static void SetField(object obj, string fieldName, object value)
	{
		obj.GetType().GetField(fieldName)?.SetValue(obj, value);
	}

	private static void SetProperty(object obj, string propertyName, object value)
	{
		PropertyInfo property = obj.GetType().GetProperty(propertyName);
		if (property != null && property.CanWrite)
		{
			property.SetValue(obj, value);
		}
		else
		{
			obj.GetType().GetField(propertyName)?.SetValue(obj, value);
		}
	}

	private static object GetPropertyValue(object obj, string propertyName)
	{
		return obj.GetType().GetProperty(propertyName)?.GetValue(obj);
	}

	private static void RegisterTemplateVariables()
	{
		try
		{
			if (_registerContextVariableMethod != null)
			{
				TryRegisterContextVariable("ustas.rimai.actions", "ea_keywords", () => EAVariableProvider.GetKeywords(), "Підтримувані ключові слова EA", checkPawnStatus: true);
				TryRegisterContextVariable("ustas.rimai.actions", "ea_json_format", EAVariableProvider.GetJsonFormat, "Формат JSON для EA", checkPawnStatus: true);
				TryRegisterContextVariable("ustas.rimai.actions", "ea_actions", EAVariableProvider.GetActions, "Увімкнені дії EA", checkPawnStatus: true);
				TryRegisterContextVariable("ustas.rimai.actions", "ea_summary", EAVariableProvider.GetSummary, "Повний опис EA", checkPawnStatus: true);
				TryRegisterContextVariable("ustas.rimai.actions", "ea_act_effort", EAVariableProvider.GetActEffort, "Вказівки щодо рівня зусиль для дій EA", checkPawnStatus: true);
				EALogger.Info("EA template variables registered: {{ ea_keywords }}, {{ ea_json_format }}, {{ ea_actions }}, {{ ea_summary }}, {{ ea_act_effort }}");
				EALogger.Info("EA variables are pawn-aware: will return empty if pawn is drafted or on Work schedule");
			}
			else
			{
				EALogger.Warn("RegisterContextVariable method not found - template variables not available");
			}
		}
		catch (Exception ex)
		{
			EALogger.Error("Failed to register template variables", ex);
		}
	}

	private static void TryRegisterContextVariable(string modId, string name, Func<string> valueFunc, string description, bool checkPawnStatus = false)
	{
		try
		{
			ParameterInfo[] parameters = _registerContextVariableMethod.GetParameters();
			if (parameters.Length < 3)
			{
				EALogger.Debug($"RegisterContextVariable has unexpected parameter count: {parameters.Length}");
				return;
			}
			Type parameterType = parameters[2].ParameterType;
			if (!parameterType.IsGenericType)
			{
				EALogger.Debug($"Parameter 2 is not generic: {parameterType}");
				return;
			}
			Type[] genericArguments = parameterType.GetGenericArguments();
			if (genericArguments.Length != 2)
			{
				EALogger.Debug($"Expected 2 generic args, got: {genericArguments.Length}");
				return;
			}
			Type type = genericArguments[0];
			object obj = typeof(CommunicationIntegration).GetMethod("CreateContextFuncWrapper", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(type).Invoke(null, new object[2] { valueFunc, checkPawnStatus });
			_registerContextVariableMethod.Invoke(null, new object[5] { modId, name, obj, description, 50 });
			EALogger.Debug("Registered { " + name + " }" + (checkPawnStatus ? " (pawn-aware)" : ""));
		}
		catch (Exception ex)
		{
			EALogger.Warn("Failed to register " + name + ": " + ex.Message);
		}
	}

	private static Func<T, string> CreateContextFuncWrapper<T>(Func<string> valueFunc, bool checkPawnStatus = false)
	{
		return delegate(T ctx)
		{
			if (!checkPawnStatus)
			{
				return valueFunc();
			}
			EALogger.Debug($"[EA] Template variable requested, checkPawnStatus={checkPawnStatus}");
			Pawn currentPawnFromContext = GetCurrentPawnFromContext(ctx);
			if (currentPawnFromContext == null)
			{
				EALogger.Debug("[EA] No pawn found in PromptContext - returning normal value");
				return valueFunc();
			}
			EALogger.Debug("[EA] Found pawn in context: " + currentPawnFromContext.LabelShort);
			EASettings settings = EAModMain.Settings;
			if (settings == null || !settings.Enabled)
			{
				EALogger.Debug("[EA] Settings null or EA disabled");
				return "";
			}
			bool flag = currentPawnFromContext.drafter?.Drafted ?? false;
			EALogger.Debug($"[EA] Pawn {currentPawnFromContext.LabelShort} drafted={flag}, SkipDraftedPawns={settings.SkipDraftedPawns}");
			if (settings.SkipDraftedPawns && flag)
			{
				EALogger.Debug("[EA] Pawn " + currentPawnFromContext.LabelShort + " is drafted - EA disabled for this dialogue");
				return "";
			}
			TimeAssignmentDef timeAssignmentDef = currentPawnFromContext.timetable?.CurrentAssignment;
			bool flag2 = timeAssignmentDef == TimeAssignmentDefOf.Work;
			EALogger.Debug(string.Format("[EA] Pawn {0} schedule={1}, isWork={2}, SkipWorkTimePawns={3}", currentPawnFromContext.LabelShort, timeAssignmentDef?.defName ?? "null", flag2, settings.SkipWorkTimePawns));
			if (settings.SkipWorkTimePawns && flag2)
			{
				EALogger.Debug("[EA] Pawn " + currentPawnFromContext.LabelShort + " is on Work schedule - EA disabled for this dialogue");
				return "";
			}
			EALogger.Debug("[EA] Pawn " + currentPawnFromContext.LabelShort + " can execute EA - returning normal value");
			return valueFunc();
		};
	}

	private static Pawn GetCurrentPawnFromContext(object context)
	{
		if (context == null)
		{
			EALogger.Debug("[EA] GetCurrentPawnFromContext: context is null");
			return null;
		}
		try
		{
			Type type = context.GetType();
			EALogger.Debug("[EA] PromptContext type: " + type.FullName);
			PropertyInfo[] properties = type.GetProperties();
			EALogger.Debug(string.Format("[EA] PromptContext has {0} properties: {1}", properties.Length, string.Join(", ", properties.Select((PropertyInfo p) => p.Name).Take(10))));
			PropertyInfo property = type.GetProperty("CurrentPawn");
			if (property != null)
			{
				Pawn pawn = property.GetValue(context) as Pawn;
				EALogger.Debug("[EA] Found CurrentPawn property, value: " + (pawn?.LabelShort ?? "null"));
				return pawn;
			}
			PropertyInfo property2 = type.GetProperty("Pawn");
			if (property2 != null)
			{
				Pawn pawn2 = property2.GetValue(context) as Pawn;
				EALogger.Debug("[EA] Found Pawn property, value: " + (pawn2?.LabelShort ?? "null"));
				return pawn2;
			}
			EALogger.Debug("[EA] No CurrentPawn or Pawn property found in PromptContext");
		}
		catch (Exception ex)
		{
			EALogger.Debug("[EA] Failed to get pawn from context: " + ex.Message);
		}
		return null;
	}
}
