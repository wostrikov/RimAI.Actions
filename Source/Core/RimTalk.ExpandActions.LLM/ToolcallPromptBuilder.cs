using System.Collections.Generic;
using System.Text;
using RimTalk.ExpandActions.Core;

namespace RimTalk.ExpandActions.LLM;

public static class ToolcallPromptBuilder
{
	public static string BuildSystemPrompt()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("You are a behavior-to-action converter for a RimWorld game mod.");
		stringBuilder.AppendLine("Your task is to convert natural language behavior descriptions into structured action calls.");
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("## Available Actions");
		stringBuilder.AppendLine();
		stringBuilder.AppendLine(ActionRegistry.GetEnabledActionsPrompt());
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("## Output Format");
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("Return a JSON object with an 'actions' array:");
		stringBuilder.AppendLine("```json");
		stringBuilder.AppendLine("{");
		stringBuilder.AppendLine("  \"actions\": [");
		stringBuilder.AppendLine("    {");
		stringBuilder.AppendLine("      \"id\": \"action_id\",");
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
		stringBuilder.AppendLine("1. Only output actions that match the available actions list");
		stringBuilder.AppendLine("2. Use exact pawn names as provided in the input");
		stringBuilder.AppendLine("3. If a behavior cannot be mapped to any action, skip it");
		stringBuilder.AppendLine("4. Return empty actions array if no behaviors can be converted");
		stringBuilder.AppendLine("5. Output ONLY valid JSON, no explanations");
		return stringBuilder.ToString();
	}

	public static string BuildUserPrompt(List<string> behaviors, string speakerName)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Convert the following observed behaviors to action calls:");
		stringBuilder.AppendLine();
		if (!string.IsNullOrEmpty(speakerName))
		{
			stringBuilder.AppendLine("Speaker: " + speakerName);
			stringBuilder.AppendLine();
		}
		stringBuilder.AppendLine("Behaviors:");
		foreach (string behavior in behaviors)
		{
			stringBuilder.AppendLine("- " + behavior);
		}
		return stringBuilder.ToString();
	}
}
