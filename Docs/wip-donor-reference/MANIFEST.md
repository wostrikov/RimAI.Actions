# WIP / donor reference — Expand Actions Harmony patches

**Status:** non-production reference (outside compile tree)  
**Relocated:** Phase 9.2 Pre-Wave-C Repository Hygiene (2026-08-20)  
**Representation:** `.cs.txt` so architecture scanners and csproj do not treat these as production C#

## Why outside `Source/`

These files lived under `Source/RimWorld/` and were compiled by `RimAI.Actions.csproj`.  
They broke `tools\verify-all.ps1 -Mode full` (sibling Harmony/reflection guards and/or missing donor-era types).  
Normal checkout must pass verify-all; “aside → verify → restore” is not accepted.

## Manifest

| File | Classification | Former intended path | Provenance | Reactivation |
|---|---|---|---|---|
| `Patch_CreateInteraction.cs.txt` | PURE_DONOR_REFERENCE | `Source/RimWorld/Patch_CreateInteraction.cs` | Expand Actions / `RimTalk.ExpandActions.*` donor Harmony + reflection into RimTalk | Only after deliberate RimAI-owned redesign; do not drop back into Source as-is |
| `Patch_DeserializeFromJson.cs.txt` | PURE_DONOR_REFERENCE | `Source/RimWorld/Patch_DeserializeFromJson.cs` | Same donor stack (`JsonUtil.Sanitize` / `ea_observed`) | Same |
| `Patch_GenerateAndProcessTalkAsync.cs.txt` | PURE_DONOR_REFERENCE | `Source/RimWorld/Patch_GenerateAndProcessTalkAsync.cs` | Historical RimTalk async postfix; comment says live path is CreateInteraction | Same; likely obsolete |
| `Patch_PromptLanguage.cs.txt` | PARTIAL_PORT_WORK | `Source/RimWorld/Patch_PromptLanguage.cs` | Mixed RimAI language contracts + donor NS/`RimTalk` reflection | Future language-integration task under RimAI namespaces; rewrite, do not compile this text as-is |
| `Patch_TalkPresentationLanguage.cs.txt` | PARTIAL_PORT_WORK | `Source/RimWorld/Patch_TalkPresentationLanguage.cs` | Mixed RimAI language guard + `RimTalk.ExpandActions` types | Same |

## Notes

- `notes/stage3-runtime-validation.md` — DOCUMENTATION (runtime validation checklist); not compile input.
- Tracked production patches that **remain** in Source: `Patch_Stage6ProbeMenuStart.cs`, `Patch_TickManager.cs`.

## Wave C

Do **not** move these `.cs.txt` files into `Source/` during structural alignment.  
Do **not** delete without an explicit product decision.
