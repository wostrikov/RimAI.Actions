"""Deterministic static regression checks for the reconstructed action lifecycle."""

from pathlib import Path
import unittest


ROOT = Path(__file__).resolve().parents[1] / "Source" / "Core"


def read(relative: str) -> str:
    return (ROOT / relative).read_text(encoding="utf-8-sig")


class ActionLifecycleRegressionTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls) -> None:
        cls.executor = read("RimTalk.ExpandActions.Execution/ActionExecutor.cs")
        cls.context = read("RimTalk.ExpandActions.Execution/ExecutionContext.cs")
        cls.stop = read("RimTalk.ExpandActions.Actions.Movement/StopHandler.cs")
        cls.take_inventory = read("RimTalk.ExpandActions.Actions.Item/TakeInventoryHandler.cs")
        cls.registry = read("RimTalk.ExpandActions.Core/ActionRegistry.cs")
        cls.lifecycle_patch = read("RimTalk.ExpandActions.Patches/Patch_JobLifecycle.cs")

    def test_normal_action_success_returns_result(self):
        self.assertIn("return executionResult;", self.executor)

    def test_handler_rejection_does_not_start_a_job(self):
        rejection = self.executor.index("if (byId2.Handler == null)")
        execution = self.executor.index("byId2.Handler.Execute(executionContext)")
        self.assertLess(rejection, execution)

    def test_job_cannot_start_is_a_terminal_handler_result(self):
        self.assertIn("if (!executionContext.CanActorAct())", self.executor)
        self.assertIn("ErrorCode.ActorIncapable", self.executor)

    def test_normal_job_completion_is_not_followed_by_path_stop(self):
        self.assertNotIn("StopDead", self.stop)

    def test_interrupted_job_uses_rimworld_end_current_job(self):
        self.assertIn("EndCurrentJob(JobCondition.InterruptForced)", self.stop)

    def test_execution_exception_is_converted_to_failure(self):
        self.assertIn("catch (Exception ex)", self.executor)
        self.assertIn("ErrorCode = ErrorCode.ExecutionException", self.executor)

    def test_queued_next_action_is_left_for_job_tracker(self):
        self.assertIn("jobQueue.EnqueueLast(job)", self.context)
        self.assertNotIn("StopDead", self.stop)

    def test_no_queued_next_action_returns_without_path_lock(self):
        self.assertIn("return ExecutionResult.Succeeded(context);", self.stop)
        self.assertNotIn("StopDead", self.stop)

    def test_personal_inventory_action_uses_canonical_take_inventory_job(self):
        self.assertIn('Id = "take_inventory"', self.registry)
        self.assertIn("JobDefOf.TakeInventory", self.take_inventory)
        self.assertIn('GetArg("quantity", 1)', self.take_inventory)
        self.assertIn("foreach (Thing item in items)", self.take_inventory)
        self.assertIn("ExecutionResult.Partial", self.take_inventory)

    def test_personal_inventory_action_does_not_claim_success_when_job_was_skipped(self):
        self.assertIn("TryStartOrQueueJob(job, out string failure)", self.take_inventory)
        self.assertIn("ErrorCode.JobNotQueued", self.take_inventory)
        self.assertIn("ExecutionResult.Queued", self.take_inventory)

    def test_runtime_trace_distinguishes_start_and_terminal_state(self):
        self.assertIn("state=STARTED", self.lifecycle_patch)
        self.assertIn("CompleteEntry", self.lifecycle_patch)


if __name__ == "__main__":
    unittest.main()
