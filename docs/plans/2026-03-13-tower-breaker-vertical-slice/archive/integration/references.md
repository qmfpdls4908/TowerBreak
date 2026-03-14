# Integration References

## Reference Documents

- `docs/plans/2026-03-13-tower-breaker-vertical-slice/00-overview.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/05-integration-and-verification.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/archive/ui-flow/README.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/archive/combat-loop/README.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/archive/meta-loop/README.md`

## Existing Code References

### UIFlow (Presenters/Views)

- `Assets/Scripts/UIFlow/Title/TitleScreenPresenter.cs`
- `Assets/Scripts/UIFlow/Title/TitleScreenView.cs`
- `Assets/Scripts/UIFlow/Title/ITitleFlowRouter.cs`
- `Assets/Scripts/UIFlow/Title/ITitleSaveStateReader.cs`
- `Assets/Scripts/UIFlow/Lobby/LobbyPresenter.cs`
- `Assets/Scripts/UIFlow/Lobby/LobbyView.cs`
- `Assets/Scripts/UIFlow/Lobby/ILobbyFlowRouter.cs`
- `Assets/Scripts/UIFlow/Lobby/ILobbyStateReader.cs`
- `Assets/Scripts/UIFlow/Battle/BattleHudPresenter.cs`
- `Assets/Scripts/UIFlow/Battle/BattleHudView.cs`
- `Assets/Scripts/UIFlow/Battle/IBattleHudStateReader.cs`
- `Assets/Scripts/UIFlow/Results/RewardResultsPresenter.cs`
- `Assets/Scripts/UIFlow/Results/RewardResultsView.cs`
- `Assets/Scripts/UIFlow/Results/IRewardResultsFlowRouter.cs`
- `Assets/Scripts/UIFlow/Results/IRewardResultsStateReader.cs`
- `Assets/Scripts/UIFlow/Growth/GrowthScreenPresenter.cs`
- `Assets/Scripts/UIFlow/Growth/GrowthScreenView.cs`
- `Assets/Scripts/UIFlow/Growth/IGrowthFlowRouter.cs`
- `Assets/Scripts/UIFlow/Growth/IGrowthStateReader.cs`

### Combat (Services/State)

- `Assets/Scripts/Combat/CombatState.cs`
- `Assets/Scripts/Combat/BattleLoopController.cs`
- `Assets/Scripts/Combat/BattleLoopUpdateResult.cs`
- `Assets/Scripts/Combat/CombatSampleSceneBootstrap.cs`
- `Assets/Scripts/Combat/WaveSpawnPlanner.cs`
- `Assets/Scripts/Combat/WaveSpawnPlan.cs`
- `Assets/Scripts/Combat/WaveSpawnEntry.cs`
- `Assets/Scripts/Combat/PooledCombatInstantiator.cs`
- `Assets/Scripts/Combat/ICombatInstantiator.cs`
- `Assets/Scripts/Combat/CombatDebugBattleService.cs`

### Meta (Services/State)

- `Assets/Scripts/Meta/State/PlayerInventoryState.cs`
- `Assets/Scripts/Meta/State/PlayerWalletState.cs`
- `Assets/Scripts/Meta/State/OwnedEquipment.cs`
- `Assets/Scripts/Meta/Rewards/BattleRewardService.cs`
- `Assets/Scripts/Meta/Rewards/RewardResolver.cs`
- `Assets/Scripts/Meta/Rewards/RewardBundle.cs`
- `Assets/Scripts/Meta/Equipment/EquipmentComparisonService.cs`
- `Assets/Scripts/Meta/Equipment/EquipmentComparisonResult.cs`
- `Assets/Scripts/Meta/Equipment/EquipmentStatBlock.cs`
- `Assets/Scripts/Meta/Equipment/EquipmentRerollService.cs`
- `Assets/Scripts/Meta/Equipment/RerollCostPolicy.cs`
- `Assets/Scripts/Meta/Equipment/RerollResult.cs`
- `Assets/Scripts/Meta/Progression/FloorProgressionService.cs`
- `Assets/Scripts/Meta/Progression/FloorProgressionResult.cs`
- `Assets/Scripts/Meta/Progression/BattleOutcome.cs`

### Infrastructure

- `Assets/Scripts/DI/DIContainer.cs`
- `Assets/Scripts/DI/DIInstaller.cs`
- `Assets/Scripts/DI/DIGlobalContext.cs`
- `Assets/Scripts/EventBus/EventBus.cs`
- `Assets/Scripts/GameData/TowerBreaker/TowerBreakerGameData.cs`
- `Assets/Scripts/GameData/TowerBreaker/FloorRow.cs`
- `Assets/Scripts/GameData/TowerBreaker/FloorWaveRow.cs`
- `Assets/Scripts/GameData/TowerBreaker/EnemyRow.cs`
- `Assets/Scripts/GameData/TowerBreaker/WeaponRow.cs`
- `Assets/Scripts/GameData/TowerBreaker/RewardTableRow.cs`
- `Assets/Scripts/GameData/TowerBreaker/RewardEntryRow.cs`

### Scenes

- `Assets/Scenes/Bootstrap.unity` - Title/start scene
- `Assets/Scenes/Lobby.unity` - Lobby hub scene
- `Assets/Scenes/Battle.unity` - Combat scene
- `Assets/Scenes/SampleScene.unity` - Combat debug sample

### Tests

- `Assets/Tests/EditMode/UIFlow/` - UIFlow tests (90 tests)
- `Assets/Tests/EditMode/Combat/` - Combat tests (58 tests)
- `Assets/Tests/EditMode/Meta/` - Meta tests (84 tests)
- `Assets/Tests/EditMode/Core/TitleToLobbyIntegrationTests.cs` - Phase 3 tests (15 tests)
- `Assets/Tests/EditMode/Core/LobbyToBattleIntegrationTests.cs` - Phase 4 tests (12 tests, pending verification)
- `Assets/Tests/EditMode/Core/BattleToRewardIntegrationTests.cs` - Phase 5 tests (12 tests, pending verification)
- `Assets/Tests/EditMode/Core/TitleToLobbyIntegrationTests.cs` - Phase 3 tests (15 tests)
- `Assets/Tests/EditMode/Core/LobbyToBattleIntegrationTests.cs` - Phase 4 tests (12 tests)

## Expected New Work In This Milestone

- `Assets/Scripts/Core/Flow/` - Scene flow controllers (if needed for future phases)
- `Assets/Scripts/Core/State/` - Concrete state reader implementations
- `Assets/Scripts/Core/Router/` - Concrete router implementations (Phase 3, 4 complete)
- `Assets/Scripts/Core/Events/` - Combat result events (Phase 5)
- `Assets/Tests/EditMode/Core/` - Integration tests (Phase 3, 4 complete)
