# Combat Loop Milestone

## Goal

Build the first playable combat path that consumes imported floor/enemy data and resolves enemy prefabs through the global Addressables provider.

## Reference Documents

- `docs/plans/2026-03-13-tower-breaker-vertical-slice/00-overview.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/02-combat-loop.md`
- `docs/plans/2026-03-13-implementation-workflow.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/archive/data-schema/README.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/archive/addressables-provider/README.md`

## Code And Tests Researched

- `Assets/Scenes/SampleScene.unity`
- `Assets/Scripts/GameData/TowerBreaker/TowerBreakerGameData.cs`
- `Assets/Scripts/GameData/TowerBreaker/FloorRow.cs`
- `Assets/Scripts/GameData/TowerBreaker/FloorWaveRow.cs`
- `Assets/Scripts/GameData/TowerBreaker/EnemyRow.cs`
- `Assets/Scripts/GameData/Addressables/IAddressableAssetProvider.cs`
- `Assets/Scripts/GameData/Addressables/AddressablesGlobalInstaller.cs`
- `Assets/Scripts/DI/DIInstaller.cs`
- `Assets/Scripts/DI/DIGlobalContext.cs`
- `Assets/Scripts/EventBus/EventBus.cs`

## Research Findings

- The repo currently has no `Combat` assembly or combat tests yet, so this milestone starts a new runtime/test surface.
- The gameplay data and Addressables provider foundations are already archived and ready to consume.
- `SampleScene.unity` is the only checked-in scene, so the first battle presentation path should likely start there unless a dedicated combat scene is added immediately.
- The strongest near-term validation target is not full combat depth yet, but a thin vertical path: load floor data, derive an enemy spawn plan, resolve an enemy prefab key, and instantiate a placeholder enemy in-scene.
- The first pure combat slice is now in place as a `WaveSpawnPlanner` that converts floor/wave/enemy data into deterministic spawn entries with prefab keys.
- The first presentation glue is now also in place as `EnemySpawnPresenter`, which takes a `WaveSpawnEntry` and requests the prefab through `IAddressableAssetProvider`.
- The next thin layer is now in place as `EnemySpawnRuntimeSpawner`, which takes the loaded prefab and creates a scene instance.
- A temporary scene-visible debug path now exists: `CombatSampleSceneBootstrap` loads `TowerBreakerGameData` from `Resources`, builds the first floor spawn plan, and spawns a visible placeholder enemy in `SampleScene`.
- The temporary debug provider was adjusted so the template prefab stays hidden and inactive; only the spawned clone should be visible in-scene.
- The first pooling step is now in place as `PooledCombatInstantiator`, which reuses released enemy instances for the same prefab key path.
- The bootstrap now also emits a warning-level summary line so console verification is easier even when normal log visibility is missed.

## Implementation Guardrails

- Keep the first combat pass narrow: data-driven enemy spawn before full battle rules complexity.
- Separate pure battle/domain logic from Unity scene glue from the start.
- Prefer using imported `FloorRow`, `FloorWaveRow`, and `EnemyRow` directly or via small adapters instead of introducing large abstraction layers.
- Keep Addressables usage behind `IAddressableAssetProvider`.
- Do not block the first visible spawn milestone on object pooling; add pooling immediately after the first end-to-end enemy spawn is proven.

## Checklist

- Done: create milestone folder and seed milestone docs
- Done: research current scene/code/test baseline for combat work
- Done: define the first smallest playable combat slice for this milestone
- Done: add failing tests for floor/wave to enemy spawn planning
- Done: implement the first combat runtime slice for data-driven spawn planning
- Done: verify the spawn planning slice with targeted Unity tests
- Done: add the first presentation glue for prefab-key-based enemy spawn
- Done: add the first runtime spawn glue that instantiates a loaded enemy prefab
- Done: wire the spawn planner and runtime spawner into a `SampleScene` debug path
- Done: fix the duplicate-visible-debug-prefab issue so only the spawned clone is meant to appear
- Done: add the first enemy spawn pooling implementation
- Done: add `CombatPooledScenePathTests` - spawn/release/reuse integration through `CombatDebugSpawnService`
- Done: route `CombatSampleSceneBootstrap` through `PooledCombatInstantiator`
- Done: add spawn→release→reuse demo in bootstrap with `Debug.Log pool_reuse` console verification
- Done: real Addressables spawn verified in SampleScene Play Mode (`EnemyBasicMelee(Clone)`, pool_reuse=True)
- Done: add `SpawnAllEnemiesAsync` for multi-enemy wave spawn in plan order
- Done: add `CombatMultiWaveSpawnTests` (4 tests covering count, positions, release-reuse, same-prefab-distinct)
- Done: update bootstrap to demonstrate multi-enemy spawn + pooled reuse across all wave entries
- Done: expand multi-wave spawn so `WaveSpawnEntry.Quantity` produces repeated enemy instances
- Done: start the first combat state model to move from spawn demo into actual battle rules
- Done: add the first player action rule on top of `CombatState` (`ApplyPlayerAttack`)
- Todo: connect the attack rule to a minimal scene-visible trigger or player input path

## Verification

- Passed: `Logs\combat-multiwave-green.xml` with `19/19` tests passing for all `TowerBreak.Combat.Tests`
- Passed: `Logs\combat-wave-green.xml` with `2/2` tests passing for `WaveSpawnPlannerTests`
- Passed: `dotnet build "TowerBreak.GameData.Tests.csproj"`
- Passed: `Logs\combat-presenter-green.xml` with `3/3` tests passing for `WaveSpawnPlannerTests` including `EnemySpawnPresenter`
- Passed: `Logs\combat-spawn-green.xml` with `4/4` tests passing for `WaveSpawnPlannerTests` including `EnemySpawnRuntimeSpawner`
- Passed: `Logs\combat-debug-service-green.xml` with `2/2` tests passing for `CombatDebugSpawnServiceTests`
- Passed: `Logs\combat-debug-visibility-green.xml` with `3/3` tests passing for `CombatDebugSpawnServiceTests`, including hidden template visibility behavior
- Passed: `Logs\combat-pooling-green.xml` with `4/4` tests passing for `CombatDebugSpawnServiceTests`, including `PooledCombatInstantiator` reuse
- Passed: `Logs\combat-pooled-scene-path.xml` with `3/3` tests passing for `CombatPooledScenePathTests`, including spawn→release→reuse integration
- Passed: `Logs\combat-pooled-all-green.xml` with `11/11` tests passing for all `TowerBreak.Combat.Tests`
- Passed: `Logs\combat-summary-green.xml` with `5/5` tests passing for `CombatPooledScenePathTests`, including summary warning output
- Passed: `Logs\combat-quantity-fixed.xml` with `5/5` tests passing for `CombatMultiWaveSpawnTests` after quantity expansion
- Passed: `Logs\combat-full-fixed.xml` with `20/20` tests passing for all `TowerBreak.Combat.Tests`
- Passed: `Logs\combat-state-green.xml` with `3/3` tests passing for `CombatStateTests`
- Passed: `Logs\combat-attack-green.xml` with `5/5` tests passing for `CombatStateTests` including attack rules

## Manual Follow-Ups

- Open `SampleScene` and enter Play Mode
- Check the Console for multi-spawn log lines:
  - `[CombatDebug] Wave spawn [0]: EnemyBasicMelee(Clone) (id=<N>)`
  - `[CombatDebug] Wave spawn [1]: EnemyBasicMelee(Clone) (id=<N>)`
  - `[CombatDebug] Wave spawn [2]: EnemyArmoredPusher(Clone) (id=<N>)`
  - `[CombatDebug] Released 3 instances back to pool.`
  - `[CombatDebug] Second spawn [0]: ... pool_reuse=True`
  - `[CombatDebug] Second spawn [1]: ... pool_reuse=True`
  - `[CombatDebug] Second spawn [2]: ... pool_reuse=True`
- If normal log lines are easy to miss, check for this warning summary line instead:
  - `[CombatDebugSummary] wave_count=3 pool_reuse_count=3`
- The second spawn lines should show `pool_reuse=True`, confirming the same instances are reused
- The visible enemies in the scene should appear at the second-batch positions after the demo
