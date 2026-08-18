using System.Collections.Generic;
using System.Text;
using Ustas.RimAI.Actions.Core;
using Ustas.RimAI.Actions.Mod;

namespace Ustas.RimAI.Actions.Integration;

public static class EAVariableProvider
{
	public class EATemplateObject
	{
		public string keywords => GetKeywords();

		public string json_format => GetJsonFormat();

		public string actions => GetActions();

		public string summary => GetSummary();

		public string act_effort => GetActEffort();
	}

	// RimAI.composition: TEMPORARY_EXPLICIT_COMPOSITION_EXCEPTION — prompt-variable bag still located globally
	public static readonly EATemplateObject Instance = new EATemplateObject();

	public static string GetKeywords(string dialogueContext = null)
	{
		EASettings settings = EAModMain.Settings;
		if (settings != null && !settings.Enabled)
		{
			return "(EA disabled)";
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Supported EA behavior keywords:");
		IEnumerable<ActionDefinition> enumerable = ActionRegistry.GetEnabledActions();
		if (EAModMain.Settings.EnableKeywordPreFilter && !string.IsNullOrEmpty(dialogueContext))
		{
			enumerable = KeywordMatcher.Match(dialogueContext, enumerable);
		}
		Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
		foreach (ActionDefinition item in enumerable)
		{
			List<string> promptKeywords = KeywordConfigManager.GetPromptKeywords(item.Id);
			if (promptKeywords == null || promptKeywords.Count == 0)
			{
				continue;
			}
			string key = item.Category.ToString();
			if (!dictionary.ContainsKey(key))
			{
				dictionary[key] = new List<string>();
			}
			foreach (string item2 in promptKeywords)
			{
				if (!dictionary[key].Contains(item2))
				{
					dictionary[key].Add(item2);
				}
			}
		}
		foreach (KeyValuePair<string, List<string>> item3 in dictionary)
		{
			stringBuilder.AppendLine("- " + item3.Key + ": " + string.Join(", ", item3.Value));
		}
		return stringBuilder.ToString();
	}

	public static string GetJsonFormat()
	{
		return "ea_observed field format:\n\"ea_observed\": [\n  \"ActorName keyword [target]\",\n  \"张三 砍树\",\n  \"李四 跟随 王五\",\n  \"赵六 攻击 敌人\"\n]\nRules:\n- Each entry: \"Actor keyword [target]\"\n- Use keywords from the supported list\n- Actor name must match pawn's full name\n- Target is optional, depends on action\n- Multiple actions for same pawn in one response: executed sequentially (first action starts, then next queues after it)\n- Different pawns execute actions in parallel\n- New dialogue response overrides previous actions for the same pawn\n\nIMPORTANT - When to use ea_observed:\n- Output ea_observed in EVERY response where pawns are performing actions\n- Include the action each pawn SHOULD be doing right now\n- If a pawn starts a NEW action (sex, moving, fighting, etc.), ALWAYS include it\n- You may omit a pawn only if they should continue their current unchanged action\n- When in doubt, include ea_observed rather than omitting it";
	}

	public static string GetActions()
	{
		EASettings settings = EAModMain.Settings;
		if (settings != null && !settings.Enabled)
		{
			return "(EA disabled)";
		}
		return ActionRegistry.GetEnabledActionsPrompt();
	}

	public static string GetActEffort()
	{
		return (EAModMain.Settings?.ActionEffortLevel ?? 1) switch
		{
			2 => "ea_observed effort: HIGH\n- You MUST include ea_observed in EVERY response, no exceptions.\n- Every pawn mentioned in the scene MUST have an action listed.\n- Describe their physical action in detail: what they are doing RIGHT NOW.\n- Even if chatting, include their body action (walking, standing, sitting, leaning, etc.)\n- If a pawn is doing something physical (sex, fighting, working, moving), ALWAYS include it.\n- Never omit ea_observed. An empty response without ea_observed is WRONG.",
			0 => "ea_observed effort: LOW\n- Only include ea_observed when a pawn's action SIGNIFICANTLY changes.\n- Do NOT include ea_observed for minor movements, idle actions, or continuing current work.\n- Include ea_observed only for: starting sex, starting a fight, fleeing, following someone, or other major action changes.\n- If all pawns are just chatting normally, do NOT include ea_observed.\n- Prefer omitting ea_observed over including trivial actions.",
			_ => "ea_observed effort: MEDIUM\n- Include ea_observed when pawns start new actions or change behavior.\n- You may omit ea_observed if all pawns continue their current actions unchanged.\n- Always include ea_observed for important actions: sex, combat, movement to new location, following.\n- For casual chatting with no action change, ea_observed is optional.",
		};
	}

	public static string GetSummary()
	{
		EASettings settings = EAModMain.Settings;
		if (settings != null && !settings.Enabled)
		{
			return "";
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("=== Expand Actions (EA) ===");
		stringBuilder.AppendLine("You can include observable behaviors in ea_observed field.");
		stringBuilder.AppendLine();
		stringBuilder.AppendLine(GetActEffort());
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("Multiple actions for same pawn in one response: executed sequentially in order. Different pawns execute in parallel. New response overrides previous actions.");
		stringBuilder.AppendLine();
		stringBuilder.AppendLine(GetJsonFormat());
		stringBuilder.AppendLine();
		stringBuilder.AppendLine(GetKeywords());
		return stringBuilder.ToString();
	}
}
