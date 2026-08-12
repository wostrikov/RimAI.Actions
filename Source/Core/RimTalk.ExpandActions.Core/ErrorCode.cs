namespace RimTalk.ExpandActions.Core;

public enum ErrorCode
{
	None,
	ActorNotFound,
	ActorAmbiguous,
	TargetNotFound,
	TargetAmbiguous,
	ActionDisabled,
	ActionNotInWhitelist,
	UnsupportedActionId,
	JobNotInWhitelist,
	InvalidParameters,
	ActorIncapable,
	TargetUnreachable,
	ExecutionException,
	AlreadyExecuted,
	OnCooldown,
	JobNotQueued
}
