# Addressables Provider Milestone

## Goal

Add a small runtime Addressables provider layer that resolves string keys from imported gameplay data without leaking Addressables-specific details into row classes or domain logic.

## Reference Documents

- `docs/plans/2026-03-13-tower-breaker-vertical-slice/00-overview.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/01-foundation-and-bootstrap.md`
- `docs/plans/2026-03-13-unity-excel-gamedata-converter.md`
- `docs/plans/2026-03-13-implementation-workflow.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/archive/data-schema/README.md`

## Code And Tests Researched

- `Packages/manifest.json`
- `Assets/Scripts/DI/DIContainer.cs`
- `Assets/Scripts/DI/DIInstaller.cs`
- `Assets/Scripts/GameData/TowerBreaker/EnemyRow.cs`
- `Assets/Scripts/GameData/TowerBreaker/WeaponRow.cs`
- `Assets/Scripts/GameData/TowerBreaker/FloorRow.cs`

## Research Findings

- Addressables is already installed in the project through `Packages/manifest.json`.
- The data schema now exposes string key fields such as `PrefabKey`, `IconKey`, `BattleBackdropKey`, and `BattleBgmKey`, so the next layer should accept plain strings and resolve typed assets at runtime.
- The repo already has a DI module, so the provider should be registered through DI rather than accessed statically.
- This milestone depends on the schema milestone because the provider contract should match the key fields already frozen there.
- The first implementation now exists as a provider + validator + loader seam under runtime `GameData`, and registration now goes through a dedicated global installer path.

## Implementation Guardrails

- The provider API should accept string keys, not row objects.
- Runtime code should depend on an interface such as `IAddressableAssetProvider`.
- Validation for missing keys should fail loudly for required assets.
- Keep caching minimal until a real use case requires more.

## Checklist

- Done: inspect existing DI registration patterns in detail
- Done: define the minimum provider interface and validation surface
- Done: add failing tests for typed loads and missing-key failure behavior
- Done: implement the provider runtime code and Unity loader seam
- Done: add the global installer path and register the provider there
- Done: verify targeted fixture results through a reliable Unity test output path
- Done: capture decisive targeted test evidence

## Verification

- Passed: `dotnet build "TowerBreak.GameData.Tests.csproj"`
- Passed: `dotnet build "TowerBreak.GameData.csproj"`
- Passed: `dotnet build "TowerBreak.DI.Tests.csproj"`
- Passed: `Logs\di-global-tests-noquit.xml` with `3/3` tests passing for `DIGlobalInstallerTests`
- Passed: `Logs\addressable-provider-noquit.xml` with `5/5` tests passing for `AddressableAssetProviderTests`
- Passed: `Logs\addressable-global-noquit.xml` with `1/1` test passing for `AddressablesGlobalInstallerTests`
- Important operational note: do not use `-quit` together with `-runTests` in Unity 6 for these CLI runs, because it can terminate the editor before test completion and prevent XML result output

## Manual Follow-Ups

- Confirm later whether any initial Addressables groups or labels need editor-side setup beyond code
