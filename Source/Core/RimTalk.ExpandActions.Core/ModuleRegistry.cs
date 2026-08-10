using System;
using System.Collections.Generic;
using System.Reflection;
using RimTalk.ExpandActions.Util;

namespace RimTalk.ExpandActions.Core;

public static class ModuleRegistry
{
	private static readonly Dictionary<string, IEAExtensionModule> _modules = new Dictionary<string, IEAExtensionModule>();

	private static readonly List<string> _moduleJobWhitelist = new List<string>();

	private static readonly List<IEAVariableContributor> _variableContributors = new List<IEAVariableContributor>();

	private static bool _discovered = false;

	public static void DiscoverAndRegisterModules()
	{
		if (_discovered)
		{
			return;
		}
		_discovered = true;
		Type typeFromHandle = typeof(IEAExtensionModule);
		int num = 0;
		try
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				Type[] types;
				try
				{
					types = assembly.GetTypes();
				}
				catch (Exception)
				{
					continue;
				}
				Type[] array = types;
				foreach (Type type in array)
				{
					if (type.IsInterface || type.IsAbstract || !typeFromHandle.IsAssignableFrom(type))
					{
						continue;
					}
					try
					{
						IEAExtensionModule iEAExtensionModule = (IEAExtensionModule)Activator.CreateInstance(type);
						if (iEAExtensionModule.IsAvailable())
						{
							RegisterModule(iEAExtensionModule);
							num++;
						}
					}
					catch (Exception ex2)
					{
						EALogger.Error("Failed to instantiate extension module " + type.FullName, ex2);
					}
				}
			}
		}
		catch (Exception ex3)
		{
			EALogger.Error("Failed during module discovery", ex3);
		}
		if (num > 0)
		{
			EALogger.Info($"ModuleRegistry: discovered and registered {num} extension module(s)");
		}
	}

	private static void RegisterModule(IEAExtensionModule module)
	{
		_modules[module.ModuleId] = module;
		int num = 0;
		foreach (ActionDefinition action in module.GetActions())
		{
			action.SourceModule = module.ModuleId;
			ActionRegistry.Register(action);
			num++;
		}
		foreach (string jobWhitelistEntry in module.GetJobWhitelistEntries())
		{
			if (!_moduleJobWhitelist.Contains(jobWhitelistEntry))
			{
				_moduleJobWhitelist.Add(jobWhitelistEntry);
			}
		}
		foreach (IEAVariableContributor variableContributor in module.GetVariableContributors())
		{
			_variableContributors.Add(variableContributor);
		}
		EALogger.Info($"Registered module '{module.ModuleId}' with {num} actions");
	}

	public static IEnumerable<IEAExtensionModule> GetRegisteredModules()
	{
		return _modules.Values;
	}

	public static IEAExtensionModule GetModule(string moduleId)
	{
		if (!_modules.TryGetValue(moduleId, out var value))
		{
			return null;
		}
		return value;
	}

	public static bool IsJobInModuleWhitelist(string jobDefName)
	{
		return _moduleJobWhitelist.Contains(jobDefName);
	}

	public static IEnumerable<string> GetModuleJobWhitelist()
	{
		return _moduleJobWhitelist;
	}

	public static IEnumerable<IEAVariableContributor> GetVariableContributors()
	{
		return _variableContributors;
	}
}
