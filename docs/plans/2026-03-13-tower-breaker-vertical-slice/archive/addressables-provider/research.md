# Addressables Provider Research

## Reference Documents

- `docs/plans/2026-03-13-tower-breaker-vertical-slice/00-overview.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/01-foundation-and-bootstrap.md`
- `docs/plans/2026-03-13-unity-excel-gamedata-converter.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/archive/data-schema/README.md`

## Code And Tests Researched

- `Packages/manifest.json`
- `Assets/Scripts/DI/DIContainer.cs`
- `Assets/Scripts/DI/DIInstaller.cs`
- `Assets/Scripts/GameData/TowerBreaker/FloorRow.cs`
- `Assets/Scripts/GameData/TowerBreaker/EnemyRow.cs`
- `Assets/Scripts/GameData/TowerBreaker/WeaponRow.cs`

## Research Findings

### Package / dependency findings

- `com.unity.addressables` is already installed, so no package addition is needed before the provider layer is implemented.
- The provider should stay in runtime-safe code and only wrap Addressables usage, not editor-only authoring utilities.

### DI integration findings

- The repo already uses `DIInstaller` and `DIContainer`, so the provider should be registered through the existing DI path.
- A small interface-based provider aligns with the repo's preference for explicit dependencies and easier test doubles.
- `DIInstaller` creates a fresh `DIContainer` in `Awake`, registers bindings, and removes the container in `OnDestroy`, so it is the scene installer surface.
- `DIContainerTests` show that the newest registered container wins, which means a scene-specific provider registration can override an earlier root registration if needed.
- The new global-install requirement fits a separate runtime path better than overloading `DIInstaller`, so the split is now: `DIInstaller` for scene scope, `DIGlobalInstaller` plus `DIGlobalContext` for startup scope.

### Data contract findings

- The current gameplay schema already contains string key fields on rows that need loadable content.
- The provider should therefore accept simple string keys and return typed assets or explicit failures, rather than receiving schema row objects.

### Scope findings

- This milestone should solve runtime key resolution and key validation only.
- It should not yet expand into combat presentation, UI wiring, or Addressables group authoring automation.
- To keep the first implementation testable, a loader seam is useful between the provider and static `UnityEngine.AddressableAssets.Addressables` calls.
- Provider registration can now happen globally through a runtime-initialized installer without making gameplay row types depend on scene objects.

### Verification findings

- The missing test result XML issue was not caused by asmdefs, package setup, or output paths.
- The root cause was the Unity 6 CLI option combination: using `-quit` together with `-runTests` can cause the editor to exit before tests complete.
- Removing `-quit` restores normal XML generation for targeted EditMode fixtures in this repo.

## Implementation Guardrails

- Keep the first provider API tiny.
- Prefer loud exceptions for missing required keys.
- Avoid caching or pooling unless tests or real use reveal a need.
- Keep runtime consumers independent from Addressables-specific static calls by hiding those behind the provider interface.
- Keep startup registration in a plain method that the `RuntimeInitializeOnLoadMethod` entry point can call, so tests can invoke the same path directly.
