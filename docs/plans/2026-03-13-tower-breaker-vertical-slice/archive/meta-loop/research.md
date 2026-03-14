# Meta Loop Research

## Reference Documents

- `docs/plans/2026-03-13-tower-breaker-vertical-slice/00-overview.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/03-meta-progression.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/archive/combat-loop/README.md`

## Research Findings

- The next planned feature after combat is meta progression, not more combat presentation polish.
- The design plan already narrows meta progression to deterministic, testable services before any lobby UI: inventory/wallet state, reward resolution, one active equipment slot, reroll rules, and floor progression.
- The repo currently has no `Meta` asmdef or `Meta` tests, so the first implementation step should establish those boundaries and start with plain state objects.
- Combat now provides a stable battle outcome surface to build from, but there is not yet any reward handoff object or progression state.
- `TowerBreakerGameData` already contains the row types needed to begin meta work, so the milestone should consume imported rows rather than invent new serialized sources.
- `WeaponRow` already exposes the minimum first-pass combat-facing numbers and content keys for meta comparison work: `BaseAttack`, `AttackSpeed`, `PushPower`, `RerollGroupId`, and icon/VFX/SFX keys.
- `RewardTableRow` already models guaranteed gold plus a probabilistic weapon drop surface, and `RewardEntryRow` provides weighted table entries with quantity ranges; that is enough to start deterministic reward-resolution tests without new schema changes.
- `EnhancementCostRow` and `RerollCostRow` also exist, but enhancement is explicitly post-MVP depth in the overview, so the first meta step should prefer wallet/inventory/reward wiring before any reinforcement logic.

## Initial Hypothesis

- The smallest useful `meta-loop` start is Task 1 from `03-meta-progression.md`: create inventory/wallet/equipment state plus tests.
- That keeps scope tight, unlocks reward application next, and avoids premature UI-flow work.

## Task 1 Implementation Findings (2026-03-13)

- `TowerBreak.Meta.asmdef` uses `noEngineReferences: true` because state/inventory types have no Unity runtime dependency — same pattern as combat domain classes.
- `OwnedEquipment` is a plain sealed class with `InstanceId` and `WeaponId` (both `int`). It validates at construction: both must be > 0. It does not hold a `WeaponRow` reference; callers look up row data via `WeaponId` against `TowerBreakerGameData` when needed, keeping state lightweight.
- `PlayerWalletState` is mutable: `AddGold` and `TryDeductGold` operate in-place. `TryDeductGold` returns `bool` rather than throwing, which is the idiomatic pattern for "might fail due to data" vs "definitely wrong input".
- `PlayerInventoryState` rejects duplicate `InstanceId` with `InvalidOperationException` — a programming error if the caller tries to add the same instance twice. `EquipWeapon` also throws if the instance is not in inventory; callers are responsible for only requesting equip on owned items.
- `EquippedWeaponInstanceId` is `int?` (nullable) — starts null; will be non-null once the player equips. This is the single active weapon slot the design doc specifies; multi-slot or party expansion is out of current scope.
- The `IReadOnlyList<OwnedEquipment> Equipment` exposure lets future services (reward applier, comparison service) read inventory without being able to mutate the list directly.
- No `TowerBreak.GameData` types appear in the state layer yet; the GameData reference on the asmdef is pre-positioned for Task 2 (reward resolution) where `RewardTableRow` and `RewardEntryRow` will be consumed.

## Task 2 Implementation Findings (2026-03-13)

- `TowerBreak.Meta.Tests.asmdef` needed `TowerBreak.GameData` added to its references once tests started using `FloorRow`, `RewardTableRow`, `RewardEntryRow`, `RewardType` directly. The runtime `TowerBreak.Meta.asmdef` already had this reference from Task 1.
- `RewardResolver` is a pure `static` class — no instance needed. Takes `System.Random` rather than a roll policy interface because test boundary cases (`WeaponDropChance = 0.0f` → always false, `1.0f` → always true) never depend on the seed, and the weighted selection "consistent same seed" test is trivially proven without needing to know the exact output.
- `RewardBundle` is immutable: construction-time validation only. Gold must be non-negative; weapon ID list may be empty.
- `WeaponDropChance = 0.0f`: `NextDouble()` returns `[0, 1)`, so `roll < 0.0` is always false → no weapon dropped. No edge case needed.
- `WeaponDropChance = 1.0f`: `roll < 1.0` is always true (NextDouble returns `[0, 1)`) → weapon always dropped. Drop roll is always consumed, even at the boundaries, to keep random state consistent for callers that chain multiple resolves.
- Weighted entry selection: `roll = NextDouble() * totalWeight`, walk entries accumulating weight, return first entry where cumulative exceeds roll. Fallback to last entry to guard floating-point edge cases.
- `FallbackRewardId` lookup: scoped to same table (`RewardTableId == table.Id && RewardId == fallbackId`). Supports both `Gold` and `Weapon` fallback types. ~~If fallback entry is not found, fallback is silently skipped (non-fatal — the guaranteed gold is still applied).~~ **Updated (2026-03-14):** Now throws `InvalidOperationException` if fallback entry is configured but missing.
- `QuantityMin == QuantityMax` fast path skips the random call for deterministic fixed-quantity entries (most test data). Range quantities call `NextDouble` again.
- `BattleRewardService.Apply` uses `Func<int> nextInstanceId` so callers own instance ID generation. For tests: `() => nextId++` starting from any value. For production: caller maintains a counter or UUID sequence.
- The three types together (RewardBundle → RewardResolver.Resolve → BattleRewardService.Apply) form a clean, composable pipeline: data in, state mutations out, no global state.

## Task 2 Validation Fix Findings (2026-03-14)

- **Problem identified in audit:** `RewardResolver` silently skipped invalid reward data instead of failing explicitly:
  - `FallbackRewardId` configured but entry not found → silently skipped
  - Weapon drop succeeded but no weapon entries in table → silently skipped
  - Malformed `Gold` entry (`QuantityMin > QuantityMax` or negative) → silently accepted
  - Malformed `Weapon` entry (`TargetItemId <= 0`) → silently accepted
- **Fix applied:** Added explicit `InvalidOperationException` throws with descriptive messages for all invalid data cases:
  - `Weapon drop succeeded but no weapon entries found for table ID {id}`
  - `Fallback reward ID {fallbackId} not found in table ID {tableId}`
  - `Invalid gold entry: QuantityMin ({min}) cannot be negative. RewardId={id}, TableId={tableId}`
  - `Invalid gold entry: QuantityMin ({min}) cannot exceed QuantityMax ({max}). RewardId={id}, TableId={tableId}`
  - `Invalid weapon entry: TargetItemId ({targetId}) must be greater than 0. RewardId={id}, TableId={tableId}`
- **Validation placement:** Validation occurs at the point of use:
  - Weapon entry validation in `SelectWeighted` result processing and `ApplyEntry` for weapon rewards
  - Gold entry validation in `ApplyEntry` before quantity calculation
  - Fallback entry existence check immediately after lookup
- **Test coverage:** 5 new tests added to verify exception behavior for all invalid data scenarios:
  - `Resolve_FallbackRewardId_SetButEntryNotFound_ThrowsInvalidOperationException`
  - `Resolve_WeaponDropSucceeds_ButNoWeaponEntries_ThrowsInvalidOperationException`
  - `Resolve_MalformedGoldEntry_QuantityMinGreaterThanQuantityMax_ThrowsInvalidOperationException`
  - `Resolve_MalformedGoldEntry_NegativeQuantityMin_ThrowsInvalidOperationException`
  - `Resolve_MalformedWeaponEntry_TargetItemIdInvalid_ThrowsInvalidOperationException`
- All existing tests continue to pass (regression verified), and new validation tests pass (20/20 RewardResolverTests, 32/32 full Meta.Tests).

## Task 3 Implementation Findings (2026-03-14)

- `EquipmentStatBlock` is a `readonly struct` (value semantics, immutable) with `Attack` (int), `AttackSpeed` (float), `PushPower` (float). It mirrors the fields on `WeaponRow` that are directly combat-facing for MVP floors 1-3.
- `EquipmentStatBlock.FromWeaponRow` is a static factory that validates null and reads `BaseAttack`, `AttackSpeed`, `PushPower` directly — no enhancement multiplier or floor scaling at this stage.
- `EquipmentComparisonResult` is an immutable class: takes current and candidate `EquipmentStatBlock` at construction and pre-computes `DeltaAttack`, `DeltaAttackSpeed`, `DeltaPushPower` (candidate minus current). Pre-computing deltas at construction time keeps callers simple and keeps the result deterministic.
- `EquipmentComparisonService` is a pure `static` class. Three methods: `GetEquippedStatBlock` (returns zero block if no weapon equipped; throws if equipped instance or weapon row missing), `GetCandidateStatBlock` (throws if instance not in inventory or row missing), `Compare` (wraps two stat blocks in a result, no logic).
- Float precision: `1.2f - 1.0f` in float32 arithmetic yields `0.20000003f` not `0.2f` (0.2 is not representable exactly in IEEE 754 float32). Fixed integration test to use `Is.EqualTo(0.2f).Within(0.0001f)` for `DeltaAttackSpeed` and `DeltaPushPower` where non-power-of-two values appear. Exact equality only works for values that are exact sums of powers of two (0.5, 1.0, etc.).
- Missing `.meta` file: `EquipmentComparisonService.cs.meta` was absent. Created with a fresh GUID before testing — Unity requires a `.meta` file to track the asset and include it in the assembly.
- Stray `Assets/nul` file: an accidental artifact (contained a bash error redirect) caused Unity AssetDatabase to loop indefinitely during refresh. Removed the file before the clean test run.
- The three types together (`EquipmentStatBlock`, `EquipmentComparisonResult`, `EquipmentComparisonService`) form a clean comparison surface: caller gets equipped stats, gets candidate stats, calls Compare, reads deltas. No mutation, no side effects.

## Task 4 Implementation Findings (2026-03-14)

- `RerollCostRow` exposes `Rarity`, `GoldCost`, `RollCount`, `MinBonus`, `MaxBonus`. This is the complete schema needed for reroll — no additional GameData rows required.
- `RolledStatValue` is a `readonly struct` with a single `int Value` property. The additive bonus model (not a stat-kind enum) was chosen because `RerollCostRow` itself only defines a single min/max range rather than per-stat ranges. Adding a `StatKind` enum would require schema changes not in scope.
- `RerollResult` is a small immutable class (not struct, because `IReadOnlyList<RolledStatValue>` makes it a reference type naturally). Stores `InstanceId` and `WeaponId` from the source `OwnedEquipment` so callers can verify identity without holding the original reference.
- `RerollCostPolicy` is a pure static helper with a single `FindRow` method. Kept separate from `EquipmentRerollService` so the policy lookup is independently testable without standing up a full service call.
- `EquipmentRerollService.Reroll` uses `Random.Next(range)` rather than `NextDouble() * range` for integer roll — `Random.Next(maxValue)` returns `[0, maxValue)` which is the correct discrete uniform distribution. `NextDouble()` would require a cast and has a subtle bias at the boundary.
- Gold deduction is atomic with respect to the success path: `TryDeductGold` is called after validation but before rolling. If gold is insufficient, the method throws `InvalidOperationException` and the wallet is unchanged (verified by test).
- `OwnedEquipment` was not modified — rolled stats are returned as part of `RerollResult` rather than stored on the item. The caller owns the association between `InstanceId` and rolled stats. This respects "inventory 구조를 뒤엎지 말 것" and avoids re-designing the state layer for Task 4.
- Validation order in `EquipmentRerollService`: null guards first (programming errors), then `FindRow` (data error), then `ValidateCostRow` (data error), then `TryDeductGold` (runtime state). This ensures the cheapest and most informative errors fire earliest.
- `ValidateCostRow` catches two invalid configurations: `RollCount <= 0` and `MinBonus > MaxBonus`. Both would cause silent misbehavior (zero rolls or negative range) without explicit guards.

## Task 5 Implementation Findings (2026-03-14)

- `FloorRow` has no `NextFloorId` field — "next floor" is determined by list index position (`floors[currentIndex + 1]`). This is the only ordering contract available from the data schema, and it matches the design intent of an ordered floor sequence.
- `BattleOutcome` is a plain enum with two values: `Clear` and `Fail`. No additional outcome types (timeout, surrender, etc.) are in scope for MVP floors 1-3.
- `FloorProgressionResult` is an immutable class with four properties: `CurrentFloorId` (int), `NextFloorId` (int?), `IsRunComplete` (bool), `CanContinue` (bool). The nullable `NextFloorId` cleanly expresses "no next floor" for both the fail path and the run-complete path without needing a sentinel value.
- `FloorProgressionService.Advance` handles three output branches: (1) Fail → `{canContinue: false, nextFloorId: null, isRunComplete: false}`, (2) Clear + last floor → `{canContinue: false, nextFloorId: null, isRunComplete: true}`, (3) Clear + more floors → `{canContinue: true, nextFloorId: floors[i+1].Id, isRunComplete: false}`. All three branches are deterministic given the same inputs.
- Validation: null floors throws `ArgumentNullException`; empty floors throws `InvalidOperationException` (data contract violation); unknown floor ID throws `InvalidOperationException` with the ID in the message. No silent fallbacks.
- All types live under `TowerBreak.Meta.Progression` namespace in `Assets/Scripts/Meta/Progression/`. This is consistent with the `Meta` asmdef's `noEngineReferences: true` — no Unity types needed.
- The service intentionally does not take `PlayerInventoryState` or `PlayerWalletState` as inputs. Progression is a pure decision on floor structure and battle result; applying rewards is the caller's responsibility (already handled by `BattleRewardService` from Task 2).
