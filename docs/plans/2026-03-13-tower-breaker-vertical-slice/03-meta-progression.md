# Tower Breaker Meta Progression Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Implement the smallest reward, inventory, equipment, and reroll loop needed to make the Excel-backed battle slice replayable.

**Architecture:** Keep meta progression in plain services over explicit data/state objects, with inventory mutations and economy changes routed through small application services. Use `GameData` assets for tables and definitions so the first slice can be tuned without code churn, and keep item/weapon presentation assets referenced by string keys so UI can resolve icons and reward visuals through Addressables.

**Tech Stack:** Unity 6, C#, existing `TowerBreak.GameData`, existing `TowerBreak.EventBus`, NUnit

---

### Task 1: Create player inventory and wallet state

**Files:**
- Create: `Assets/Scripts/Meta/TowerBreak.Meta.asmdef`
- Create: `Assets/Scripts/Meta/State/PlayerInventoryState.cs`
- Create: `Assets/Scripts/Meta/State/PlayerWalletState.cs`
- Create: `Assets/Scripts/Meta/State/OwnedEquipment.cs`
- Test: `Assets/Tests/EditMode/Meta/PlayerInventoryStateTests.cs`

**Step 1: Write the failing test**

Cover adding reward items, stacking currencies, equipping one active weapon, and rejecting invalid duplicate ownership assumptions.

**Step 2: Run test to verify it fails**

Run inventory state tests.
Expected: inventory/wallet types missing.

**Step 3: Write minimal implementation**

Add small state holders for currencies, owned gear, equipped slot, and pending post-battle rewards.

**Step 4: Run test to verify it passes**

Confirm state transitions are deterministic and side effects stay isolated.

### Task 2: Implement reward resolution after battle clear

**Files:**
- Create: `Assets/Scripts/Meta/Rewards/RewardBundle.cs`
- Create: `Assets/Scripts/Meta/Rewards/RewardResolver.cs`
- Create: `Assets/Scripts/Meta/Rewards/BattleRewardService.cs`
- Create: `Assets/Scripts/Meta/Events/RewardsGrantedEvent.cs`
- Test: `Assets/Tests/EditMode/Meta/RewardResolverTests.cs`

**Step 1: Write the failing test**

Add tests for floor-based reward selection, guaranteed gold payout, equipment drop chance, and checkpoint bonus handling.

**Step 2: Run test to verify it fails**

Run reward tests.
Expected: reward services missing.

**Step 3: Write minimal implementation**

Resolve one reward bundle from current floor data and apply it to inventory/wallet state through a dedicated service.

**Step 4: Run test to verify it passes**

Confirm rewards are reproducible with seeded randomness or deterministic test doubles.

### Task 3: Implement equipment compare, equip, and minimal stat growth

**Files:**
- Create: `Assets/Scripts/Meta/Equipment/EquipmentComparisonService.cs`
- Create: `Assets/Scripts/Meta/Equipment/EquipmentStatBlock.cs`
- Test: `Assets/Tests/EditMode/Meta/EquipmentComparisonServiceTests.cs`

**Step 1: Write the failing test**

Cover weapon comparison output, equip replacement, simple stat growth visibility, and combat stat recalculation after equip.

**Step 2: Run test to verify it fails**

Run equipment service tests.
Expected: minimal equipment services missing.

**Step 3: Write minimal implementation**

Implement weapon stat aggregation with one active weapon slot and only the smallest growth hook needed for floors `1-3`. Defer enhancement and dismantle systems until after the loop is playable.

**Step 4: Run test to verify it passes**

Confirm post-growth combat stats change in the expected direction.

### Task 4: Implement reroll rules for variable weapon stats

**Files:**
- Create: `Assets/Scripts/Meta/Equipment/EquipmentRerollService.cs`
- Create: `Assets/Scripts/Meta/Equipment/RerollCostPolicy.cs`
- Create: `Assets/Scripts/Meta/Equipment/RolledStatValue.cs`
- Test: `Assets/Tests/EditMode/Meta/EquipmentRerollServiceTests.cs`

**Step 1: Write the failing test**

Cover reroll cost deduction, stat range regeneration, same-item identity preservation, and insufficient currency rejection.

**Step 2: Run test to verify it fails**

Run reroll tests.
Expected: reroll policy missing.

**Step 3: Write minimal implementation**

Implement a reroll service that consumes a fixed or table-driven cost and replaces only variable stat rolls, not weapon archetype or rarity identity. Keep any visual refresh based on imported icon/effect keys, not direct object references.

**Step 4: Run test to verify it passes**

Confirm reroll behavior matches the design doc and exposes useful UI-facing result data.

### Task 5: Implement floor progression surface first, defer checkpoint persistence

**Files:**
- Create: `Assets/Scripts/Meta/Progression/FloorProgressionService.cs`
- Test: `Assets/Tests/EditMode/Meta/FloorProgressionServiceTests.cs`

**Step 1: Write the failing test**

Add tests for floor advance, defeat retry target, and returning to the lobby with the next challenge unlocked.

**Step 2: Run test to verify it fails**

Run floor progression tests.
Expected: progression surface missing.

**Step 3: Write minimal implementation**

Track current floor and highest cleared floor in a small runtime state surface. Defer checkpoint persistence and save/load round-tripping until after the MVP is playable.

**Step 4: Run test to verify it passes**

Confirm progression state survives the title -> lobby -> battle -> reward -> lobby loop.
