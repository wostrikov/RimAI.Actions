using System.Collections.Generic;
using System.Text;
using RimAI.Core.Application;
using RimAI.Core.Catalog;
using RimAI.Core.Execution;
using RimAI.RimWorld.Application;
using Ustas.RimAI.Actions.Mod;

namespace Ustas.RimAI.Actions.LLM;

public static class ToolcallPromptBuilder
{
	public static string BuildSystemPrompt()
	{
		var catalog = RimAIApplicationHost.Catalog;
		var settings = EAModMain.Settings;
		var language = LanguageRuntime.Current;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("You are a behavior-to-action converter for a RimWorld game mod.");
		stringBuilder.AppendLine("Your task is to convert natural language behavior descriptions into structured capability requests.");
		stringBuilder.AppendLine(LanguagePromptContract.BuildHumanOutputInstruction(language));
		stringBuilder.AppendLine("Human-facing reason text, if any, must use that output language. JSON keys and capability IDs stay canonical.");
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("## Available Capabilities");
		stringBuilder.AppendLine();
		foreach (var capability in CapabilityPromptContract.ListFrontendCapabilities(catalog))
		{
			if (!IsPromptEnabled(capability, settings))
				continue;
			string alias = AliasOf(capability.CapabilityId);
			stringBuilder.Append("- ").Append(capability.CapabilityId);
			if (!string.IsNullOrEmpty(alias))
				stringBuilder.Append(" (alias ").Append(alias).Append(')');
			stringBuilder.Append(": required ").Append(string.Join(", ", capability.Parameters.Required));
			if (capability.Parameters.Optional.Count > 0)
				stringBuilder.Append("; optional ").Append(string.Join(", ", capability.Parameters.Optional));
			stringBuilder.AppendLine();
		}
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("Machine contract (authoritative):");
		stringBuilder.AppendLine(CapabilityPromptContract.BuildMachineContract(catalog));
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("## Output Format");
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("Return a JSON object with an 'actions' array:");
		stringBuilder.AppendLine("```json");
		stringBuilder.AppendLine("{");
		stringBuilder.AppendLine("  \"actions\": [");
		stringBuilder.AppendLine("    {");
		stringBuilder.AppendLine("      \"id\": \"capability_id_or_alias\",");
		stringBuilder.AppendLine("      \"actor\": \"Pawn full name\",");
		stringBuilder.AppendLine("      \"target\": \"Target name (optional)\",");
		stringBuilder.AppendLine("      \"args\": { ... } (if needed),");
		stringBuilder.AppendLine("      \"reason\": \"Brief explanation (optional)\"");
		stringBuilder.AppendLine("    }");
		stringBuilder.AppendLine("  ]");
		stringBuilder.AppendLine("}");
		stringBuilder.AppendLine("```");
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("## Rules");
		stringBuilder.AppendLine("1. Only output capabilities from the available list");
		stringBuilder.AppendLine("2. Use exact pawn names as provided in the input");
		stringBuilder.AppendLine("3. If a behavior cannot be mapped, skip it");
		stringBuilder.AppendLine("4. Return empty actions array if no behaviors can be converted");
		stringBuilder.AppendLine("5. Output ONLY valid JSON, no explanations");
		stringBuilder.AppendLine("6. Never emit retired identifiers");
		return stringBuilder.ToString();
	}

	public static string BuildUserPrompt(List<string> behaviors, string speakerName)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Convert the following observed behaviors to structured capability requests:");
		stringBuilder.AppendLine();
		if (!string.IsNullOrEmpty(speakerName))
		{
			stringBuilder.AppendLine("Speaker: " + speakerName);
			stringBuilder.AppendLine();
		}
		stringBuilder.AppendLine("Behaviors:");
		foreach (string behavior in behaviors)
			stringBuilder.AppendLine("- " + behavior);
		return stringBuilder.ToString();
	}

	private static bool IsPromptEnabled(CapabilityDescriptor capability, EASettings settings)
	{
		if (CapabilityOwnershipRegistry.TryResolve(capability.CapabilityId, out var ownership)
		    && ownership != null)
			return settings.IsActionEnabled(ownership.LegacyActionId);
		return true;
	}

	private static string AliasOf(string capabilityId)
	{
		if (CapabilityOwnershipRegistry.TryResolve(capabilityId, out var ownership) && ownership != null)
			return ownership.LegacyActionId;
		return null;
	}
}
