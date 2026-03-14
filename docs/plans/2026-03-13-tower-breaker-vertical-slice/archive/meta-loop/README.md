# Meta Loop Milestone

## Goal

Implement the smallest reward, inventory, equipment, and floor-progression loop needed to make the current combat slice replayable from battle result into next-run preparation.

## Reference Documents

- `docs/plans/2026-03-13-tower-breaker-vertical-slice/00-overview.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/03-meta-progression.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/04-ui-flow-and-content.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/archive/data-schema/README.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/archive/addressables-provider/README.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/archive/combat-loop/README.md`

## Code And Tests To Research First

- `Assets/Scripts/GameData/TowerBreaker/WeaponRow.cs`
- `Assets/Scripts/GameData/TowerBreaker/RewardTableRow.cs`
- `Assets/Scripts/GameData/TowerBreaker/RewardEntryRow.cs`
- `Assets/Scripts/GameData/TowerBreaker/EnhancementCostRow.cs`
- `Assets/Scripts/GameData/TowerBreaker/RerollCostRow.cs`
- `Assets/Resources/TowerBreakerGameData.asset`
- `Assets/Scripts/Combat/`
- `Assets/Tests/EditMode/Combat/`

## Research Findings

- The foundation, Addressables provider, and combat-loop milestones are archived, so meta progression can build on stable row data and an already-playable battle smoke path.
- `03-meta-progression.md` already scopes the first meta pass tightly: inventory/wallet state, reward resolution, one active equipment slot, reroll rules, and floor progression surface.
- No `Assets/Scripts/Meta/` runtime assembly or `Assets/Tests/EditMode/Meta/` test assembly exists yet, so the first meta task should establish those boundaries before feature code spreads into unrelated modules.
- The next milestone should stay service-first and deterministic like combat: plain state objects, narrow services, and EditMode tests before any UI or scene glue.

## Implementation Guardrails

- Keep meta logic in a dedicated `Meta` runtime assembly under `Assets/Scripts/Meta/`.
- Prefer explicit state objects and small services over MonoBehaviours.
- Use imported `TowerBreakerGameData` rows as the source of truth for rewards, costs, and weapon definitions.
- Defer rich UI wiring and scene flow until the `ui-flow` milestone unless a tiny seam is strictly required.

## Checklist

- Done: research existing reward/weapon/cost data rows and confirm the minimum state/services needed for the first meta slice
- Done: create `Assets/Scripts/Meta/TowerBreak.Meta.asmdef` with `TowerBreak.GameData` reference and `noEngineReferences: true`
- Done: create `Assets/Tests/EditMode/Meta/TowerBreak.Meta.Tests.asmdef` (Editor-only, TestAssemblies)
- Done: add failing tests for `PlayerInventoryState`, `PlayerWalletState`, and `OwnedEquipment` — compile error confirmed (red)
- Done: implement `OwnedEquipment`, `PlayerWalletState`, `PlayerInventoryState` — 12/12 tests green
- Done: add `TowerBreak.GameData` reference to `TowerBreak.Meta.Tests.asmdef` (required for reward tests using `FloorRow`, `RewardTableRow`, etc.)
- Done: add failing tests for `RewardBundle`, `RewardResolver`, `BattleRewardService` (15 tests) — red confirmed
- Done: implement `RewardBundle` (immutable, gold + weapon IDs), `RewardResolver` (pure static, seeded random, guaranteed gold + drop chance + weighted selection + fallback), `BattleRewardService` (static apply to wallet + inventory)
- Done: verify 27/27 full `TowerBreak.Meta.Tests` passing (12 Task 1 + 15 Task 2)
- Done: add 5 failing tests for invalid reward data validation (fallback entry not found, weapon drop success but no entries, malformed gold entries, malformed weapon entries) — red confirmed
- Done: implement validation in `RewardResolver` to throw `InvalidOperationException` for invalid reward data instead of silent skip
- Done: verify 32/32 full `TowerBreak.Meta.Tests` passing (12 Task 1 + 20 Task 2 with validation)
- Done: add failing tests for equipment comparison (17 tests covering stat block construction, FromWeaponRow, GetEquippedStatBlock, GetCandidateStatBlock, Compare, full integration flows) — red confirmed via missing types
- Done: implement `EquipmentStatBlock` (readonly struct, `FromWeaponRow` factory, null guard)
- Done: implement `EquipmentComparisonResult` (immutable, precomputed deltas for attack/attackSpeed/pushPower)
- Done: implement `EquipmentComparisonService` (static service, null/not-found guards on all methods)
- Done: fix float precision in integration test — use `Within(0.0001f)` for `1.2f - 1.0f` delta comparison
- Done: verify 17/17 `EquipmentComparisonServiceTests` passing (targeted)
- Done: verify 49/49 `TowerBreak.Meta.Tests` passing (full suite — 12 Task 1 + 20 Task 2 + 17 Task 3)
- Done: add failing tests for reroll (19 tests — `FindRow` policy, null guards, invalid cost row, gold deduction, insufficient gold, identity preservation, stat count/bounds, determinism) — red confirmed via `CS0103`/`CS0246`
- Done: implement `RolledStatValue` (readonly struct, single int bonus value)
- Done: implement `RerollResult` (immutable read model: InstanceId + WeaponId + `IReadOnlyList<RolledStatValue>`)
- Done: implement `RerollCostPolicy` (static `FindRow` by `WeaponRarity`, null guard + not-found throw)
- Done: implement `EquipmentRerollService` (static: null guards → `ValidateCostRow` → `TryDeductGold` → roll via `Random.Next(range)` → `RerollResult`)
- Done: verify 19/19 `EquipmentRerollServiceTests` passing (targeted)
- Done: verify 68/68 `TowerBreak.Meta.Tests` passing (full suite — 12 Task 1 + 20 Task 2 + 17 Task 3 + 19 Task 4)
- Done: add failing tests for floor progression (16 tests — null/empty/unknown guards, clear middle, clear last, fail any, single floor, determinism) — red confirmed via `CS0234`/`CS0103`/`CS0246`
- Done: implement `BattleOutcome` enum (Clear, Fail) in `TowerBreak.Meta.Progression`
- Done: implement `FloorProgressionResult` (immutable: CurrentFloorId, NextFloorId?, IsRunComplete, CanContinue)
- Done: implement `FloorProgressionService.Advance` (static: null/empty guard → linear search → Fail→no-continue, Clear-last→run-complete, Clear-mid→CanContinue+NextFloorId)
- Done: verify 16/16 `FloorProgressionServiceTests` passing (targeted)
- Done: verify 84/84 `TowerBreak.Meta.Tests` passing (full suite — 12 Task 1 + 20 Task 2 + 17 Task 3 + 19 Task 4 + 16 Task 5)

## Verification

- Expected red phase: `Logs\meta-state-red.log` failed with `Scripts have compiler errors` — `PlayerInventoryState`, `PlayerWalletState`, `OwnedEquipment` were missing
- Passed command: `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "D:\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.Meta.Tests" -logFile "D:\Fork\TowerBreak\Logs\meta-state-green.log" -testResults "D:\Fork\TowerBreak\Logs\meta-state-green.xml"`
- Evidence: `Logs\meta-state-green.xml` shows `total="12" passed="12" failed="0"`
- Expected red phase: `Logs\meta-reward-red.log` failed with `Scripts have compiler errors` — `RewardBundle`, `RewardResolver`, `BattleRewardService` were missing
- Passed command: `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "D:\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.Meta.Tests.RewardResolverTests" -logFile "D:\Fork\TowerBreak\Logs\meta-reward-green.log" -testResults "D:\Fork\TowerBreak\Logs\meta-reward-green.xml"`
- Evidence: `Logs\meta-reward-green.xml` shows `total="15" passed="15" failed="0"`
- Passed command: `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "D:\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.Meta.Tests" -logFile "D:\Fork\TowerBreak\Logs\meta-reward-full.log" -testResults "D:\Fork\TowerBreak\Logs\meta-reward-full.xml"`
- Evidence: `Logs\meta-reward-full.xml` shows `total="27" passed="27" failed="0"`
- Expected red phase: `Logs\meta-reward-validation-red.log` showed 5 failed tests — `FallbackRewardId` not found, weapon drop with no entries, malformed gold/weapon entries were silently skipped instead of throwing
- Passed command: `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "D:\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.Meta.Tests.RewardResolverTests" -logFile "D:\Fork\TowerBreak\Logs\meta-reward-validation-green.log" -testResults "D:\Fork\TowerBreak\Logs\meta-reward-validation-green.xml"`
- Evidence: `Logs\meta-reward-validation-green.xml` shows `total="20" passed="20" failed="0"`
- Passed command: `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "D:\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.Meta.Tests" -logFile "D:\Fork\TowerBreak\Logs\meta-full-final.log" -testResults "D:\Fork\TowerBreak\Logs\meta-full-final.xml"`
- Evidence: `Logs\meta-full-final.xml` shows `total="32" passed="32" failed="0"`
- Expected red phase (Task 3): `Logs\meta-comparison-green.xml` showed 16/17 — `FullComparisonFlow_EquippedVsCandidate` failed on float imprecision (`1.2f - 1.0f != 0.2f` exactly in float32)
- Fix: added `Within(0.0001f)` tolerance to `DeltaAttackSpeed` and `DeltaPushPower` assertions in the integration test
- Passed command (Task 3 targeted): `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "D:\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.Meta.Tests.EquipmentComparisonServiceTests" -logFile "D:\Fork\TowerBreak\Logs\meta-comparison-green2.log" -testResults "D:\Fork\TowerBreak\Logs\meta-comparison-green2.xml"`
- Evidence: `Logs\meta-comparison-green2.xml` shows `total="17" passed="17" failed="0"`
- Passed command (Task 3 full): `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "D:\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.Meta.Tests" -logFile "D:\Fork\TowerBreak\Logs\meta-comparison-full.log" -testResults "D:\Fork\TowerBreak\Logs\meta-comparison-full.xml"`
- Evidence: `Logs\meta-comparison-full.xml` shows `total="49" passed="49" failed="0"`
- Expected red phase (Task 4): `Logs\meta-reroll-red.log` showed `CS0103`/`CS0246` — `RerollCostPolicy`, `EquipmentRerollService`, `RerollResult` were missing (TDD red)
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

- Done (Task 3): equipment comparison loop verified end-to-end
- Done (Task 4): reroll loop verified end-to-end
- Done (Task 5): floor progression surface verified — all 5 tasks complete; milestone ready to archive
