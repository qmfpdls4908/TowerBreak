# Combat Loop Checklist

## Reference Documents

- `docs/plans/2026-03-13-tower-breaker-vertical-slice/00-overview.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/02-combat-loop.md`
- `docs/plans/2026-03-13-implementation-workflow.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/archive/data-schema/README.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/archive/addressables-provider/README.md`

## Checklist

- Done: create milestone folder and seed milestone docs
- Done: research combat baseline, archived foundations, and available scene surface
- Done: lock the first narrow combat checkpoint for this milestone
- Done: add the first failing combat planning tests
- Done: implement the first floor/wave to spawn-plan runtime slice
- Done: verify the spawn-plan slice with build and targeted tests
- Done: add the first presentation glue for prefab-key-based enemy spawn
- Done: add the first runtime spawn glue that instantiates a loaded prefab
- Done: wire the spawn planner into a scene-visible debug path
- Done: verify `Resources` data asset loading for the debug path
- Done: fix the duplicate-visible-template issue in the temporary debug provider
- Done: add the first pooled enemy instantiator and verify reuse behavior
- Done: add `CombatPooledScenePathTests` - spawn/release/reuse integration tests through `CombatDebugSpawnService`
- Done: route the actual scene debug path through `PooledCombatInstantiator` in `CombatSampleSceneBootstrap`
- Done: add spawn→release→reuse demonstration in bootstrap with `Debug.Log` pool_reuse verification
- Done: add warning-level summary output so console verification remains visible when normal logs are missed
- Done: add `CombatProviderResolver` to resolve real `IAddressableAssetProvider` from DI with debug fallback
- Done: wire `TowerBreak.Combat.asmdef` to reference `TowerBreak.DI`
- Done: add `CombatProviderResolverTests` (2 tests: DI resolution + debug fallback)
- Done: author `CombatAddressableGroupSetup` Editor menu tool to create placeholder prefab and Addressables group
- Done: fix `TowerBreak.Combat.Editor.asmdef` to reference `Unity.Addressables.Editor`
- Done: run `TowerBreak/Setup/Author Combat Enemy Addressables` in Unity Editor → real prefab registered
- Done: verify real Addressables spawn in SampleScene Play Mode → `EnemyBasicMelee(Clone)` visible, pool_reuse=True confirmed
- Done: add `SpawnAllEnemiesAsync` to `CombatDebugSpawnService` - spawns all wave entries in plan order
- Done: add `CombatMultiWaveSpawnTests` (4 tests: count/positions/release-reuse/same-prefab-distinct)
- Done: update `CombatSampleSceneBootstrap` to demonstrate multi-enemy spawn + release + pool reuse
- Done: add `BuildMultiSummaryMessage`/`LogMultiSummary` static helpers for console verification
- Done: fix `00-overview.md` archived milestone list format
- Done: fix `SpawnAllEnemiesAsync` so it honors `WaveSpawnEntry.Quantity`
- Done: extend `CombatAddressableGroupSetup` to create+register `EnemyArmoredPusher` (Cube, 2×2) at `enemy/armored_pusher`
- Done: add initial `CombatState` / `CombatEnemyState` model with time and wall-health transitions
- Done: add first player attack rule for enemy damage and enemy removal on defeat
- Done: add `CombatDebugBattleService` as a thin bridge from `CombatState` into the debug scene
- Done: wire `Space` key in `CombatSampleSceneBootstrap` to apply the first attack rule
- Manual: verify `Space` attacks reduce/remove the first active enemy in `SampleScene`
- Manual: run `TowerBreak/Setup/Author Combat Enemy Addressables` in Unity Editor to create `EnemyArmoredPusher.prefab` and register it
- Manual: verify SampleScene Play Mode shows 3 enemies (2×BasicMelee + 1×ArmoredPusher) with no `InvalidKeyException`

## Verification

- Passed command: `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "C:\Users\admin\Desktop\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.Combat.Tests.WaveSpawnPlannerTests" -logFile "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\combat-wave-green.log" -testResults "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\combat-wave-green.xml"`
- Evidence: `Logs\combat-wave-green.xml` shows `total="2" passed="2" failed="0"`
- Passed command: `dotnet build "TowerBreak.GameData.Tests.csproj"`
- Passed command: `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "C:\Users\admin\Desktop\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.Combat.Tests.WaveSpawnPlannerTests" -logFile "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\combat-presenter-green.log" -testResults "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\combat-presenter-green.xml"`
- Evidence: `Logs\combat-presenter-green.xml` shows `total="3" passed="3" failed="0"`
- Passed command: `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "C:\Users\admin\Desktop\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.Combat.Tests.WaveSpawnPlannerTests" -logFile "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\combat-spawn-green.log" -testResults "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\combat-spawn-green.xml"`
- Evidence: `Logs\combat-spawn-green.xml` shows `total="4" passed="4" failed="0"`
- Passed command: `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "C:\Users\admin\Desktop\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.Combat.Tests.CombatDebugSpawnServiceTests" -logFile "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\combat-debug-service-green.log" -testResults "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\combat-debug-service-green.xml"`
- Evidence: `Logs\combat-debug-service-green.xml` shows `total="2" passed="2" failed="0"`
- Passed command: `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "C:\Users\admin\Desktop\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.Combat.Tests.CombatDebugSpawnServiceTests" -logFile "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\combat-debug-visibility-green.log" -testResults "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\combat-debug-visibility-green.xml"`
- Evidence: `Logs\combat-debug-visibility-green.xml` shows `total="3" passed="3" failed="0"`
- Passed command: `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "C:\Users\admin\Desktop\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.Combat.Tests.CombatDebugSpawnServiceTests" -logFile "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\combat-pooling-green.log" -testResults "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\combat-pooling-green.xml"`
- Evidence: `Logs\combat-pooling-green.xml` shows `total="4" passed="4" failed="0"`
- Passed command: `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "C:\Users\admin\Desktop\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.Combat.Tests.CombatPooledScenePathTests" -logFile "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\combat-pooled-scene-path.log" -testResults "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\combat-pooled-scene-path.xml"`
- Evidence: `Logs\combat-pooled-scene-path.xml` shows `total="3" passed="3" failed="0"`
- Passed command: `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "C:\Users\admin\Desktop\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.Combat.Tests" -logFile "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\combat-pooled-all-green.log" -testResults "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\combat-pooled-all-green.xml"`
- Evidence: `Logs\combat-pooled-all-green.xml` shows `total="11" passed="11" failed="0"`
- Passed command: `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "C:\Users\admin\Desktop\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.Combat.Tests.CombatPooledScenePathTests" -logFile "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\combat-summary-green.log" -testResults "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\combat-summary-green.xml"`
- Evidence: `Logs\combat-summary-green.xml` shows `total="5" passed="5" failed="0"`
- Passed command: `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "C:\Users\admin\Desktop\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.Combat.Tests" -logFile "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\combat-di-provider-green.log" -testResults "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\combat-di-provider-green.xml"`
- Evidence: `Logs\combat-di-provider-green.xml` shows `total="15" passed="15" failed="0"` (includes 2 new `CombatProviderResolverTests`)
- Passed command: `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "C:\Users\admin\Desktop\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.Combat.Tests" -logFile "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\combat-multiwave-green.log" -testResults "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\combat-multiwave-green.xml"`
- Evidence: `Logs\combat-multiwave-green.xml` shows `total="19" passed="19" failed="0"` (includes 4 new `CombatMultiWaveSpawnTests`)
- Passed command: `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "C:\Users\admin\Desktop\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.Combat.Tests.CombatMultiWaveSpawnTests" -logFile "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\combat-quantity-fixed.log" -testResults "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\combat-quantity-fixed.xml"`
- Evidence: `Logs\combat-quantity-fixed.xml` shows `total="5" passed="5" failed="0"`
- Passed command: `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "C:\Users\admin\Desktop\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.Combat.Tests" -logFile "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\combat-full-fixed.xml.log" -testResults "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\combat-full-fixed.xml"`
- Evidence: `Logs\combat-full-fixed.xml` shows `total="20" passed="20" failed="0"`
- Passed command: `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "C:\Users\admin\Desktop\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.Combat.Tests" -logFile "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\combat-armored-pusher-green.log" -testResults "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\combat-armored-pusher-green.xml"`
- Evidence: `Logs\combat-armored-pusher-green.xml` shows `total="20" passed="20" failed="0"` (regression check after setup tool expansion)
- Passed command: `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "C:\Users\admin\Desktop\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.Combat.Tests.CombatStateTests" -logFile "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\combat-state-green.log" -testResults "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\combat-state-green.xml"`
- Evidence: `Logs\combat-state-green.xml` shows `total="3" passed="3" failed="0"`
- Passed command: `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "C:\Users\admin\Desktop\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.Combat.Tests.CombatStateTests" -logFile "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\combat-attack-green.log" -testResults "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\combat-attack-green.xml"`
- Evidence: `Logs\combat-attack-green.xml` shows `total="5" passed="5" failed="0"`
- Passed command: `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "C:\Users\admin\Desktop\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.Combat.Tests.CombatDebugBattleServiceTests" -logFile "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\combat-debug-battle-green.log" -testResults "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\combat-debug-battle-green.xml"`
- Evidence: `Logs\combat-debug-battle-green.xml` shows `total="3" passed="3" failed="0"`
- Passed command: `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "C:\Users\admin\Desktop\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.Combat.Tests.CombatStateTests" -logFile "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\combat-attack-green.log" -testResults "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\combat-attack-green.xml"`
- Evidence: `Logs\combat-attack-green.xml` shows `total="5" passed="5" failed="0"`

## Manual Follow-Ups

- Open SampleScene in Unity and enter Play Mode
- Check Console for multi-spawn log lines:
  - `[CombatDebug] Wave spawn [0]: EnemyBasicMelee(Clone) (id=<N>)`
  - `[CombatDebug] Wave spawn [1]: EnemyBasicMelee(Clone) (id=<N>)`
  - `[CombatDebug] Wave spawn [2]: EnemyArmoredPusher(Clone) (id=<N>)`
  - `[CombatDebug] Released 3 instances back to pool.`
  - `[CombatDebug] Second spawn [0]: ... pool_reuse=True`
  - `[CombatDebug] Second spawn [1]: ... pool_reuse=True`
  - `[CombatDebug] Second spawn [2]: ... pool_reuse=True`
- Check for the warning summary line:
  - `[CombatDebugSummary] wave_count=3 pool_reuse_count=3`
- The Hierarchy should show 3 enemy clones parented under CombatSampleSceneBootstrap
- No `InvalidKeyException: No Location found for Key=enemy/armored_pusher` in Console
- If armored pusher is still missing: run `TowerBreak > Setup > Author Combat Enemy Addressables` from the menu bar to create `Assets/Prefabs/Combat/Enemies/EnemyArmoredPusher.prefab` and register it at address `enemy/armored_pusher`
