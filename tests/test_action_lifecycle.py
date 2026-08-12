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
        cls.probe = read("RimTalk.ExpandActions.Integration/Stage6RuntimeProbe.cs")
        cls.probe_menu = read("RimTalk.ExpandActions.Patches/Patch_Stage6ProbeMenuStart.cs")

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

    def test_wrong_save_does_not_run_probe(self):
        self.assertIn("Find.World?.info?.FileNameNoExtension, SaveName", self.probe)

    def test_correct_disposable_save_may_run_probe(self):
        self.assertIn('SaveName = "Stage6_AI_E2E_Disposable"', self.probe)
        self.assertIn("File.Delete(marker)", self.probe)

    def test_original_save_is_never_modified(self):
        self.assertNotIn('"001"', self.probe)
        self.assertNotIn("SaveGame(", self.probe)

    def test_missing_requested_save_fails_clearly(self):
        self.assertIn("RequestedSaveMissing", self.probe)
        self.assertIn("FilePathForSavedGame(SaveName)", self.probe)

    def test_load_request_executes_only_once_from_native_menu_lifecycle(self):
        self.assertIn("loadAttempted", self.probe)
        self.assertIn("SavedGameLoaderNow.LoadGameFromSaveFileNow(SaveName)", self.probe)
        self.assertIn("Root_Play.Start", self.probe_menu)
        self.assertNotIn("stableFrames", self.probe_menu)

    def test_probe_returns_to_production_main_thread_dispatcher(self):
        self.assertIn("MainThreadDispatcher.Enqueue", self.probe)


if __name__ == "__main__":
    unittest.main()
