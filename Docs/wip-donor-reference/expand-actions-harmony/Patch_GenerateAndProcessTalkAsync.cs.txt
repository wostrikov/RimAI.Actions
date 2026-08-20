using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimTalk.ExpandActions.Util;
using Verse;

namespace RimTalk.ExpandActions.Patches;

/// <summary>
/// Historical RimTalk async postfix. Live capability execution is
/// Patch_CreateInteraction -> RimTalkCapabilityFrontend -> RimAIApplicationHost.
/// </summary>
public static class Patch_GenerateAndProcessTalkAsync
{
	[HarmonyTargetMethod]
	public static MethodBase TargetMethod()
	{
		Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((Assembly a) => a.GetName().Name == "RimTalk");
		if (assembly == null)
			return null;
		Type type = assembly.GetType("RimTalk.Service.TalkService");
		if (type == null)
			return null;
		return type.GetMethod("GenerateAndProcessTalkAsync", BindingFlags.Static | BindingFlags.NonPublic)
		       ?? type.GetMethod("GenerateAndProcessTalkAsync", BindingFlags.Static | BindingFlags.Public);
	}

	[HarmonyPostfix]
	public static void Postfix(object __0, object __result)
	{
		EALogger.Debug("GenerateAndProcessTalkAsync postfix ignored; live path is CreateInteraction");
	}
}
