# UI Flow Checklist

## Reference Documents

- `docs/plans/2026-03-13-tower-breaker-vertical-slice/00-overview.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/04-ui-flow-and-content.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/milestones/ui-flow/research.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/milestones/ui-flow/references.md`

## Checklist

### Task 1: Title screen presenter/view seam (complete)

- Done: create `Assets/Scripts/UIFlow/TowerBreak.UIFlow.asmdef`
- Done: create `Assets/Tests/EditMode/UIFlow/TowerBreak.UIFlow.Tests.asmdef`
- Done: `ITitleFlowRouter`, `ITitleSaveStateReader` interfaces
- Done: `TitleScreenPresenter` — 8 tests, 8/8 passed
- Done: `TitleScreenView` — 5 tests, 5/5 passed
- Done: full UIFlow verify `uiflow-view-full.xml` — 13/13

### Task 2: Lobby presenter/view seam (complete)

- Done: `ILobbyFlowRouter` interface (OpenChallenge, OpenEquipment, OpenReroll)
- Done: `ILobbyStateReader` interface (CurrentFloorId, Gold, EquippedWeaponId)
- Done: `LobbyPresenter` — 12 tests, 12/12 passed
- Done: `LobbyView` — 5 tests, 5/5 passed
- Done: targeted verify `lobby-green.xml` — 12/12
- Done: full UIFlow verify `lobby-uiflow-full.xml` — 30/30

### Task 3: Battle HUD presenter/view seam (complete)

- Done: `IBattleHudStateReader` interface (PlayerHealth, FloorId, WaveNumber, IsDangerActive, IsWallDefeated)
- Done: `BattleHudPresenter` (UIFlow, `TowerBreak.UIFlow.Battle`) — 12 tests, 12/12 passed
- Done: `BattleHudView` — 6 tests, 6/6 passed
- Done: targeted verify `hud-green.xml` — 12/12
- Done: full UIFlow verify `hud-uiflow-full.xml` — 48/48

### Task 4: Reward results + growth screen presenter/view seam (complete)

- Done: `IRewardResultsStateReader`, `IRewardResultsFlowRouter` interfaces
- Done: `RewardResultsPresenter` — 8 tests, 8/8 passed
- Done: `RewardResultsView` — 4 tests, 4/4 passed
- Done: `IGrowthStateReader`, `IGrowthFlowRouter` interfaces
- Done: `GrowthScreenPresenter` — 14 tests, 14/14 passed
- Done: `GrowthScreenView` — 5 tests, 5/5 passed
- Done: targeted verify `reward-green.xml` — 8/8
- Done: full UIFlow verify `task4-uiflow-full.xml` — 79/79

### Task 5: Placeholder content validation tests (complete)

- Done: add `TowerBreak.GameData` reference to `TowerBreak.UIFlow.Tests.asmdef`
- Done: `VerticalSliceContentValidationTests` — 11 tests, 11/11 passed
- Done: `TowerBreakerGameData.asset` populated — Weapons (Claw + Lance), RewardTables (×3), RewardEntries (×3), RerollCosts (Common + Rare)
- Done: placeholder scenes `Assets/Scenes/Bootstrap.unity`, `Lobby.unity`, `Battle.unity`
- Done: `Assets/Prefabs/UI/` directory created
- Done: targeted verify `content-green.xml` — 11/11
- Done: full UIFlow verify `task5-uiflow-full.xml` — 90/90

## Verification

### Task 1

- 2026-03-14: RED — `uiflow-view-red.log` CS0246 × 4 (`TitleScreenView` type not found)
- 2026-03-14: GREEN — `uiflow-view-full.xml` full UIFlow filter: **13/13 passed**
  - `TitleScreenPresenterTests` 8/8 ✓
  - `TitleScreenViewTests` 5/5 ✓

### Task 2

- 2026-03-14: RED — `lobby-red.log` CS0234/CS0246 × 10 (Lobby namespace missing)
- 2026-03-14: GREEN targeted — `lobby-green.xml` `LobbyPresenterTests` filter: **12/12 passed**
- 2026-03-14: GREEN full UIFlow — `lobby-uiflow-full.xml` **30/30 passed**

### Task 3

- 2026-03-14: RED — `hud-red.log` CS0234/CS0246 × 8 (Battle namespace missing)
- 2026-03-14: GREEN targeted — `hud-green.xml` `BattleHudPresenterTests` filter: **12/12 passed**
- 2026-03-14: GREEN full UIFlow — `hud-uiflow-full.xml` **48/48 passed**
  - `TitleScreenPresenterTests` 8/8 ✓
  - `TitleScreenViewTests` 5/5 ✓
  - `LobbyPresenterTests` 12/12 ✓
  - `LobbyViewTests` 5/5 ✓
  - `BattleHudPresenterTests` 12/12 ✓
  - `BattleHudViewTests` 6/6 ✓
- Project-wide: 216 tests registered, no failures

### Task 4

- 2026-03-14: RED — `reward-red.log` CS0234/CS0246 × 6+ (Results, Growth namespaces missing)
- 2026-03-14: GREEN targeted — `reward-green.xml` `RewardResultsPresenterTests` filter: **8/8 passed**
- 2026-03-14: GREEN full UIFlow — `task4-uiflow-full.xml` **79/79 passed**
  - `TitleScreenPresenterTests` 8/8 ✓ · `TitleScreenViewTests` 5/5 ✓
  - `LobbyPresenterTests` 12/12 ✓ · `LobbyViewTests` 5/5 ✓
  - `BattleHudPresenterTests` 12/12 ✓ · `BattleHudViewTests` 6/6 ✓
  - `RewardResultsPresenterTests` 8/8 ✓ · `RewardResultsViewTests` 4/4 ✓
  - `GrowthScreenPresenterTests` 14/14 ✓ · `GrowthScreenViewTests` 5/5 ✓
- Project-wide: 247 tests registered, no failures

### Task 5

- 2026-03-14: RED — `content-red.xml` 3/11 passed (GameData loads, floors, enemy archetypes pre-existing); 8 failures (weapons empty, RewardTables/Entries/RerollCosts empty, scenes missing, Prefabs/UI missing)
- 2026-03-14: GREEN targeted — `content-green.xml` `VerticalSliceContentValidationTests` filter: **11/11 passed**
- 2026-03-14: GREEN full UIFlow — `task5-uiflow-full.xml` **90/90 passed**
  - `TitleScreenPresenterTests` 8/8 ✓ · `TitleScreenViewTests` 5/5 ✓
  - `LobbyPresenterTests` 12/12 ✓ · `LobbyViewTests` 5/5 ✓
  - `BattleHudPresenterTests` 12/12 ✓ · `BattleHudViewTests` 6/6 ✓
  - `RewardResultsPresenterTests` 8/8 ✓ · `RewardResultsViewTests` 4/4 ✓
  - `GrowthScreenPresenterTests` 14/14 ✓ · `GrowthScreenViewTests` 5/5 ✓
  - `VerticalSliceContentValidationTests` 11/11 ✓
- Project-wide: 258 tests registered, no failures

### Task 1 test list

- `Constructor_NullRouter_ThrowsArgumentNullException` ✓
- `Constructor_NullSaveStateReader_ThrowsArgumentNullException` ✓
- `IsContinueAvailable_WhenHasSaveData_ReturnsTrue` ✓
- `IsContinueAvailable_WhenNoSaveData_ReturnsFalse` ✓
- `OnStartNewGame_CallsRouterStartNewGame` ✓
- `OnStartNewGame_DoesNotCallRouterContinueGame` ✓
- `OnContinueGame_WhenContinueAvailable_CallsRouterContinueGame` ✓
- `OnContinueGame_WhenContinueNotAvailable_DoesNotCallRouter` ✓
- `Refresh_WhenContinueAvailable_EnablesContinue` ✓
- `Refresh_WhenContinueNotAvailable_DisablesContinue` ✓
- `Bind_NullPresenter_ThrowsArgumentNullException` ✓
- `Bind_WhenPresenterHasSaveData_EnablesContinue` ✓
- `Bind_WhenPresenterHasNoSaveData_DisablesContinue` ✓

### Task 2 test list

- `Constructor_NullRouter_ThrowsArgumentNullException` ✓
- `Constructor_NullStateReader_ThrowsArgumentNullException` ✓
- `CurrentFloorId_ReturnsStateReaderValue` ✓
- `Gold_ReturnsStateReaderValue` ✓
- `HasEquippedWeapon_WhenNoWeapon_ReturnsFalse` ✓
- `HasEquippedWeapon_WhenWeaponEquipped_ReturnsTrue` ✓
- `EquippedWeaponId_WhenNoWeapon_ReturnsNull` ✓
- `EquippedWeaponId_WhenWeaponEquipped_ReturnsId` ✓
- `OnOpenChallenge_CallsRouterOpenChallenge` ✓
- `OnOpenChallenge_DoesNotCallOtherRouterMethods` ✓
- `OnOpenEquipment_CallsRouterOpenEquipment` ✓
- `OnOpenReroll_CallsRouterOpenReroll` ✓
- `Refresh_NullPresenter_ThrowsArgumentNullException` ✓
- `Refresh_SetsDisplayedFloorId` ✓
- `Refresh_SetsDisplayedGold` ✓
- `Refresh_WhenNoWeapon_SetsIsWeaponEquippedFalse` ✓
- `Refresh_WhenWeaponEquipped_SetsIsWeaponEquippedTrue` ✓

### Task 5 test list

- `GameData_CanBeLoadedFromResources` ✓
- `FloorRows_ContainsFloors1Through3` ✓
- `EnemyRows_ContainsBothMvpArchetypes` ✓
- `WeaponRows_ContainsBothMvpArchetypes` ✓
- `RewardTableRows_ContainsOnePerMvpFloor` ✓
- `RewardEntryRows_EachTableHasAtLeastOneEntry` ✓
- `RerollCostRows_ContainsCommonRarity` ✓
- `Scene_Bootstrap_FileExists` ✓
- `Scene_Lobby_FileExists` ✓
- `Scene_Battle_FileExists` ✓
- `PrefabDirectory_UI_Exists` ✓

## Manual Follow-Ups

- Pending: connect `TitleScreenPresenter` + `TitleScreenView` to Bootstrap scene and verify play-mode routing
- Pending: connect `LobbyPresenter` + `LobbyView` to Lobby scene and verify play-mode routing
- Pending: connect `BattleHudPresenter` + `BattleHudView` to Battle scene; wire `IBattleHudStateReader` to actual `CombatState`
- Pending: connect `RewardResultsPresenter` + `GrowthScreenPresenter` to post-battle flow; wire readers to actual Meta state
- Pending: add scene-level smoke steps once presenter/view pairs are wired to actual buttons in prefabs
