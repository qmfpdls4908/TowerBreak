# Meta Loop Checklist

## Reference Documents

- `docs/plans/2026-03-13-tower-breaker-vertical-slice/00-overview.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/03-meta-progression.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/milestones/meta-loop/research.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/milestones/meta-loop/references.md`

## Checklist

- Done: inspect reward, weapon, and cost rows plus current runtime/test asmdef boundaries
- Done: create `Assets/Scripts/Meta/TowerBreak.Meta.asmdef` (references `TowerBreak.GameData`, `noEngineReferences: true`)
- Done: create `Assets/Tests/EditMode/Meta/TowerBreak.Meta.Tests.asmdef` (Editor-only, `TestAssemblies`)
- Done: write failing tests for `PlayerWalletState` (5 tests), `PlayerInventoryState` + `OwnedEquipment` (7 tests) — red confirmed via compile error
- Done: implement `OwnedEquipment` (instanceId + weaponId, validation at construction)
- Done: implement `PlayerWalletState` (gold, AddGold, TryDeductGold with balance guard)
- Done: implement `PlayerInventoryState` (Equipment list, EquippedWeaponInstanceId, AddEquipment with duplicate rejection, EquipWeapon with ownership check)
- Done: verify 12/12 `TowerBreak.Meta.Tests` passing
- Done: add failing tests for `RewardBundle`, `RewardResolver`, `BattleRewardService` (15 tests) — red confirmed via compile error (`CS0234` missing `TowerBreak.GameData` reference in test asmdef, fixed before green)
- Done: add `TowerBreak.GameData` reference to `TowerBreak.Meta.Tests.asmdef` so reward tests can reference `FloorRow`, `RewardTableRow`, `RewardEntryRow`, `RewardType`
- Done: implement `RewardBundle` (immutable: gold + weapon IDs)
- Done: implement `RewardResolver` (pure static: finds table by `floor.RewardTableId`, rolls drop chance, weighted weapon entry selection, fallback entry apply, guaranteed gold always included)
- Done: implement `BattleRewardService` (static apply: adds gold to wallet, adds each weapon as `OwnedEquipment` with caller-supplied `nextInstanceId`)
- Done: verify 15/15 `RewardResolverTests` passing (targeted)
- Done: verify 27/27 `TowerBreak.Meta.Tests` passing (full suite — 12 Task 1 + 15 Task 2)
- Done: add 5 failing tests for invalid reward data validation (fallback entry not found, weapon drop success but no entries, malformed gold entries, malformed weapon entries) — red confirmed
- Done: implement validation in `RewardResolver` to throw `InvalidOperationException` for invalid reward data instead of silent skip
- Done: verify 32/32 `TowerBreak.Meta.Tests` passing (full suite — 12 Task 1 + 20 Task 2 with validation)
- Done: add failing tests for equipment comparison (`EquipmentComparisonService`, `EquipmentStatBlock`, `EquipmentComparisonResult`) — 17 tests, red confirmed via missing types
- Done: implement `EquipmentStatBlock` (readonly struct: attack, attackSpeed, pushPower; `FromWeaponRow` factory)
- Done: implement `EquipmentComparisonResult` (immutable: current + candidate blocks, precomputed deltas)
- Done: implement `EquipmentComparisonService` (static: `GetEquippedStatBlock`, `GetCandidateStatBlock`, `Compare` — all with null/not-found guards)
- Done: fix float precision in `FullComparisonFlow_EquippedVsCandidate` test (use `Within(0.0001f)` for 1.2f-1.0f delta)
- Done: create `EquipmentComparisonService.cs.meta` (missing Unity asset GUID)
- Done: verify 17/17 `EquipmentComparisonServiceTests` passing (targeted)
- Done: verify 49/49 `TowerBreak.Meta.Tests` passing (full suite — 12 Task 1 + 20 Task 2 + 17 Task 3)
- Done: add failing tests for reroll (`EquipmentRerollService`, `RerollCostPolicy`, `RolledStatValue`, `RerollResult`) — 19 tests, red confirmed via `CS0103`/`CS0246` compile errors
- Done: implement `RolledStatValue` (readonly struct, single int bonus value)
- Done: implement `RerollResult` (immutable: InstanceId + WeaponId + `IReadOnlyList<RolledStatValue>`, null guard on stats)
- Done: implement `RerollCostPolicy` (static: `FindRow` by rarity, null guard + not-found throw)
- Done: implement `EquipmentRerollService` (static: null guards → `ValidateCostRow` → `TryDeductGold` → roll via `Random.Next` → `RerollResult`)
- Done: verify 19/19 `EquipmentRerollServiceTests` passing (targeted)
- Done: verify 68/68 `TowerBreak.Meta.Tests` passing (full suite — 12 Task 1 + 20 Task 2 + 17 Task 3 + 19 Task 4)
- Done: add failing tests for floor progression (16 tests — null/empty/unknown guards, clear middle, clear last, fail any, single floor, determinism) — red confirmed via `CS0234`/`CS0103`/`CS0246`
- Done: implement `BattleOutcome` enum (Clear, Fail)
- Done: implement `FloorProgressionResult` (immutable: CurrentFloorId, NextFloorId?, IsRunComplete, CanContinue)
- Done: implement `FloorProgressionService.Advance` (static: null/empty guard → find index → fail→no-continue, clear-last→run-complete, clear-mid→next floor)
- Done: verify 16/16 `FloorProgressionServiceTests` passing (targeted)
- Done: verify 84/84 `TowerBreak.Meta.Tests` passing (full suite — 12 Task 1 + 20 Task 2 + 17 Task 3 + 19 Task 4 + 16 Task 5)

## Verification

- Expected red phase: `Logs\meta-state-red.log` showed `Scripts have compiler errors` — `PlayerInventoryState`, `PlayerWalletState`, `OwnedEquipment` were missing (TDD red)
- Passed command: `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "D:\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.Meta.Tests" -logFile "D:\Fork\TowerBreak\Logs\meta-state-green.log" -testResults "D:\Fork\TowerBreak\Logs\meta-state-green.xml"`
- Evidence: `Logs\meta-state-green.xml` shows `total="12" passed="12" failed="0"`
- Expected red phase: `Logs\meta-reward-red.log` showed `Scripts have compiler errors` — `RewardBundle`, `RewardResolver`, `BattleRewardService` were missing (TDD red)
- Passed command: `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "D:\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.Meta.Tests.RewardResolverTests" -logFile "D:\Fork\TowerBreak\Logs\meta-reward-green.log" -testResults "D:\Fork\TowerBreak\Logs\meta-reward-green.xml"`
- Evidence: `Logs\meta-reward-green.xml` shows `total="15" passed="15" failed="0"`
- Passed command: `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "D:\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.Meta.Tests" -logFile "D:\Fork\TowerBreak\Logs\meta-reward-full.log" -testResults "D:\Fork\TowerBreak\Logs\meta-reward-full.xml"`
- Evidence: `Logs\meta-reward-full.xml` shows `total="27" passed="27" failed="0"`
- Expected red phase: `Logs\meta-reward-validation-red.log` showed 5 failed tests — fallback entry not found, weapon drop with no entries, malformed gold/weapon entries were silently skipped instead of throwing `InvalidOperationException`
- Passed command: `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "D:\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.Meta.Tests.RewardResolverTests" -logFile "D:\Fork\TowerBreak\Logs\meta-reward-validation-green.log" -testResults "D:\Fork\TowerBreak\Logs\meta-reward-validation-green.xml"`
- Evidence: `Logs\meta-reward-validation-green.xml` shows `total="20" passed="20" failed="0"`
- Passed command: `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "D:\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.Meta.Tests" -logFile "D:\Fork\TowerBreak\Logs\meta-full-final.log" -testResults "D:\Fork\TowerBreak\Logs\meta-full-final.xml"`
- Evidence: `Logs\meta-full-final.xml` shows `total="32" passed="32" failed="0"`
- Evidence: `Logs\meta-comparison-green2.xml` shows `total="17" passed="17" failed="0"` (Task 3 targeted)
- Evidence: `Logs\meta-comparison-full.xml` shows `total="49" passed="49" failed="0"` (Task 3 full)
- Expected red phase: `Logs\meta-reroll-red.log` showed `CS0103`/`CS0246` — `RerollCostPolicy`, `EquipmentRerollService`, `RerollResult` were missing (TDD red)
- Passed command (Task 4 targeted): `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "D:\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.Meta.Tests.EquipmentRerollServiceTests" -logFile "D:\Fork\TowerBreak\Logs\meta-reroll-green.log" -testResults "D:\Fork\TowerBreak\Logs\meta-reroll-green.xml"`
- Evidence: `Logs\meta-reroll-green.xml` shows `total="19" passed="19" failed="0"`
- Passed command (Task 4 full): `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "D:\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.Meta.Tests" -logFile "D:\Fork\TowerBreak\Logs\meta-reroll-full.log" -testResults "D:\Fork\TowerBreak\Logs\meta-reroll-full.xml"`
- Evidence: `Logs\meta-reroll-full.xml` shows `total="68" passed="68" failed="0"`

- Expected red phase (Task 5): `Logs\meta-floor-red.log` showed `CS0234`/`CS0103`/`CS0246` — `FloorProgressionService`, `FloorProgressionResult`, `BattleOutcome` were missing (TDD red)
- Passed command (Task 5 targeted): `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "D:\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.Meta.Tests.FloorProgressionServiceTests" -logFile "D:\Fork\TowerBreak\Logs\meta-floor-green.log" -testResults "D:\Fork\TowerBreak\Logs\meta-floor-green.xml"`
- Evidence: `Logs\meta-floor-green.xml` shows `total="16" passed="16" failed="0"`
- Passed command (Task 5 full): `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "D:\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.Meta.Tests" -logFile "D:\Fork\TowerBreak\Logs\meta-floor-full.log" -testResults "D:\Fork\TowerBreak\Logs\meta-floor-full.xml"`
- Evidence: `Logs\meta-floor-full.xml` shows `total="84" passed="84" failed="0"`

## Manual Follow-Ups

- Done (Task 3): equipment comparison loop is pure-service verified
- Done (Task 4): reroll loop is pure-service verified
- Done (Task 5): floor progression surface is pure-service verified — all 5 tasks complete, milestone ready to archive
- Pending archive: move `milestones/meta-loop/` → `archive/meta-loop/` and update `00-overview.md`
