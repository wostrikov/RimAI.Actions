using RimTalk.ExpandActions.Core;

namespace RimTalk.ExpandActions.Execution;

public class ExecutionResult
{
	public enum LifecycleState
	{
		Accepted,
		Queued,
		Partial,
		Started,
		Completed,
		Failed
	}

	public bool Success { get; set; }

	public LifecycleState State { get; set; }

	public string ActionId { get; set; }

	public string ActorName { get; set; }

	public string TargetName { get; set; }

	public ErrorCode ErrorCode { get; set; }

	public string ErrorMessage { get; set; }

	public long ExecutionTimeMs { get; set; }

	public string Description { get; set; }

	public static ExecutionResult Succeeded(ExecutionContext ctx, string description = null)
	{
		return new ExecutionResult
		{
			Success = true,
			State = LifecycleState.Accepted,
			ActionId = ctx.ActionCall.Id,
			ActorName = (ctx.ResolvedActor?.Name?.ToStringFull ?? ctx.ActionCall.Actor),
			TargetName = (ctx.ResolvedTarget?.LabelCap ?? ctx.ActionCall.Target)
		};
	}

	public static ExecutionResult Queued(ExecutionContext ctx, string description = null)
	{
		ExecutionResult result = Succeeded(ctx, description);
		result.State = LifecycleState.Queued;
		result.Description = description;
		return result;
	}

	public static ExecutionResult Partial(ExecutionContext ctx, string description)
	{
		ExecutionResult result = Queued(ctx, description);
		result.State = LifecycleState.Partial;
		return result;
	}

	public static ExecutionResult Failed(ErrorCode code, string message = null)
	{
		return new ExecutionResult
		{
			Success = false,
			State = LifecycleState.Failed,
			ErrorCode = code,
			ErrorMessage = (message ?? code.ToString())
		};
	}

	public static ExecutionResult Failed(ExecutionContext ctx, ErrorCode code, string message = null)
	{
		return new ExecutionResult
		{
			Success = false,
			State = LifecycleState.Failed,
			ActionId = ctx?.ActionCall?.Id,
			ActorName = (ctx?.ResolvedActor?.Name?.ToStringFull ?? ctx?.ActionCall?.Actor),
			TargetName = (ctx?.ResolvedTarget?.LabelCap ?? ctx?.ActionCall?.Target),
			ErrorCode = code,
			ErrorMessage = (message ?? code.ToString())
		};
	}
}
