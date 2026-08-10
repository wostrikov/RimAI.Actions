using System;
using System.Text;
using RimTalk.ExpandActions.Core;
using RimTalk.ExpandActions.RJW.Util;
using RimWorld;
using Verse;

namespace RimTalk.ExpandActions.RJW.State;

public class RJWStatusProvider : IEAVariableContributor
{
	public string VariableName => "rjw_status";

	public string GetValue(object pawnContext)
	{
		if (!(pawnContext is Pawn pawn))
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		Type needSexType = RJWReflectionCache.NeedSexType;
		if (needSexType != null)
		{
			Need need = pawn.needs?.AllNeeds?.FirstOrDefault((Need n) => n.GetType() == needSexType);
			if (need != null)
			{
				stringBuilder.Append($"Sex need: {need.CurLevel:P0}");
			}
		}
		if (RJWReflectionCache.IsFrustrated(pawn))
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append("sexually frustrated");
		}
		float sexDrive = RJWReflectionCache.GetSexDrive(pawn);
		if (sexDrive > 0f)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(", ");
			}
			if (1 == 0)
			{
			}
			string text = ((sexDrive > 1.5f) ? "very high" : ((sexDrive > 1f) ? "high" : ((!(sexDrive > 0.5f)) ? "low" : "moderate")));
			if (1 == 0)
			{
			}
			string text2 = text;
			stringBuilder.Append("sex drive: " + text2);
		}
		if (pawn.health?.hediffSet != null)
		{
			foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
			{
				if (hediff.def.defName.Contains("Pregnancy") || hediff.def.defName.Contains("pregnant"))
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.Append(", ");
					}
					stringBuilder.Append("pregnant");
					break;
				}
			}
		}
		if (pawn.story?.traits != null)
		{
			Trait trait = pawn.story.traits.GetTrait(TraitDef.Named("Nymphomaniac"));
			if (trait != null)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append("nymphomaniac");
			}
		}
		return (stringBuilder.Length > 0) ? stringBuilder.ToString() : null;
	}
}
