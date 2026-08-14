"""Deterministic static checks for the RimTalk -> RimAI frontend path."""

from pathlib import Path
import unittest


ROOT = Path(__file__).resolve().parents[1] / "Source" / "Core"
RIMAI = Path(__file__).resolve().parents[2] / "RimAI.Core" / "src" / "RimAI.RimWorld"


class ActionLifecycleRegressionTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls) -> None:
        cls.frontend = read("RimTalk.ExpandActions.Frontend/RimTalkCapabilityFrontend.cs")
        cls.probe = read("RimTalk.ExpandActions.Integration/Stage6RuntimeProbe.cs")
        cls.probe_menu = read("RimTalk.ExpandActions.Patches/Patch_Stage6ProbeMenuStart.cs")
        cls.create = read("RimTalk.ExpandActions.Patches/Patch_CreateInteraction.cs")
        cls.host = (RIMAI / "Application" / "RimAIApplicationHost.cs").read_text(encoding="utf-8")

    def test_live_path_uses_rimai_host(self):
        self.assertIn("RimAIApplicationHost.ExecuteBatch", self.frontend)
        self.assertIn("RimTalkCapabilityFrontend", self.create)
        self.assertNotIn("ActionExecutor", self.create)
        self.assertIn("class RimAIApplicationHost", self.host)

    def test_production_executor_shell_is_gone(self):
        self.assertFalse((ROOT / "RimTalk.ExpandActions.Execution/ActionExecutor.cs").exists())
        self.assertFalse((ROOT / "RimTalk.ExpandActions.Integration/RimAICapabilityMigrationRouter.cs").exists())
        self.assertFalse((ROOT / "RimTalk.ExpandActions.Core/CapabilityCatalogBridge.cs").exists())
        self.assertFalse((ROOT / "RimTalk.ExpandActions.Parsing/ToolcallParser.cs").exists())

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


def read(relative: str) -> str:
    return (ROOT / relative).read_text(encoding="utf-8-sig")


if __name__ == "__main__":
    unittest.main()
