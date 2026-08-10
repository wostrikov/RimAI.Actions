using RimTalk.ExpandActions.Core;

namespace RimTalk.ExpandActions.Execution;

public class ExecutionResult
{
	public bool Success { get; set; }

	public string ActionId { get; set; }

	public string ActorName { get; set; }

	public string TargetName { get; set; }

	public ErrorCode ErrorCode { get; set; }

	public string ErrorMessage { get; set; }

	public long ExecutionTimeMs { get; set; }

	public static ExecutionResult Succeeded(ExecutionContext ctx, string description = null)
	{
		return new ExecutionResult
		{
			Success = true,
			ActionId = ctx.ActionCall.Id,
			ActorName = (ctx.ResolvedActor?.Name?.ToStringFull ?? ctx.ActionCall.Actor),
			TargetName = (ctx.ResolvedTarget?.LabelCap ?? ctx.ActionCall.Target)
		};
	}

	public static ExecutionResult Failed(ErrorCode code, string message = null)
	{
		return new ExecutionResult
		{
			Success = false,
			ErrorCode = code,
			ErrorMessage = (message ?? code.ToString())
		};
	}

	public static ExecutionResult Failed(ExecutionContext ctx, ErrorCode code, string message = null)
	{
		return new ExecutionResult
		{
			Success = false,
			ActionId = ctx?.ActionCall?.Id,
			ActorName = (ctx?.ResolvedActor?.Name?.ToStringFull ?? ctx?.ActionCall?.Actor),
			TargetName = (ctx?.ResolvedTarget?.LabelCap ?? ctx?.ActionCall?.Target),
			ErrorCode = code,
			ErrorMessage = (message ?? code.ToString())
		};
	}
}
