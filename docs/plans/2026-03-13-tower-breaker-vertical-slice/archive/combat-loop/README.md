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
- The next smallest schedule-aligned extension after the first attack path is enemy pressure accumulation that damages the wall and turns on a danger flag without introducing full guard/dash systems yet.
- The current debug scene can absorb that rule with minimal glue by ticking pure pressure state and logging wall damage/danger transitions rather than adding a larger battle controller early.
- In Play Mode, the real DI-registered Addressables provider can still fail when a combat key has not been authored in Addressables yet. The `SampleScene` debug bootstrap now retries once with `CombatDebugAddressableAssetProvider`, but logs a loud warning telling the user to run the setup menu so the real path can be restored.
- `CombatSampleSceneBootstrap.Update()` was using legacy `UnityEngine.Input.GetKeyDown(KeyCode.Space)`, which throws `InvalidOperationException` when the project's active Input Handling is set to "Input System Package". Fixed by replacing with `Keyboard.current?.spaceKey.wasPressedThisFrame` and extracting a testable `IsAttackInputDown()` static seam.
- The next smallest loss-state follow-up after guard is a pure wall-defeat outcome: once `WallHealth` reaches `0`, `CombatState` should expose that as explicit battle loss state and the debug scene should stop further pressure/input progression with one loud log line.
- The next smallest HUD-visible feedback after wall defeat is not a full presenter yet; it is a one-line scene overlay in `CombatSampleSceneBootstrap` that mirrors the current debug-console validation pattern by showing `DANGER` or `DEFEAT` directly on screen.
- The next smallest architecture-aligned follow-up after the overlay is to stop growing `CombatSampleSceneBootstrap` directly and extract a thin `BattleLoopController` that owns pressure ticking / defeat gating plus a `BattleHudPresenter` that owns status text formatting.
- That controller/presenter split is now in place, so the SampleScene path is still minimal but no longer keeps all combat progression rules inside the bootstrap MonoBehaviour.
- During review, one regression surfaced: async enemy-release cleanup after `Space` attack was pausing the whole battle loop because `Update()` early-returned while an attack task was in flight. Fixed by separating "tick combat" from "start a new attack" so pressure/guard continue to advance during async cleanup.

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
- Done: connect the attack rule to a minimal scene-visible trigger or player input path
- Done: add failing tests for enemy pressure accumulation, wall damage, and initial danger activation
- Done: implement pure enemy pressure accumulation in `CombatState` with wall-hit threshold handling
- Done: add `CombatPressureResult` + `CombatDebugBattleService.AdvanceEnemyPressure` bridge for scene/debug use
- Done: tick enemy pressure in `CombatSampleSceneBootstrap` and log wall damage / danger activation
- Done: make `SampleScene` bootstrap recover from missing combat Addressables keys by retrying once with the debug provider and an explicit warning
- Done: fix debug attack input — replace legacy `UnityEngine.Input.GetKeyDown(KeyCode.Space)` with Input System `Keyboard.current.spaceKey.wasPressedThisFrame`; add `IsAttackInputDown()` public static seam; add `Unity.InputSystem` to `TowerBreak.Combat.asmdef`
- Done: add `IsAttackInputDown_WhenNoKeyboardDevice_ReturnsFalse` test to `CombatPooledScenePathTests` (seam is null-safe in EditMode batch)
- Done: add guard action — `CombatState.ApplyPlayerGuard` reduces `PendingEnemyPressure`, danger flag stays sticky; `CombatDebugBattleService.ApplyPlayerGuard` bridge returns `CombatGuardResult`; G key wired in `CombatSampleSceneBootstrap`; `IsGuardInputDown()` seam + seam test added
- Done: add wall-defeat outcome — `CombatState.IsWallDefeated`, `CombatPressureResult.DefeatedWall`, and one-time debug loss log that stops further scene actions once the wall is depleted
- Done: add minimal HUD-visible danger/defeat feedback via `CombatSampleSceneBootstrap.OnGUI()` and `BuildOverlayStatusMessage(CombatState)`
- Done: add failing tests for minimal `BattleLoopController` pressure-tick / defeat-gating behavior and `BattleHudPresenter` status formatting
- Done: extract `BattleLoopController` so pressure ticking and wall-defeat gating no longer live directly in `CombatSampleSceneBootstrap`
- Done: extract `BattleHudPresenter` and route `CombatSampleSceneBootstrap.OnGUI()` through it
- Done: fix `pool_reuse_count=1` Play Mode regression — release loop in `DemonstrateMultiSpawnAsync` now expands by `entry.Quantity` so all 3 instances are released under correct prefab keys; `pool_reuse_count=3` now expected
- Done: fix overlay not visible on second+ Play session — `overlayStyle` changed from `private static` to `private` instance field so `GUIStyle` is freshly initialized each Play session
- Todo: decide whether to finish `combat-loop` with one more narrow player/enemy presenter pass or close the milestone and begin `meta-loop`

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
- Passed: `Logs\combat-pressure-green.xml` with `11/11` tests passing for targeted `CombatStateTests` + `CombatDebugBattleServiceTests`
- Passed: `Logs\combat-pressure-full.xml` with `31/31` tests passing for all `TowerBreak.Combat.Tests`
- Passed: `Logs\combat-fallback-green.xml` with `19/19` tests passing for fallback detection + pressure-related combat fixtures
- Passed: `Logs\combat-input-fix-green.xml` with `35/35` tests passing for all `TowerBreak.Combat.Tests` including new `IsAttackInputDown_WhenNoKeyboardDevice_ReturnsFalse`
- Passed: `Logs\combat-guard-green.xml` with `40/40` tests passing for all `TowerBreak.Combat.Tests` including 5 new guard tests
- Passed: `Logs\combat-wall-defeat-green.xml` with `29/29` tests passing for targeted wall-defeat fixtures
- Passed: `Logs\combat-wall-defeat-full.xml` with `44/44` tests passing for all `TowerBreak.Combat.Tests`
- Passed: `Logs\combat-overlay-green.xml` with `14/14` tests passing for `CombatPooledScenePathTests` including overlay message helpers
- Passed: `Logs\combat-overlay-full.xml` with `47/47` tests passing for all `TowerBreak.Combat.Tests`
- Expected red phase: `Logs\combat-controller-red.log` failed to compile because `BattleLoopController` was missing
- Passed: `Logs\combat-controller-green.xml` with `22/22` tests passing for `BattleLoopControllerTests`, `BattleHudPresenterTests`, and `CombatPooledScenePathTests`
- Expected red phase: `Logs\combat-controller-followup-red.log` failed to compile because `CombatSampleSceneBootstrap.ShouldTickCombat` did not exist yet
- Passed: `Logs\combat-controller-followup-green.xml` with `21/21` tests passing for the controller/bootstrap follow-up seam
- Passed: `Logs\combat-controller-full.xml` with `58/58` tests passing for all `TowerBreak.Combat.Tests`
- Passed: `Logs\combat-poolreuse-fix.xml` with `59/59` tests passing for all `TowerBreak.Combat.Tests` including new `SpawnAllEnemiesAsync_ReleaseAllWithQuantityExpansion_ReusesAllInstances`
- Attempted: `dotnet build "TowerBreak.GameData.Tests.csproj"`
- Actual status: blocked in this shell because `dotnet` is not installed or not on `PATH`

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
- After a short idle delay with enemies still alive, check for pressure logs such as:
  - `[CombatDebug] Enemy pressure wall_damage=1 wall_health=4 danger=True pending_pressure=<value>`
- Press `Space` between pressure ticks and confirm enemy count drops while wall pressure logs continue only while enemies remain — no `InvalidOperationException` about `UnityEngine.Input` should appear
- Press `G` to guard and confirm a log appears: `[CombatDebug] Guard applied pressure_reduced=5.00 pending_pressure=<value>` — pressure should drop immediately
- While danger is active but before defeat, confirm a top-left on-screen label appears: `DANGER - Wall <value>`
- Let the wall reach `0` and confirm one defeat warning appears, then no further attack/guard/pressure logs continue:
  - `[CombatDebug] Wall defeated. Battle lost. wall_health=0`
- After defeat, confirm the on-screen label switches to `DEFEAT - Wall 0`
- Regression check after controller extraction: confirm the same visible behavior still works even though pressure ticking is now routed through `BattleLoopController` instead of staying in `CombatSampleSceneBootstrap`
- If real Addressables are still missing, expect one warning and then a successful debug fallback instead of a hard stop:
  - `[CombatDebug] Addressables enemy spawn failed. Falling back to debug combat prefabs for SampleScene...`
