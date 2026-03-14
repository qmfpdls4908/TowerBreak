# Combat Loop Research

## Reference Documents

- `docs/plans/2026-03-13-tower-breaker-vertical-slice/00-overview.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/02-combat-loop.md`
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
- `Assets/Tests/EditMode/DI/DIContainerTests.cs`
- `Assets/Tests/EditMode/GameData/Addressables/AddressableAssetProviderTests.cs`

## Research Findings

### Current baseline

- There is no checked-in combat runtime code yet.
- There is no checked-in combat EditMode test assembly yet.
- There is only one scene checked in: `Assets/Scenes/SampleScene.unity`.

### Existing reusable foundations

- `TowerBreaker-MVP.xlsx` now carries floor, wave, enemy, weapon, and reward placeholder data.
- The repo has a working `ExcelWorkbookReader`, but not yet a generic workbook-to-asset import pipeline.
- Addressables provider and global DI registration are already in place and verified.

### Practical first-slice implication

- The first combat milestone should not try to solve the entire combat design from `02-combat-loop.md` at once.
- The next high-value checkpoint is a visible data-driven spawn path: choose a floor, read a wave plan, resolve an enemy prefab key, and instantiate a placeholder enemy object.
- That checkpoint gives the user a meaningful reason to open Unity because it creates something visually inspectable.
- Object pooling is likely beneficial for user experience later, but it is a follow-up optimization/architecture step after the first visible spawn path exists.

### First implementation findings

- A small `WaveSpawnPlanner` is sufficient as the first combat domain slice.
- The useful minimum output is a deterministic list of spawn entries carrying `WaveIndex`, `EnemyId`, `SpawnOrder`, `SpawnTime`, `Quantity`, and `PrefabKey`.
- Missing prefab keys should fail fast in the planning step so the later scene glue does not silently continue with broken content data.
- The first presentation seam can stay extremely thin: it only needs to request `GameObject` prefabs from `IAddressableAssetProvider` using the `PrefabKey` already present on `WaveSpawnEntry`.
- Pooling should wrap a proven spawn/despawn path, not precede it. Otherwise the milestone risks solving lifecycle complexity before confirming the content path is even correct.
- The first runtime spawn seam can also stay thin if prefab instantiation is wrapped behind `ICombatInstantiator`, keeping instantiation testable and making later pooling replacement easier.
- To get a user-visible checkpoint before real Addressables groups are authored, a temporary debug provider can stand in for actual prefab assets while preserving the same `IAddressableAssetProvider`-based flow.
- If the temporary debug provider creates a scene object as its template, that template must stay hidden and inactive or it will appear alongside the spawned clone.
- Pooling can start safely at the `ICombatInstantiator` layer because that is where repeated instantiate/release behavior becomes explicit without entangling planning logic.
- The next schedule-aligned step after spawn validation is a small combat state model that can hold player health, wall health, elapsed time, and active enemy state without depending on scene objects.
- The next smallest step after state modeling is a tiny battle service that applies one attack rule to the first active enemy. That service can then be called from a debug scene trigger before full input/combat controllers exist.
- The scene path switch from `UnityCombatInstantiator` to `PooledCombatInstantiator` required no interface changes; both implement `ICombatInstantiator.Instantiate`.
- For spawn→release→reuse in the bootstrap demo, the caller needs both the `PooledCombatInstantiator` reference (for `Release`) and the `CombatDebugAddressableAssetProvider` reference (to retrieve the cached prefab by key). The provider caches by key, so `LoadAssetAsync(sameKey)` returns the same `GameObject` instance that was originally passed to `PooledCombatInstantiator.Instantiate`, which is the correct prefab key for the pool dictionary lookup.
- `CombatPooledScenePathTests` validates the full integration: `CombatDebugSpawnService` + `PooledCombatInstantiator` + spawn/release/respawn with reuse assertion.
- The next smallest schedule-aligned combat rule is not full action resolution yet; it is enemy pressure accumulation on the existing `CombatState` so wall damage and danger can become visible before guard/dash recovery exists.
- A low-risk first danger hook is a sticky boolean on `CombatState` that becomes active on the first wall hit. That keeps the state pure and gives later guard/dash/HUD work a stable seam without inventing recovery early.
- The existing enemy rows already expose `Pressure`, so the first pressure rule can derive from active enemies directly with no schema changes.
- The thin debug bridge can compare previous and next `CombatState` snapshots and return a small result object for scene logging, avoiding combat logic inside `MonoBehaviour` code.
- Because `SampleScene` is still a debug path, a loud one-time fallback from the real provider to `CombatDebugAddressableAssetProvider` is acceptable there when manual Addressables authoring has not been done yet. That keeps the scene usable without silently masking the configuration problem.

### Real Addressables path investigation (2026-03-13)

- No `AddressableAssetsData/` folder exists in the project → Addressables groups have not been initialized yet; this must be done via Unity Editor or an Editor setup script.
- No prefabs have been created yet → `Assets/Prefabs/` folder does not exist.
- `AddressablesGlobalInstaller` registers a real `AddressableAssetProvider` (backed by `UnityAddressableAssetLoader`) in `DIGlobalContext` at `RuntimeInitializeLoadType.BeforeSceneLoad`. This means when the game runs, DI always has a real provider registered.
- `TowerBreak.Combat.asmdef` only references `TowerBreak.GameData`, not `TowerBreak.DI`. Adding `TowerBreak.DI` is required for `CombatSampleSceneBootstrap` (or a new helper class) to call `DIContainer.TryResolveFromRegistered<IAddressableAssetProvider>()`.
- The correct minimal helper is `CombatProviderResolver.Resolve()`: tries DI, falls back to `CombatDebugAddressableAssetProvider` if no DI provider is registered. In EditMode tests, `RuntimeInitializeOnLoadMethod` does not run, so DI is clean and the fallback triggers correctly; in Play mode, DI always has the real provider.
- `CombatSampleSceneBootstrap.DemonstratePoolReuseAsync` takes `CombatDebugAddressableAssetProvider` (concrete) but only calls methods on `IAddressableAssetProvider`; changing the parameter to the interface is safe and required for the DI path.
- When Addressables groups are NOT set up and the real provider is used, `UnityAddressableAssetLoader.LoadAssetAsync` fails with an exception (Addressables runtime error). This is the expected feedback loop: the user needs to run the Editor setup tool before real spawning works.
- Editor setup tool path: `Assets/Scripts/Combat/Editor/CombatAddressableGroupSetup.cs` with `TowerBreak.Combat.Editor.asmdef`; it creates `Assets/Prefabs/Combat/Enemies/EnemyBasicMelee.prefab` and registers it with Addressables address `enemy/basic_melee`.
- In EditMode tests `CombatProviderResolverTests`, the DI container must be reset in `[SetUp]` and `[TearDown]` using `DIGlobalContext.Reset()`.

### ArmoredPusher Addressables authoring (2026-03-13)

- `enemy/armored_pusher` key was missing from Addressables → `InvalidKeyException` at runtime when floor 1 wave entry 2 was reached.
- `CombatAddressableGroupSetup` was extended to also create `EnemyArmoredPusher.prefab` (Cube primitive, 2×2 scale for visual distinction from BasicMelee Quad 1.5×1.5) and register it at address `enemy/armored_pusher` in the same "Combat" Addressables group.
- The private `RegisterEntry` helper was extracted to avoid duplicating the GUID→entry wiring.
- After running the menu item again, both keys resolve and 3 enemies spawn.

### Multi-enemy wave expansion (2026-03-13)

- Floor 1 in the real `TowerBreakerGameData.asset` has two `FloorWaveRow` entries: EnemyId=101 (BasicMelee, `enemy/basic_melee`, Qty=2) at SpawnOrder=1 and EnemyId=102 (ArmoredPusher, `enemy/armored_pusher`, Qty=1) at SpawnOrder=2.
- `CombatDebugSpawnService.SpawnFirstEnemyAsync` only consumed the first entry; `SpawnAllEnemiesAsync` now iterates all plan entries in order, spawning one instance per entry.
- `CombatDebugAddressableAssetProvider` caches by string key, so two entries sharing a key (e.g., both `enemy/basic_melee`) resolve to the same prefab object — `PooledCombatInstantiator` correctly creates distinct instances for simultaneous spawns and reuses from the pool after release.
- `Func<int, Vector3> positionSelector` keeps position logic out of the service; callers decide spacing.
- Bootstrap demo: spawn all → release all → respawn all. With `CombatDebugAddressableAssetProvider`, `enemy/armored_pusher` resolves to a new Quad prefab template distinct from `enemy/basic_melee`, so pooling works correctly per prefab key.

### Debug input path fix — Input System (2026-03-13)

- `CombatSampleSceneBootstrap.Update()` used `UnityEngine.Input.GetKeyDown(KeyCode.Space)` (legacy Input Manager).
- The project's Player Settings sets active Input Handling to "Input System Package (New)". Using the legacy API at runtime throws `InvalidOperationException: You are trying to read Input using the UnityEngine.Input class, but you have switched active Input handling to Input System package`.
- Root cause confirmed: `CombatSampleSceneBootstrap.cs` line ~68 was the sole caller of `UnityEngine.Input`.
- Fix: replaced with `Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame` from `UnityEngine.InputSystem`.
- `Keyboard.current` is null in EditMode batch tests (no input device registered), so the null guard makes the static seam test safe with no Input System simulation required.
- `Unity.InputSystem` was added to `TowerBreak.Combat.asmdef` references; no changes needed to the test asmdef since `IsAttackInputDown()` returns plain `bool`.
- The change is scoped to scene glue only; no combat domain classes (`CombatState`, `CombatDebugBattleService`) were touched.

### Danger follow-up — guard action (2026-03-13)

- `IsDangerActive` is sticky: once a wall hit occurs, danger is permanently flagged. Guard does not undo wall damage or reset the flag; it only reduces `PendingEnemyPressure` to slow future wall hits.
- The smallest coherent follow-up to danger activation is `CombatState.ApplyPlayerGuard(float pressureReduction)`: pure state method, no UnityEngine, testable in EditMode.
- `CombatDebugBattleService.ApplyPlayerGuard` provides the thin bridge that computes actual reduction (clamped at 0) and returns a `CombatGuardResult`.
- Scene glue: `IsGuardInputDown()` static seam (`Keyboard.current.gKey`) in `CombatSampleSceneBootstrap.Update()` — mirrors the existing `IsAttackInputDown()` seam pattern, no new input layer.
- Guard is instant (synchronous), unlike attack which is async. No change to `isAttackInFlight` guard pattern needed.
- `CombatGuardResult` follows the same `readonly struct` pattern as `CombatPressureResult`; single field `PressureReduced`.
- `pressureReduction` value chosen for the debug scene: `5f` constant on the bootstrap — enough to make a visible difference within a 10-unit threshold tick cycle.

### Danger follow-up — wall defeat outcome (2026-03-13)

- The current combat slice still models loss through `WallHealth`, even though the longer-term design may reinterpret that as the player being pushed into the left wall. For this milestone, the smallest consistent next step is to make `WallHealth == 0` an explicit loss state instead of silently allowing combat to continue.
- `CombatState.IsWallDefeated` is the smallest pure-state seam: it can be set in both `ApplyWallDamage` and `AdvanceEnemyPressure` with no new Unity dependency and no new controller layer.
- `CombatPressureResult` already compares previous and next snapshots, so extending it with `DefeatedWall` is the smallest way to tell the bootstrap that the loss transition happened on this tick.
- In the debug scene, stopping further pressure/input progression once the wall is defeated is enough to make the outcome feel final without adding HUD or restart flow yet.
- A one-time warning log is sufficient for this milestone because console-visible debug feedback is already an established validation pattern in `CombatSampleSceneBootstrap`.

### HUD-visible feedback follow-up — status overlay (2026-03-13)

- The original plan eventually wants a scene adapter / HUD presenter layer, but the current milestone is still intentionally earlier than `BattleHudPresenter`. A tiny `OnGUI` overlay inside `CombatSampleSceneBootstrap` is the smallest acceptable bridge for now.
- The useful seam is not the `OnGUI` method itself; it is a pure formatter helper (`BuildOverlayStatusMessage`) that maps `CombatState` to a short visible label. That keeps the behavior testable in EditMode without UI simulation.
- `DANGER` should appear whenever `IsDangerActive` is true and `IsWallDefeated` is still false; `DEFEAT` should take precedence once the wall is depleted.
- Returning an empty string for safe state keeps the overlay absent outside the narrow validation moments and avoids inventing a fuller HUD vocabulary before the project is ready.
- Using one bold top-left label matches the current debug-scene role: visible enough for manual smoke testing, but still clearly temporary compared to the later dedicated presenter layer in `02-combat-loop.md`.

### Thin scene-adapter follow-up - controller / HUD presenter (2026-03-13)

- The next architecture-aligned step after the overlay was not more behavior; it was moving update-loop responsibility out of `CombatSampleSceneBootstrap` before that MonoBehaviour became the de facto battle system.
- A thin `BattleLoopController` is sufficient for the current milestone: it wraps `CombatDebugBattleService`, owns pressure tick accumulation, forwards attack/guard commands, and blocks further interaction after wall defeat.
- A separate `BattleHudPresenter.BuildStatusMessage(CombatState)` is enough to establish a presenter seam without committing yet to uGUI, TMP, or a prefab HUD hierarchy.
- Keeping `CombatSampleSceneBootstrap` as the scene entry point but routing its update / overlay calls through controller-presenter seams makes the current SampleScene path easier to replace later with a real battle scene.
- This keeps the milestone within schedule: no new action systems, no real HUD construction, no production prefabs, just a better boundary for the already-implemented combat rules.
- One review-found regression mattered immediately: attack cleanup is async because enemy defeat returns the instance to the pool after an awaited prefab load, so `Update()` must keep ticking pressure while that cleanup is in flight. Input for *starting another attack* should be gated; the whole combat loop should not be.

### Play Mode manual verification fixes (2026-03-13)

- `pool_reuse_count=1` instead of 3: `DemonstrateMultiSpawnAsync` release loop iterated `plan.Entries.Count` (2 entries) not `batch1.Count` (3). This released `batch1[1]` (a BasicMelee instance) under `armored_pusher`'s prefab key and left `batch1[2]` (ArmoredPusher) unreleased. On second spawn, only 1 pool lookup succeeded because only 1 instance was stored under the correct prefab key. Fix: inner quantity loop matching `SpawnAllEnemiesAsync`'s expansion pattern, with a shared `releaseIndex` counter tracking batch position.
- Overlay text invisible on second+ Play session: `overlayStyle` was declared `private static GUIStyle`. Unity Editor reuses static field state across Play/Stop cycles within the same editor session. On the second run, `overlayStyle != null` was true immediately (stale reference from previous session), so the style was never recreated with valid `GUI.skin` state, causing silent render failures. Fix: change to `private GUIStyle overlayStyle` (instance field) and `GetOverlayStyle()` to instance method — the field is null at `MonoBehaviour` construction each Play session and is initialized correctly on first `OnGUI` call.
- The release loop fix is covered by `SpawnAllEnemiesAsync_ReleaseAllWithQuantityExpansion_ReusesAllInstances` in `CombatMultiWaveSpawnTests`, which asserts `reuseCount == batch1.Count` after a quantity-expanded release. The overlay fix has no unit-level test because `GUIStyle` lifecycle is Editor-session-scoped behavior that only manifests in Play Mode.

## Implementation Guardrails

- Start with one narrow vertical path before deeper battle simulation.
- Keep the first tests on pure planning/state code where possible.
- Add scene-facing glue only after a minimal data/planning test surface exists.
- Do not call `AddressablesGlobalInstaller.Install()` in EditMode tests; reset `DIGlobalContext` instead to ensure a clean DI state.
- The Editor setup tool is the required manual step before real Addressables spawning works in Play mode.
- Keep pressure/danger as pure-state progression first; after the thin controller/presenter extraction, avoid growing the debug bootstrap again unless the next task explicitly needs it.
