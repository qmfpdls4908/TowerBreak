# Integration Milestone

## Goal

Wire the completed UI flow, combat, and meta systems into a minimal end-to-end playable vertical slice: title -> lobby -> battle -> reward -> growth -> next floor or lobby return.

## Reference Documents

- `docs/plans/2026-03-13-tower-breaker-vertical-slice/00-overview.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/05-integration-and-verification.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/archive/ui-flow/README.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/archive/combat-loop/README.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/archive/meta-loop/README.md`

## Code And Tests To Research First

- `Assets/Scripts/UIFlow/` - Presenters and views for title, lobby, battle HUD, reward, growth
- `Assets/Scripts/Combat/` - Combat state, battle loop controller, spawn services
- `Assets/Scripts/Meta/` - Inventory, wallet, reward resolver, equipment, progression services
- `Assets/Scenes/Bootstrap.unity` - Title scene
- `Assets/Scenes/Lobby.unity` - Lobby hub scene
- `Assets/Scenes/Battle.unity` - Combat scene
- `Assets/Scripts/DI/` - DI container and installers
- `Assets/Scripts/EventBus/` - Cross-system event communication

## Research Findings

### Completed Systems (Ready to Integrate)

1. **UIFlow** (90/90 tests passing)
   - `TitleScreenPresenter` + `TitleScreenView` - Title screen with start/continue
   - `LobbyPresenter` + `LobbyView` - Lobby hub with floor/gold/weapon display
   - `BattleHudPresenter` + `BattleHudView` - Battle HUD with health/danger
   - `RewardResultsPresenter` + `RewardResultsView` - Post-battle reward display
   - `GrowthScreenPresenter` + `GrowthScreenView` - Equipment/growth management

2. **Combat** (58/58 tests passing)
   - `CombatState` - Pure battle state (player health, wall health, enemies)
   - `BattleLoopController` - Pressure ticking, attack/guard handling
   - `WaveSpawnPlanner` - Data-driven enemy spawn from floor/wave rows
   - `PooledCombatInstantiator` - Object pooling for enemy instances
   - `CombatSampleSceneBootstrap` - Debug bootstrap for SampleScene

3. **Meta** (84/84 tests passing)
   - `PlayerInventoryState` + `PlayerWalletState` - Player progression state
   - `BattleRewardService` + `RewardResolver` - Reward resolution from combat
   - `EquipmentComparisonService` - Compare equipped vs candidate weapons
   - `EquipmentRerollService` - Stat rerolling with gold cost
   - `FloorProgressionService` - Advance to next floor or end run

### Integration Gaps Identified

1. **Scene Flow Controllers** - Missing thin coordinators to bridge presenters with scene transitions
2. **State Readers** - Presenters need concrete implementations of reader interfaces
3. **Flow Routers** - Presenters need concrete implementations of router interfaces
4. **Scene Bootstrap** - Bootstrap scenes need DI wiring and controller setup
5. **Combat Result Bridge** - Combat outcome needs to trigger reward resolution

## Implementation Guardrails

- Keep scene controllers thin - only handle scene loading and service coordination
- Keep presenter/domain/service boundaries intact - no rewrites
- Use existing DI module for registration - no second service locator
- Use existing EventBus for cross-system events - no direct coupling
- Prefer EditMode tests for integration verification
- Keep scene glue minimal - "just enough" to connect the loop

## Checklist

### Phase 1: Research and Documentation

- [x] Research existing presenter/service seams
- [x] Create integration milestone docs (README, research, checklist, references)
- [x] Update 00-overview.md with integration as active milestone

### Phase 2: Core Infrastructure

- [x] Create `TowerBreak.Core.asmdef` (references: UIFlow, Meta, Combat, EventBus, DI)
- [x] Create `TowerBreak.Core.Tests.asmdef`
- [x] Create `ISceneLoader` interface
- [x] Create `SceneLoader` implementation
- [x] Create `CoroutineRunner` for async scene loading
- [x] Create `PlayerSessionState` singleton session state

### Phase 3: Title -> Lobby Integration

- [x] Write failing test for title -> lobby flow
- [x] Create concrete `TitleFlowRouter`
- [x] Create concrete `TitleSaveStateReader`
- [x] Create concrete `LobbyStateReader`
- [x] Create concrete `LobbyFlowRouter`
- [x] Verify test passes (15/15 green)

### Phase 4: Lobby -> Battle Integration

- [x] Write failing test for lobby -> battle flow
- [x] Update `LobbyFlowRouter` to require `PlayerSessionState` and validate run state
- [x] Add `PendingBattleFloorId` to `PlayerSessionState` for battle launch context
- [x] Wire floor selection to battle initialization
- [x] Verify test passes (12/12 green)

### Phase 5: Battle -> Reward Integration

- [x] Write failing test for battle -> reward flow
- [x] Create `CombatEndedEvent` for EventBus
- [x] Create `CombatResultBridge` service
- [x] Create concrete `RewardResultsStateReader`
- [ ] Create `BattleFlowController` (battle scene) - deferred, minimal bridge pattern sufficient
- [x] Verify test passes (15/15 green - `Logs/battle-to-reward-fix.xml`)

### Phase 6: Reward -> Growth -> Lobby Integration

- [x] Write failing tests for reward -> growth and growth -> lobby flows
- [x] Create concrete `RewardResultsFlowRouter`
- [x] Create concrete `GrowthStateReader`
- [x] Create concrete `GrowthFlowRouter`
- [x] Wire equipment comparison (`EquipmentComparisonService`) in `GrowthStateReader`
- [x] Wire reroll service (`EquipmentRerollService`) in `GrowthFlowRouter`
- [x] Wire floor progression service (`FloorProgressionService`) in `GrowthFlowRouter`
- [x] Verify tests pass (37/37 green - `Logs/phase6-targeted.xml`)

### Phase 7: Full Loop Integration Test

- [x] Write failing end-to-end test: title -> lobby -> battle -> reward -> growth -> lobby
- [x] Assemble all existing routers/state readers/bridge in a single fixture (no new production code)
- [x] Verify full loop test passes (11/11 green - `Logs/phase7-targeted.xml`)

### Phase 8: Verification

- [x] Run all UIFlow tests — **90/90 passed** (`Logs/integration-uiflow-results.xml`)
- [x] Run all Combat tests — **59/59 passed** (`Logs/integration-combat-results.xml`)
- [x] Run all Meta tests — **84/84 passed** (`Logs/integration-meta-results.xml`)
- [x] Run all Core/Integration tests — **90/90 passed** (`Logs/integration-core-results.xml`)
- [x] `dotnet build TowerBreak.sln` — environment limitation: `dotnet` not on PATH; Unity batchmode confirms compile success
- [x] Document manual smoke test steps — see Manual Smoke Procedure section below

## Verification

### Phase 4-5 Implementation Complete and Verified

| Component | Status | Notes |
|-----------|--------|-------|
| `PlayerSessionState.PendingBattleFloorId` | ✓ Verified | With Set/Clear methods |
| `LobbyFlowRouter` with `PlayerSessionState` | ✓ Verified | Run validation + pending floor set |
| `LobbyToBattleIntegrationTests` | ✓ 12/12 | All scenarios covered |
| `CombatEndedEvent` | ✓ Verified | EventBus payload for combat end notification |
| `CombatResultBridge` | ✓ Verified | Event subscriber + reward resolution |
| `RewardResultsStateReader` | ✓ Verified | `IRewardResultsStateReader` concrete implementation |
| `PlayerSessionState.LastBattleReward` | ✓ Verified | Reward storage for presenter consumption |
| `IGameDataProvider` | ✓ Verified | Interface for game data access in Core |
| `BattleToRewardIntegrationTests` | ✓ 15/15 | Event/bridge/reader flow covered |

### Phase 6 Implementation Complete and Verified

| Component | Status | Notes |
|-----------|--------|-------|
| `RewardResultsFlowRouter` | ✓ Verified | `Continue()` loads "Growth" scene |
| `GrowthStateReader` | ✓ Verified | Equipped/candidate weapon IDs + delta stats |
| `GrowthFlowRouter` | ✓ Verified | Equip/Reroll/Continue wired to Meta services |
| `RewardToGrowthIntegrationTests` | ✓ 15/15 | Router and state reader coverage |
| `GrowthToLobbyIntegrationTests` | ✓ 22/22 | Flow router + end-to-end coverage |
| **Phase 6 Total** | **37/37** | `Logs/phase6-targeted.xml` |

### Phase 7 Implementation Complete and Verified

| Component | Status | Notes |
|-----------|--------|-------|
| `FullLoopIntegrationTests` | ✓ 11/11 | No new production code needed |
| Victory loop (full path) | ✓ | floor advances, scene sequence correct |
| Defeat loop (full path) | ✓ | run ends, no reward state |
| State continuity | ✓ | gold, candidate weapon, equip persistence |
| Multi-floor progression | ✓ | two loops advances floor 1→3 |
| Presenter assembly | ✓ | all presenters wire from same session state |
| **Phase 7 Total** | **11/11** | `Logs/phase7-targeted.xml` |

### Current Verification Status

| Test Suite | Count | Status |
|------------|-------|--------|
| TitleToLobbyIntegrationTests | 15 | ✓ Passed |
| LobbyToBattleIntegrationTests | 12 | ✓ Passed |
| BattleToRewardIntegrationTests | 15 | ✓ Passed |
| RewardToGrowthIntegrationTests | 15 | ✓ Passed |
| GrowthToLobbyIntegrationTests | 22 | ✓ Passed |
| FullLoopIntegrationTests | 11 | ✓ Passed |
| **Core/Integration Total** | **90** | ✓ Passing |

### Phase 8 Final Verification

| Suite | Count | Result | Log |
|-------|-------|--------|-----|
| UIFlow | 90 | ✓ Passed | `Logs/integration-uiflow-results.xml` |
| Combat | 59 | ✓ Passed | `Logs/integration-combat-results.xml` |
| Meta | 84 | ✓ Passed | `Logs/integration-meta-results.xml` |
| Core | 90 | ✓ Passed | `Logs/integration-core-results.xml` |
| **Grand Total** | **323** | **✓ All Green** | Phase 8 complete |

**Build note**: `dotnet build TowerBreak.sln` could not be executed — `dotnet` is not on PATH in this environment. Unity 6000.3.11f1 batchmode import confirms all assemblies compile without errors before each test run. Run `dotnet build TowerBreak.sln` manually when .NET SDK is available.

## Manual Smoke Procedure

Steps to validate the integration manually in the Unity Editor. Open the project in Unity 6000.3.11f1, then follow each step in order.

### Prerequisites

- Unity Editor open, project loaded at `D:\Fork\TowerBreak`
- No existing PlayMode running
- Open `Assets/Scenes/Bootstrap.unity` as the start scene

### Step 1 — Title Screen: Start New Game

| | |
|---|---|
| **Action** | Enter PlayMode. The Title screen should appear. Click **Start New Game**. |
| **Expected** | `Lobby` scene loads. Floor indicator shows **Floor 1**. Gold shows **0**. No weapon equipped. |
| **Checks** | `TitleFlowRouter.StartNewGame()` called → `PlayerSessionState.StartNewRun()` → `IsRunActive=true, CurrentFloorId=1` |

### Step 2 — Lobby: Challenge Entry

| | |
|---|---|
| **Action** | On the Lobby screen, click **Challenge** (or equivalent battle entry button). |
| **Expected** | `Battle` scene loads. `PendingBattleFloorId` is set to `1` before scene transition. |
| **Checks** | `LobbyFlowRouter.OpenChallenge()` validates `IsRunActive=true` before loading. |

### Step 3 — Battle: Combat Flow

| | |
|---|---|
| **Action** | Play through the battle (or use debug skip). End with a **Clear** (victory). |
| **Expected** | `CombatEndedEvent(isVictory=true, floorId=1)` published. `CombatResultBridge` resolves and stores `LastBattleReward`. |
| **Checks** | Reward bundle contains `GoldAmount ≥ 0` and may contain `GrantedWeaponIds`. |

### Step 4 — Reward Screen: View Results

| | |
|---|---|
| **Action** | Reward results screen appears automatically. Verify gold earned is displayed. Click **Continue**. |
| **Expected** | `RewardResultsPresenter` reads `GoldEarned` and `GrantedWeaponIds` from `RewardResultsStateReader`. `Growth` scene loads. |
| **Checks** | `RewardResultsFlowRouter.Continue()` → `LoadScene("Growth")`. |

### Step 5 — Growth Screen: Equip and Continue

| | |
|---|---|
| **Action** | If a candidate weapon is shown (reward included a weapon drop), click **Equip**. Then click **Continue**. |
| **Expected** | Equip: `GrowthFlowRouter.Equip()` adds candidate weapon to `PlayerInventoryState` and equips it. Continue: `FloorProgressionService.Advance(1, Clear, floors)` → `CanContinue=true` → `CurrentFloorId` becomes `2`. `Lobby` scene loads. |
| **Checks** | `GrowthStateReader` reflects equipped weapon. `LobbyStateReader.EquippedWeaponId` is non-null after equip. |

### Step 6 — Lobby: Floor Progression Confirmed

| | |
|---|---|
| **Action** | Lobby reloads. Verify floor indicator and any equipped weapon display. |
| **Expected** | Floor shows **Floor 2**. Equipped weapon (if equipped in Step 5) shows in lobby state. Run is still active. |
| **Checks** | `PlayerSessionState.CurrentFloorId == 2`, `IsRunActive == true`. |

### Step 7 — Defeat Path (Optional)

| | |
|---|---|
| **Action** | Start a battle and lose (or simulate defeat). On the Growth screen, click **Continue** without equipping. |
| **Expected** | `CombatEndedEvent(isVictory=false)` → `LastBattleReward=null`. Growth continue: `FloorProgressionService.Advance(floor, Fail, floors)` → `EndRun()`. `IsRunActive=false`. Lobby loads with no floor progression. |

### Step 8 — Reroll (Optional)

| | |
|---|---|
| **Action** | With a weapon equipped, on the Growth screen, click **Reroll**. Verify wallet gold is deducted. |
| **Expected** | `GrowthFlowRouter.Reroll()` → `EquipmentRerollService.Reroll()` → `PlayerWalletState.Gold` decreases by reroll cost. |
| **Note** | Requires sufficient gold (≥ reroll cost for weapon rarity) in wallet. |

## Manual Follow-Ups

- [ ] Manual smoke: steps 1–6 above (happy path end-to-end)
- [ ] Manual smoke: step 7 (defeat path)
- [ ] Manual smoke: step 8 (reroll with equipped weapon)
- [ ] Verify `dotnet build TowerBreak.sln` when .NET SDK is available on PATH
