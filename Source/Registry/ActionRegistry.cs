using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ustas.RimAI.Actions.Integration;
using Ustas.RimAI.Actions.Mod;
using Ustas.RimAI.Actions.Util;

namespace Ustas.RimAI.Actions.Core;

public static class ActionRegistry
{
	private static readonly Dictionary<string, ActionDefinition> _actions = new Dictionary<string, ActionDefinition>();

	private static bool _initialized = false;

	public static void Initialize()
	{
		if (!_initialized)
		{
			RegisterAllActions();
			_initialized = true;
			EALogger.Info($"ActionRegistry initialized with {_actions.Count} built-in actions");
		}
	}

	public static void Register(ActionDefinition action)
	{
		if (_actions.ContainsKey(action.Id))
		{
			EALogger.Warn("Action '" + action.Id + "' is already registered, overwriting");
		}
		_actions[action.Id] = action;
	}

	public static ActionDefinition GetById(string id)
	{
		if (!_actions.TryGetValue(id, out var value))
		{
			return null;
		}
		return value;
	}

	public static IEnumerable<ActionDefinition> GetAll()
	{
		return _actions.Values;
	}

	public static IEnumerable<ActionDefinition> GetByCategory(ActionCategory category)
	{
		return _actions.Values.Where((ActionDefinition a) => a.Category == category);
	}

	public static IEnumerable<ActionDefinition> GetEnabledActions()
	{
		EASettings settings = EAModMain.Settings;
		return _actions.Values.Where((ActionDefinition a) => settings.IsActionEnabled(a.Id));
	}

	public static string GetEnabledActionsPrompt()
	{
		EASettings settings = EAModMain.Settings;
		StringBuilder stringBuilder = new StringBuilder();
		foreach (ActionDefinition enabledAction in GetEnabledActions())
		{
			string promptDesc = settings.GetPromptDesc(enabledAction.Id, enabledAction.DefaultPromptDesc);
			string text = string.Join(", ", enabledAction.RequiredParams);
			if (enabledAction.OptionalParams.Count > 0)
			{
				text = text + ((text.Length > 0) ? ", " : "") + "[" + string.Join(", ", enabledAction.OptionalParams) + "]";
			}
			stringBuilder.AppendLine("- " + enabledAction.Id + "(" + text + "): " + promptDesc);
		}
		return stringBuilder.ToString();
	}

	public static bool IsActionValid(string id)
	{
		if (_actions.ContainsKey(id))
		{
			return EAModMain.Settings.IsActionEnabled(id);
		}
		return false;
	}

	private static void RegisterAllActions()
	{
		ActionRegistryCatalogMovement.RegisterAll();
		ActionRegistryCatalogCombat.RegisterAll();
		ActionRegistryCatalogMedical.RegisterAll();
		ActionRegistryCatalogSocial.RegisterAll();
		ActionRegistryCatalogItem.RegisterAll();
		ActionRegistryCatalogWork.RegisterAll();
		ActionRegistryCatalogPrisoner.RegisterAll();
		ActionRegistryCatalogAnimal.RegisterAll();
		ActionRegistryCatalogRecreation.RegisterAll();
		ActionRegistryCatalogFacility.RegisterAll();
		ActionRegistryCatalogFuneral.RegisterAll();
	}
}
