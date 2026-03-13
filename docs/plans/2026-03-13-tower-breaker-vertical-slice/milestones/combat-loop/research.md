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

## Implementation Guardrails

- Start with one narrow vertical path before deeper battle simulation.
- Keep the first tests on pure planning/state code where possible.
- Add scene-facing glue only after a minimal data/planning test surface exists.
- Do not call `AddressablesGlobalInstaller.Install()` in EditMode tests; reset `DIGlobalContext` instead to ensure a clean DI state.
- The Editor setup tool is the required manual step before real Addressables spawning works in Play mode.
