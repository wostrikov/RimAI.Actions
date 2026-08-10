# RimTalk - Expand Actions Core (reconstructed source)

This repository is a clean-room-style reconstruction from the currently installed
Steam Workshop item `3661055729` (`zruic.expand.action`) for maintenance and build
reproducibility. It does not claim original authorship. The Workshop author is ZRui-C.

The two projects preserve the original runtime assembly split and identities:

- `Source/Core/RimTalk-ExpandActions.csproj`
- `Source/XXX/RimTalk-ExpandActions-XXX.csproj`

Build outputs are written outside Git under
`%LOCALAPPDATA%/Ustas/RimWorldSourceBuilds/RimTalk-ExpandActions-Core`.
No build is deployed by these projects.

Build both projects with the current RimWorld and Workshop roots supplied as MSBuild
properties. Provenance and original assembly hashes are maintained in the integration
repository under `reports/source-mods/expand-actions-core/`.
