using System;
using System.Linq;
using System.Reflection;
using Ustas.RimAI.Actions.Util;
using Verse;

namespace Ustas.RimAI.Actions.RJW.Util;

public static class RJWReflectionCache
{
	private static Type _sexUtilityType;

	private static Type _xxxType;

	private static Type _needSexType;

	private static MethodInfo _canFuck;

	private static MethodInfo _canBeFucked;

	private static MethodInfo _canMasturbate;

	private static FieldInfo _joinInBedDef;

	private static FieldInfo _quickieDef;

	private static FieldInfo _masturbateDef;

	private static FieldInfo _giveBondageGearDef;

	private static FieldInfo _unlockBondageGearDef;

	private static MethodInfo _isFrustrated;

	private static MethodInfo _getSexDrive;

	private static bool _initialized;

	public static Type NeedSexType
	{
		get
		{
			EnsureInitialized();
			return _needSexType;
		}
	}

	public static void EnsureInitialized()
	{
		if (_initialized)
		{
			return;
		}
		_initialized = true;
		try
		{
			_sexUtilityType = FindType("rjw.SexUtility");
			_xxxType = FindType("rjw.xxx");
			_needSexType = FindType("rjw.Need_Sex");
			EALogger.Debug($"[RJW] Types resolved: SexUtility={_sexUtilityType != null}, xxx={_xxxType != null}, Need_Sex={_needSexType != null}");
			if (_sexUtilityType != null)
			{
				_canFuck = _sexUtilityType.GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault((MethodInfo m) => m.Name == "can_fuck" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(Pawn));
				if (_canFuck == null)
				{
					_canFuck = _sexUtilityType.GetMethod("can_fuck", BindingFlags.Static | BindingFlags.Public);
					if (_canFuck != null)
					{
						EALogger.Debug(string.Format("[RJW] can_fuck found with {0} params: {1}", _canFuck.GetParameters().Length, string.Join(", ", from p in _canFuck.GetParameters()
							select p.ParameterType.Name)));
					}
					else
					{
						EALogger.Warn("[RJW] can_fuck method not found in SexUtility");
					}
				}
				_canBeFucked = _sexUtilityType.GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault((MethodInfo m) => m.Name == "can_be_fucked" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(Pawn));
				if (_canBeFucked == null)
				{
					_canBeFucked = _sexUtilityType.GetMethod("can_be_fucked", BindingFlags.Static | BindingFlags.Public);
					if (_canBeFucked == null)
					{
						EALogger.Warn("[RJW] can_be_fucked method not found in SexUtility");
					}
				}
				_canMasturbate = _sexUtilityType.GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault((MethodInfo m) => m.Name == "can_masturbate" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(Pawn));
				if (_canMasturbate == null)
				{
					_canMasturbate = _sexUtilityType.GetMethod("can_masturbate", BindingFlags.Static | BindingFlags.Public);
					if (_canMasturbate == null)
					{
						EALogger.Warn("[RJW] can_masturbate method not found in SexUtility");
					}
				}
				EALogger.Debug($"[RJW] SexUtility resolved: can_fuck={_canFuck != null}, can_be_fucked={_canBeFucked != null}, can_masturbate={_canMasturbate != null}");
			}
			else
			{
				EALogger.Warn("[RJW] SexUtility type not found");
			}
			if (_xxxType != null)
			{
				_joinInBedDef = _xxxType.GetField("JoinInBed", BindingFlags.Static | BindingFlags.Public);
				_quickieDef = _xxxType.GetField("Quickie", BindingFlags.Static | BindingFlags.Public);
				_masturbateDef = _xxxType.GetField("RJW_Masturbate", BindingFlags.Static | BindingFlags.Public);
				_giveBondageGearDef = _xxxType.GetField("GiveBondageGear", BindingFlags.Static | BindingFlags.Public);
				_unlockBondageGearDef = _xxxType.GetField("UnlockBondageGear", BindingFlags.Static | BindingFlags.Public);
				_isFrustrated = _xxxType.GetMethod("is_frustrated", BindingFlags.Static | BindingFlags.Public);
				_getSexDrive = _xxxType.GetMethod("get_sex_drive", BindingFlags.Static | BindingFlags.Public);
				EALogger.Debug($"[RJW] xxx resolved: JoinInBed={_joinInBedDef != null}, Quickie={_quickieDef != null}, Masturbate={_masturbateDef != null}, BondageGive={_giveBondageGearDef != null}, BondageUnlock={_unlockBondageGearDef != null}");
			}
			else
			{
				EALogger.Warn("[RJW] xxx type not found — JobDefs will be unavailable");
			}
		}
		catch (Exception ex)
		{
			EALogger.Error("[RJW] EnsureInitialized failed: " + ex.Message, ex);
		}
	}

	private static Type FindType(string typeName)
	{
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			try
			{
				Type type = assembly.GetType(typeName);
				if (type != null)
				{
					return type;
				}
			}
			catch (Exception ex)
			{
				EALogger.Debug("[RJW] FindType(" + typeName + ") skipped assembly " + assembly.GetName().Name + ": " + ex.Message);
			}
		}
		return null;
	}

	public static bool CanFuck(Pawn pawn)
	{
		EnsureInitialized();
		if (_canFuck == null)
		{
			EALogger.Debug("[RJW] CanFuck: method not resolved, returning false");
			return false;
		}
		try
		{
			bool flag = (bool)_canFuck.Invoke(null, new object[1] { pawn });
			EALogger.Debug($"[RJW] CanFuck({pawn.LabelShort}): {flag}");
			return flag;
		}
		catch (Exception ex)
		{
			EALogger.Warn("[RJW] CanFuck(" + pawn.LabelShort + ") reflection failed: " + (ex.InnerException?.Message ?? ex.Message));
			return false;
		}
	}

	public static bool CanBeFucked(Pawn pawn)
	{
		EnsureInitialized();
		if (_canBeFucked == null)
		{
			EALogger.Debug("[RJW] CanBeFucked: method not resolved, returning false");
			return false;
		}
		try
		{
			bool flag = (bool)_canBeFucked.Invoke(null, new object[1] { pawn });
			EALogger.Debug($"[RJW] CanBeFucked({pawn.LabelShort}): {flag}");
			return flag;
		}
		catch (Exception ex)
		{
			EALogger.Warn("[RJW] CanBeFucked(" + pawn.LabelShort + ") reflection failed: " + (ex.InnerException?.Message ?? ex.Message));
			return false;
		}
	}

	public static bool CanMasturbate(Pawn pawn)
	{
		EnsureInitialized();
		if (_canMasturbate == null)
		{
			EALogger.Debug("[RJW] CanMasturbate: method not resolved, returning false");
			return false;
		}
		try
		{
			bool flag = (bool)_canMasturbate.Invoke(null, new object[1] { pawn });
			EALogger.Debug($"[RJW] CanMasturbate({pawn.LabelShort}): {flag}");
			return flag;
		}
		catch (Exception ex)
		{
			EALogger.Warn("[RJW] CanMasturbate(" + pawn.LabelShort + ") reflection failed: " + (ex.InnerException?.Message ?? ex.Message));
			return false;
		}
	}

	public static JobDef GetJoinInBedDef()
	{
		EnsureInitialized();
		if (_joinInBedDef == null)
		{
			EALogger.Debug("[RJW] GetJoinInBedDef: field not resolved");
			return null;
		}
		try
		{
			return (JobDef)_joinInBedDef.GetValue(null);
		}
		catch (Exception ex)
		{
			EALogger.Warn("[RJW] GetJoinInBedDef reflection failed: " + (ex.InnerException?.Message ?? ex.Message));
			return null;
		}
	}

	public static JobDef GetQuickieDef()
	{
		EnsureInitialized();
		if (_quickieDef == null)
		{
			EALogger.Debug("[RJW] GetQuickieDef: field not resolved");
			return null;
		}
		try
		{
			return (JobDef)_quickieDef.GetValue(null);
		}
		catch (Exception ex)
		{
			EALogger.Warn("[RJW] GetQuickieDef reflection failed: " + (ex.InnerException?.Message ?? ex.Message));
			return null;
		}
	}

	public static JobDef GetMasturbateDef()
	{
		EnsureInitialized();
		if (_masturbateDef == null)
		{
			EALogger.Debug("[RJW] GetMasturbateDef: field not resolved");
			return null;
		}
		try
		{
			return (JobDef)_masturbateDef.GetValue(null);
		}
		catch (Exception ex)
		{
			EALogger.Warn("[RJW] GetMasturbateDef reflection failed: " + (ex.InnerException?.Message ?? ex.Message));
			return null;
		}
	}

	public static JobDef GetGiveBondageGearDef()
	{
		EnsureInitialized();
		if (_giveBondageGearDef == null)
		{
			EALogger.Debug("[RJW] GetGiveBondageGearDef: field not resolved");
			return null;
		}
		try
		{
			return (JobDef)_giveBondageGearDef.GetValue(null);
		}
		catch (Exception ex)
		{
			EALogger.Warn("[RJW] GetGiveBondageGearDef reflection failed: " + (ex.InnerException?.Message ?? ex.Message));
			return null;
		}
	}

	public static JobDef GetUnlockBondageGearDef()
	{
		EnsureInitialized();
		if (_unlockBondageGearDef == null)
		{
			EALogger.Debug("[RJW] GetUnlockBondageGearDef: field not resolved");
			return null;
		}
		try
		{
			return (JobDef)_unlockBondageGearDef.GetValue(null);
		}
		catch (Exception ex)
		{
			EALogger.Warn("[RJW] GetUnlockBondageGearDef reflection failed: " + (ex.InnerException?.Message ?? ex.Message));
			return null;
		}
	}

	public static bool IsFrustrated(Pawn pawn)
	{
		EnsureInitialized();
		if (_isFrustrated == null)
		{
			return false;
		}
		try
		{
			return (bool)_isFrustrated.Invoke(null, new object[1] { pawn });
		}
		catch (Exception ex)
		{
			EALogger.Debug("[RJW] IsFrustrated(" + pawn.LabelShort + ") failed: " + (ex.InnerException?.Message ?? ex.Message));
			return false;
		}
	}

	public static float GetSexDrive(Pawn pawn)
	{
		EnsureInitialized();
		if (_getSexDrive == null)
		{
			return 0f;
		}
		try
		{
			return (float)_getSexDrive.Invoke(null, new object[1] { pawn });
		}
		catch (Exception ex)
		{
			EALogger.Debug("[RJW] GetSexDrive(" + pawn.LabelShort + ") failed: " + (ex.InnerException?.Message ?? ex.Message));
			return 0f;
		}
	}
}
