# Combat Loop References

## Reference Documents

- `docs/plans/2026-03-13-tower-breaker-vertical-slice/00-overview.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/02-combat-loop.md`
- `docs/plans/2026-03-13-implementation-workflow.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/archive/data-schema/README.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/archive/addressables-provider/README.md`

## Existing Code References

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

## Expected New Work In This Milestone

- `Assets/Scripts/Combat/`
- `Assets/Tests/EditMode/Combat/`
- possible scene/prefab glue under `Assets/Scenes/` and `Assets/Prefabs/Combat/`

## New Work In This Milestone

- `Assets/Scripts/Combat/TowerBreak.Combat.asmdef`
- `Assets/Scripts/Combat/WaveSpawnEntry.cs`
- `Assets/Scripts/Combat/WaveSpawnPlan.cs`
- `Assets/Scripts/Combat/WaveSpawnPlanner.cs`
- `Assets/Scripts/Combat/CombatEnemyState.cs`
- `Assets/Scripts/Combat/CombatState.cs`
- `Assets/Scripts/Combat/CombatAttackResult.cs`
- `Assets/Scripts/Combat/CombatDebugBattleService.cs`
- `Assets/Scripts/Combat/EnemySpawnPresenter.cs`
- `Assets/Scripts/Combat/ICombatInstantiator.cs`
- `Assets/Scripts/Combat/UnityCombatInstantiator.cs`
- `Assets/Scripts/Combat/PooledCombatInstantiator.cs`
- `Assets/Scripts/Combat/EnemySpawnRuntimeSpawner.cs`
- `Assets/Scripts/Combat/CombatDebugAddressableAssetProvider.cs`
- `Assets/Scripts/Combat/CombatDebugSpawnService.cs`
- `Assets/Scripts/Combat/CombatSampleSceneBootstrap.cs`
- `Assets/Tests/EditMode/Combat/TowerBreak.Combat.Tests.asmdef`
- `Assets/Tests/EditMode/Combat/WaveSpawnPlannerTests.cs`
- `Assets/Tests/EditMode/Combat/CombatStateTests.cs`
- `Assets/Tests/EditMode/Combat/CombatDebugBattleServiceTests.cs`
- `Assets/Tests/EditMode/Combat/CombatDebugSpawnServiceTests.cs`
- `Assets/Resources/TowerBreakerGameData.asset`
