using System;
using Ustas.RimAI.Actions.Mod;
using Verse;

namespace Ustas.RimAI.Actions.Util;

public static class EALogger
{
	private const string Prefix = "[EA] ";

	public static void Info(string message)
	{
		Log.Message("[EA] " + message);
	}

	public static void Warn(string message)
	{
		Log.Warning("[EA] " + message);
	}

	public static void Error(string message)
	{
		Log.Error("[EA] " + message);
	}

	public static void Error(Exception ex)
	{
		Log.Error("[EA] " + ex.ToString());
	}

	public static void Error(string message, Exception ex)
	{
		Log.Error("[EA] " + message + "\n" + ex.ToString());
	}

	public static void Debug(string message)
	{
		EASettings settings = EAModMain.Settings;
		if (settings != null && settings.DebugMode)
		{
			Log.Message("[EA] [DEBUG] " + message);
		}
	}

	public static void DebugFormat(string format, params object[] args)
	{
		EASettings settings = EAModMain.Settings;
		if (settings != null && settings.DebugMode)
		{
			Log.Message("[EA] [DEBUG] " + string.Format(format, args));
		}
	}
}
