# Integration Checklist

## Reference Documents

- `docs/plans/2026-03-13-tower-breaker-vertical-slice/00-overview.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/05-integration-and-verification.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/milestones/integration/research.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/milestones/integration/references.md`

## Checklist

### Phase 1: Research and Documentation

- [x] Research existing presenter/service seams
- [x] Create integration milestone docs (README, research, checklist, references)
- [x] Update 00-overview.md with integration as active milestone

### Phase 2: Core Infrastructure

- [x] Create `TowerBreak.Core.asmdef`
- [x] Create `TowerBreak.Core.Tests.asmdef`
- [x] Create `ISceneLoader` interface
- [x] Create `SceneLoader` implementation
- [x] Create `CoroutineRunner` (for async scene loading)
- [x] Create `PlayerSessionState` (singleton session state)
- [x] Add `TryGetEquipment` method to `PlayerInventoryState` (required for LobbyStateReader)



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
- [x] Run all Combat tests — **59/59 passed** (`Logs/integration-combat-results.xml`) *(was 58; one test added during milestone)*
- [x] Run all Meta tests — **84/84 passed** (`Logs/integration-meta-results.xml`)
- [x] Run all Core/Integration tests — **90/90 passed** (`Logs/integration-core-results.xml`)
- [x] `dotnet build TowerBreak.sln` — **skipped**: `dotnet` not on PATH in this environment; Unity batchmode confirmed all assemblies compile cleanly
- [x] Document manual smoke test steps (see Manual Smoke Procedure section in README)

## Verification

### Final Verification (Phase 8)

| Suite | Result | Log |
|-------|--------|-----|
| UIFlow | **90/90 ✓** | `Logs/integration-uiflow-results.xml` |
| Combat | **59/59 ✓** | `Logs/integration-combat-results.xml` |
| Meta | **84/84 ✓** | `Logs/integration-meta-results.xml` |
| Core | **90/90 ✓** | `Logs/integration-core-results.xml` |
| **Total** | **323/323 ✓** | All suites green |

### Build Verification

- [x] `dotnet build TowerBreak.sln` — **environment limitation**: `dotnet` not on PATH; Unity batchmode import confirms all 4 assemblies compile without errors
- [x] Unity batchmode tests: Core/Integration - **42/42 passed** (`Logs/core-phase5-fix.xml`)
- [x] Unity batchmode tests: Phase 6 targeted - **37/37 passed** (`Logs/phase6-targeted.xml`)
- [x] Unity batchmode tests: Full Core suite after Phase 6 - **79/79 passed** (`Logs/phase6-core-full.xml`)
- [x] Unity batchmode tests: Phase 7 targeted - **11/11 passed** (`Logs/phase7-targeted.xml`)
- [x] Unity batchmode tests: Full Core suite after Phase 7 - **90/90 passed** (`Logs/phase7-core-full.xml`)
- [x] Unity batchmode tests: Phase 8 fresh run — all suites **323/323 passed**

## Manual Follow-Ups

- [ ] Manual smoke: title -> start new game -> lobby
- [ ] Manual smoke: lobby -> challenge -> battle scene
- [ ] Manual smoke: battle -> clear/fail -> reward screen
- [ ] Manual smoke: reward -> growth -> equip/reroll -> lobby
- [ ] Manual smoke: lobby -> next floor progression
- [x] Document manual smoke test steps (see README Manual Smoke Procedure section)

## Blockers

None currently identified. All prerequisite milestones are complete and archived.

## Status

**COMPLETE**: All 8 phases verified. Milestone ready for archive.

**Final State**:
- Phase 1-7: All complete and verified
- Phase 8: Final verification passed
  - UIFlow: 90/90 ✓
  - Combat: 59/59 ✓
  - Meta: 84/84 ✓
  - Core: 90/90 ✓
  - Total: 323/323 tests passing
  - Manual smoke steps documented
  - `dotnet build` environment limitation noted (not on PATH)

**Archive**: Move `milestones/integration/` → `archive/integration/` after confirming no open blockers.

**Note**: Phase 5 minimal implementation - EventBus-based combat end notification, bridge service resolves rewards via existing `RewardResolver`, results stored in session state for presenter consumption. No `BattleFlowController` needed for current scope.
