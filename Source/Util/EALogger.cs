using System;
using Ustas.RimAI.Actions.Mod;
using Ustas.RimAI.Core.Diagnostics;

namespace Ustas.RimAI.Actions.Util;

public static class EALogger
{
    public static void Info(string message) =>
        RimAiLog.Info(RimAiLogCategory.Actions, message);

    public static void Warn(string message) =>
        RimAiLog.Warning(RimAiLogCategory.Actions, message);

    public static void Error(string message) =>
        RimAiLog.Error(RimAiLogCategory.Actions, message);

    public static void Error(Exception ex) =>
        RimAiLog.Error(RimAiLogCategory.Actions, "Actions failure", ex);

    public static void Error(string message, Exception ex) =>
        RimAiLog.Error(RimAiLogCategory.Actions, message, ex);

    public static void Debug(string message)
    {
        EASettings settings = EAModMain.Settings;
        if (settings != null && settings.DebugMode)
            RimAiLog.Debug(RimAiLogCategory.Actions, message);
    }

    public static void DebugFormat(string format, params object[] args)
    {
        EASettings settings = EAModMain.Settings;
        if (settings != null && settings.DebugMode)
            RimAiLog.Debug(RimAiLogCategory.Actions, string.Format(format, args));
    }
}
