using Ustas.RimAI.Actions.Actions;
using Ustas.RimAI.Actions.Core;
using Ustas.RimAI.Actions.Execution;
using Ustas.RimAI.Actions.RJW.Util;
using Verse;
using Verse.AI;

namespace Ustas.RimAI.Actions.RJW.Handlers;

public class RJWMasturbateHandler : IActionHandler
{
	public string ActionId => "rjw_masturbate";

	public ExecutionResult Execute(ExecutionContext context)
	{
		Pawn resolvedActor = context.ResolvedActor;
		if (!RJWReflectionCache.CanMasturbate(resolvedActor))
		{
			return ExecutionResult.Failed(context, ErrorCode.ActorIncapable, "Actor cannot masturbate");
		}
		JobDef masturbateDef = RJWReflectionCache.GetMasturbateDef();
		if (masturbateDef == null)
		{
			return ExecutionResult.Failed(context, ErrorCode.ExecutionException, "RJW_Masturbate JobDef not found");
		}
		Job job = JobMaker.MakeJob(masturbateDef, resolvedActor, null, resolvedActor.Position);
		context.StartOrQueueJob(job);
		return ExecutionResult.Succeeded(context);
	}
}
