# Integration Research

## Reference Documents

- `docs/plans/2026-03-13-tower-breaker-vertical-slice/00-overview.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/05-integration-and-verification.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/archive/ui-flow/README.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/archive/combat-loop/README.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/archive/meta-loop/README.md`

## Research Findings

### Current Architecture State

The codebase has three well-defined runtime layers ready for integration:

1. **UIFlow** (`TowerBreak.UIFlow.asmdef`) - Pure presenter/view layer with no domain logic
2. **Combat** (`TowerBreak.Combat.asmdef`) - Pure battle simulation with no Unity dependencies in core rules
3. **Meta** (`TowerBreak.Meta.asmdef`) - Pure progression/inventory services with no Unity dependencies

### Key Interfaces for Integration

#### UIFlow Interfaces (Need Concrete Implementations)

```csharp
// Title
public interface ITitleFlowRouter { void StartNewGame(); void ContinueGame(); }
public interface ITitleSaveStateReader { bool HasSaveData(); }

// Lobby
public interface ILobbyFlowRouter { void OpenChallenge(); void OpenEquipment(); void OpenReroll(); }
public interface ILobbyStateReader { int CurrentFloorId { get; } int Gold { get; } int? EquippedWeaponId { get; } }

// Battle HUD
public interface IBattleHudStateReader { int PlayerHealth { get; } int FloorId { get; } int WaveNumber { get; } bool IsDangerActive { get; } bool IsWallDefeated { get; } }

// Reward Results
public interface IRewardResultsFlowRouter { void OnContinue(); }
public interface IRewardResultsStateReader { IReadOnlyList<int> GrantedWeaponIds { get; } int GrantedGold { get; } }

// Growth
public interface IGrowthFlowRouter { void OnEquip(int weaponInstanceId); void OnReroll(int weaponInstanceId); void OnContinue(); }
public interface IGrowthStateReader { int? EquippedWeaponInstanceId { get; } int? CandidateWeaponInstanceId { get; } bool IsComparisonAvailable { get; } float DeltaAttack { get; } float DeltaAttackSpeed { get; } float DeltaPushPower { get; } }
```

### Missing Integration Layer

The missing piece is a thin **Core** integration layer that:

1. **Implements state readers** - Bridge between Meta state and UIFlow presenters
2. **Implements flow routers** - Handle scene transitions and service calls
3. **Wires everything through DI** - Use existing DI module
4. **Emits/Consumes events via EventBus** - Decouple systems

### Recommended Integration Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                        Scenes                               │
│  Bootstrap (Title)  │  Lobby  │  Battle  │  Post-Battle    │
└──────────┬────────────────────┬──────────┬──────────────────┘
           │                    │          │
┌──────────▼────────────────────▼──────────▼──────────────────┐
│              Flow Controllers (Core)                        │
│  BootstrapFlowController                                    │
│  LobbyFlowController                                        │
│  BattleFlowController                                       │
│  PostBattleFlowController                                   │
└──────────┬────────────────────┬──────────┬──────────────────┘
           │                    │          │
┌──────────▼────────────────────▼──────────▼──────────────────┐
│              Presenters (UIFlow)                            │
│  TitleScreenPresenter  LobbyPresenter  BattleHudPresenter   │
│  RewardResultsPresenter  GrowthScreenPresenter              │
└──────────┬────────────────────┬──────────┬──────────────────┘
           │                    │          │
┌──────────▼────────────────────▼──────────▼──────────────────┐
│              State Readers (Core)                           │
│  Implements: ILobbyStateReader, IBattleHudStateReader, etc. │
└──────────┬────────────────────┬──────────┬──────────────────┘
           │                    │          │
┌──────────▼────────────────────▼──────────▼──────────────────┐
│              Domain Services (Meta/Combat)                  │
│  PlayerInventoryState, PlayerWalletState, CombatState       │
│  BattleRewardService, FloorProgressionService               │
└─────────────────────────────────────────────────────────────┘
```

### Event Bus for Cross-System Communication

Combat and Meta should communicate via typed events:

```csharp
// Combat emits
public struct CombatEndedEvent { public bool IsVictory; public int FloorId; }

// Meta consumes and emits
public struct RewardsGrantedEvent { public RewardBundle Rewards; }
public struct FloorAdvancedEvent { public int NewFloorId; }
```

### Scene Transition Strategy

Keep scene loading minimal and centralized:

```csharp
public interface ISceneLoader { void LoadScene(string sceneName); }

// Flow controllers use this, not presenters
public class SceneLoader : ISceneLoader 
{ 
    public void LoadScene(string sceneName) => SceneManager.LoadScene(sceneName); 
}
```

### Initial Hypothesis

The best first integration task is **NOT** to implement all controllers at once. Instead:

1. **First**: Create the Core asmdef with one end-to-end flow test (title -> lobby)
2. **Second**: Implement `BootstrapFlowController` + concrete `ITitleFlowRouter` + concrete `ILobbyStateReader`
3. **Third**: Verify the test passes
4. **Repeat** for lobby -> battle, battle -> reward, reward -> growth -> lobby

This keeps the integration incremental and testable at each step.

## Task Breakdown

### Task 1: Core Infrastructure

- Create `TowerBreak.Core.asmdef` (references: UIFlow, Meta, Combat, EventBus, DI)
- Create `TowerBreak.Core.Tests.asmdef`
- Create `ISceneLoader` interface
- Create `SceneLoader` implementation

### Task 2: Title -> Lobby Integration

- Create `BootstrapFlowController` (MonoBehaviour for Bootstrap scene)
- Create concrete `TitleFlowRouter` (implements `ITitleFlowRouter`)
- Create concrete `TitleSaveStateReader` (implements `ITitleSaveStateReader`)
- Create `PlayerSessionState` (singleton-like through DI to hold current session)
- Write integration test for title -> lobby flow

### Task 3: Lobby -> Battle Integration (Phase 4 - Implementation Complete, Pending Verification)

Implementation approach: Minimal surface over full controller

**What was implemented:**
- Extended `LobbyFlowRouter` to require `PlayerSessionState` and validate run state before challenge
- Added `PendingBattleFloorId` property to `PlayerSessionState` as battle launch context
- `OpenChallenge()` now sets pending floor ID before loading Battle scene
- Created comprehensive integration tests in `LobbyToBattleIntegrationTests.cs`

**What was intentionally NOT implemented:**
- No `LobbyFlowController` - not needed for current scope (presenter/router/state reader pattern sufficient)
- No new state readers - `LobbyStateReader` already provides floor context
- No scene bootstrap wiring - battle scene will read `PendingBattleFloorId` directly from session state

**Integration seam:**
```csharp
// Lobby side: router validates and sets context
router.OpenChallenge() -> 
    sessionState.SetPendingBattleFloorId(sessionState.CurrentFloorId)
    sceneLoader.LoadScene("Battle")

// Battle side (future): reads context
int? floorId = sessionState.PendingBattleFloorId;
sessionState.ClearPendingBattleFloorId(); // consume
```

**Test coverage:** 12 tests written (pending Unity verification)
- Router validation (run active/inactive)
- Pending floor ID storage/clearing
- End-to-end presenter -> router -> session state flow
- Null dependency guards
- Invalid floor ID handling

**Files changed:**
- `Assets/Scripts/Core/PlayerSessionState.cs` - Added `PendingBattleFloorId` property and methods
- `Assets/Scripts/Core/Router/LobbyFlowRouter.cs` - Added `PlayerSessionState` dependency and validation
- `Assets/Tests/EditMode/Core/LobbyToBattleIntegrationTests.cs` - New test fixture (12 tests)

### Task 4: Battle -> Reward Integration (Phase 5 - Implementation Complete, Pending Verification)

Implementation approach: EventBus-based notification with bridge service

**What was implemented:**
- Created `CombatEndedEvent` struct for EventBus communication (IsVictory, FloorId)
- Created `CombatResultBridge` service that subscribes to combat end events
- Bridge resolves rewards via existing `RewardResolver` and stores in session state
- Created concrete `RewardResultsStateReader` implementing `IRewardResultsStateReader`
- Extended `PlayerSessionState` with `LastBattleReward` storage
- Created `IGameDataProvider` interface for data access abstraction in Core

**What was intentionally NOT implemented:**
- No `BattleFlowController` - combat scene entry/exit management deferred to future phase
- No `PostBattleFlowController` - belongs to Phase 6 (Reward -> Growth integration)
- No direct presenter-reward coupling - all access through state reader pattern

**Integration flow:**
```csharp
// Combat side emits
combatEndedBus.Publish(new CombatEndedEvent(isVictory: true, floorId: 1));

// Bridge receives and processes
bridge.OnCombatEnded(event) ->
    if (event.IsVictory)
        reward = RewardResolver.Resolve(floor, tables, entries, random)
        sessionState.SetLastBattleReward(reward)
    else
        sessionState.ClearLastBattleReward()

// Presenter reads via state reader
reader.GoldEarned -> sessionState.LastBattleReward?.GoldAmount ?? 0
reader.GrantedWeaponIds -> sessionState.LastBattleReward?.GrantedWeaponIds ?? empty
```

**Test coverage:** 12 tests written (pending Unity verification)
- EventBus publish/subscribe functionality
- Bridge null dependency guards
- Victory reward resolution and storage
- Defeat null reward handling
- State reader gold/weapon access
- End-to-end bridge flow verification

**Files changed:**
- `Assets/Scripts/Core/Events/CombatEndedEvent.cs` - New event struct
- `Assets/Scripts/Core/Bridge/CombatResultBridge.cs` - New bridge service
- `Assets/Scripts/Core/State/RewardResultsStateReader.cs` - New state reader
- `Assets/Scripts/Core/PlayerSessionState.cs` - Added `LastBattleReward` property and methods
- `Assets/Scripts/Core/IGameDataProvider.cs` - New interface for data access
- `Assets/Tests/EditMode/Core/BattleToRewardIntegrationTests.cs` - New test fixture (12 tests)

### Task 5: Reward -> Growth -> Lobby Integration (Phase 6 - Implementation Complete, Verified)

Implementation approach: Minimal concrete implementations wiring Meta services through Core state readers and routers.

**What was implemented:**
- `RewardResultsFlowRouter` - implements `IRewardResultsFlowRouter`, `Continue()` loads "Growth" scene
- `GrowthStateReader` - implements `IGrowthStateReader`:
  - `EquippedWeaponId` - weapon template ID (WeaponId) of the equipped weapon from inventory
  - `CandidateWeaponId` - first weapon template ID from `LastBattleReward.GrantedWeaponIds`
  - Delta stats - computed via `EquipmentComparisonService` using weapon rows from game data
- `GrowthFlowRouter` - implements `IGrowthFlowRouter`:
  - `Equip()` - creates `OwnedEquipment` from candidate template ID, adds to inventory and equips
  - `Reroll()` - delegates to `EquipmentRerollService` on equipped weapon (deducts gold)
  - `Continue()` - calls `FloorProgressionService.Advance()` to determine victory/defeat path, then loads "Lobby"

**What was intentionally NOT implemented:**
- No `PostBattleFlowController` - presenter/router/state reader pattern sufficient
- No `PlayerSessionState` changes - existing `LastBattleReward` sufficient for all reads
- No persistence of reroll result - growth flow exercises service, result not stored

**Integration seam:**
```csharp
// Reward side: continue triggers growth path
router.Continue() -> sceneLoader.LoadScene("Growth")

// Growth side: reads from session state + game data
stateReader.EquippedWeaponId -> inventory.EquippedWeaponInstanceId -> OwnedEquipment.WeaponId
stateReader.CandidateWeaponId -> sessionState.LastBattleReward.GrantedWeaponIds[0]
stateReader.DeltaAttack -> EquipmentComparisonService.Compare(equipped, candidate).DeltaAttack

// Growth actions
router.Equip() -> OwnedEquipment(newInstanceId, candidateWeaponId) -> inventory.Add + EquipWeapon
router.Reroll() -> EquipmentRerollService.Reroll(equipped, weaponRow, costRows, wallet, random)
router.Continue() -> FloorProgressionService.Advance(currentFloor, outcome, floors) ->
    CanContinue ? sessionState.AdvanceToNextFloor() : sessionState.EndRun()
    sceneLoader.LoadScene("Lobby")
```

**Test coverage:** 37 tests written and verified
- `RewardToGrowthIntegrationTests.cs` (15 tests): router/state reader null guards + behavior
- `GrowthToLobbyIntegrationTests.cs` (22 tests): flow router null guards + equip/reroll/continue + end-to-end presenter coverage

**Files added:**
- `Assets/Scripts/Core/Router/RewardResultsFlowRouter.cs`
- `Assets/Scripts/Core/State/GrowthStateReader.cs`
- `Assets/Scripts/Core/Router/GrowthFlowRouter.cs`
- `Assets/Tests/EditMode/Core/RewardToGrowthIntegrationTests.cs`
- `Assets/Tests/EditMode/Core/GrowthToLobbyIntegrationTests.cs`

### Task 6: Full Loop Integration Test (Phase 7 - Implementation Complete, Verified)

Implementation approach: Single test fixture assembling all existing routers/state readers/bridge — zero new production code.

**What was implemented:**
- `FullLoopIntegrationTests.cs` (11 tests) - `Assets/Tests/EditMode/Core/FullLoopIntegrationTests.cs`

**Assembly pattern:**
```csharp
// All pieces wired from one shared PlayerSessionState
bridge = new CombatResultBridge(sessionState, gameDataProvider, combatEndedBus);
bridge.Initialize();
titleRouter   = new TitleFlowRouter(sceneLoader, sessionState);
lobbyRouter   = new LobbyFlowRouter(sceneLoader, sessionState);
rewardRouter  = new RewardResultsFlowRouter(sceneLoader);
growthRouter  = new GrowthFlowRouter(sessionState, gameDataProvider, sceneLoader, () => nextInstanceId++, new Random(0));
// State readers all read from the same sessionState
```

**Test scenarios covered:**
- Victory loop: full path (floor advances, scene sequence correct)
- Victory loop: scene sequence Lobby → Battle → Growth → Lobby
- Victory loop: two consecutive floors (1→2→3)
- Defeat loop: full path (run ends, no reward state)
- Defeat loop: state readers return zero/null after defeat
- State continuity: gold earned accessible via reward reader
- State continuity: candidate weapon available after weapon drop
- State continuity: equip propagates to lobby state reader
- State continuity: equipped weapon persists after floor advance
- Presenter assembly: all presenters constructable from same session state
- Null guard: null sessionState throws at TitleFlowRouter construction

**Key design decision:** `WeaponDropChance = 1.0f` in test data ensures weapon drops are deterministic without controlling the `Random` inside `CombatResultBridge`.

**Files added:**
- `Assets/Tests/EditMode/Core/FullLoopIntegrationTests.cs`

### Task 7: Verification

- Run all existing tests (UIFlow 90, Combat 58, Meta 84)
- Run full Core suite (90/90 expected)
- Document manual smoke test steps
- Update milestone docs
