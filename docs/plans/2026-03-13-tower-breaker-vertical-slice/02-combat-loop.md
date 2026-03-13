# Tower Breaker Combat Loop Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Implement the lane-pushing combat slice with attack hold, guard recovery, dash repositioning, wall HP loss, enemy pressure, and enough data-driven hooks to tune the MVP from Excel and load combat content through Addressables keys.

**Architecture:** Separate combat into a pure rules layer and a scene adapter layer. The rules layer should operate on explicit battle state objects and emit typed events; `MonoBehaviour` presenters should only feed input, drive animation hooks, and sync transforms/UI. Content rows should expose string keys, and the scene adapter layer should resolve enemy prefabs, hit effects, and combat audio through the shared Addressables provider.

For delivery order, prefer a two-step spawn path:
- first, prove one enemy can spawn visibly from imported data and Addressables keys using the smallest clear implementation
- second, introduce object pooling for repeated combat spawns once the visible spawn path is verified end to end

**Tech Stack:** Unity 6, C#, Input System, Addressables, existing `TowerBreak.EventBus`, NUnit, Unity Test Framework

---

### Task 1: Model battle state and lane pressure rules

**Files:**
- Create: `Assets/Scripts/Combat/TowerBreak.Combat.asmdef`
- Create: `Assets/Scripts/Combat/Domain/BattleState.cs`
- Create: `Assets/Scripts/Combat/Domain/LanePressureState.cs`
- Create: `Assets/Scripts/Combat/Domain/PlayerCombatState.cs`
- Create: `Assets/Scripts/Combat/Domain/EnemyCombatState.cs`
- Test: `Assets/Tests/EditMode/Combat/BattleStateTests.cs`

**Step 1: Write the failing test**

Cover line advancement, pushback limits, player left-wall collision, HP loss, and danger-state lock rules.

**Step 2: Run test to verify it fails**

Run `BattleStateTests`.
Expected: battle domain types do not exist.

**Step 3: Write minimal implementation**

Represent battle as explicit state values: current lane position, player hearts, enemy pressure, active stagger, and battle outcome.

**Step 4: Run test to verify it passes**

Confirm line-push rules and wall penalty behavior pass under deterministic inputs.

### Task 2: Implement the three-action player controller rules

**Files:**
- Create: `Assets/Scripts/Combat/Domain/PlayerActionResolver.cs`
- Create: `Assets/Scripts/Combat/Domain/CombatActionType.cs`
- Create: `Assets/Scripts/Combat/Domain/CombatFrameInput.cs`
- Test: `Assets/Tests/EditMode/Combat/PlayerActionResolverTests.cs`

**Step 1: Write the failing test**

Add tests that validate hold-to-attack cadence, dash cooldown windows, and guard restoring lane distance while attack is deprioritized during danger.

**Step 2: Run test to verify it fails**

Run resolver tests.
Expected: action resolver missing.

**Step 3: Write minimal implementation**

Translate normalized input snapshots into one chosen combat action per simulation tick, using weapon stats and state restrictions.

**Step 4: Run test to verify it passes**

Confirm action resolution matches the intended risk/recovery loop.

### Task 3: Add weapon archetype differences

**Files:**
- Create: `Assets/Scripts/Combat/Domain/WeaponArchetype.cs`
- Create: `Assets/Scripts/Combat/Domain/WeaponAttackProfile.cs`
- Create: `Assets/Scripts/Combat/Domain/AttackResolutionService.cs`
- Test: `Assets/Tests/EditMode/Combat/AttackResolutionServiceTests.cs`

**Step 1: Write the failing test**

Cover `Claw` double-hit behavior, `Lance` pierce behavior, and the effect of attack speed/damage modifiers from equipment stats.

**Step 2: Run test to verify it fails**

Run attack resolution tests.
Expected: weapon-specific attack profiles missing.

**Step 3: Write minimal implementation**

Implement a profile-driven attack resolver that can hit one or more enemies, apply pushback, and scale with loadout values.

**Step 4: Run test to verify it passes**

Confirm archetype differentiation is visible in rules without scene dependencies.

### Task 4: Implement wave pacing and enemy archetypes

**Files:**
- Create: `Assets/Scripts/Combat/Domain/WaveDirector.cs`
- Create: `Assets/Scripts/Combat/Domain/EnemySpawnPlan.cs`
- Create: `Assets/Scripts/Combat/Domain/EnemyArchetype.cs`
- Create: `Assets/Scripts/Combat/Presentation/EnemyPresenter.cs`
- Test: `Assets/Tests/EditMode/Combat/WaveDirectorTests.cs`

**Step 1: Write the failing test**

Add tests for timed enemy spawning, basic melee pressure, and armored enemy pressure using imported floor/wave data, plus validation that each spawned enemy type exposes the prefab key needed by the presentation layer.

**Step 2: Run test to verify it fails**

Run wave tests.
Expected: wave coordination layer missing.

**Step 3: Write minimal implementation**

Build a wave director that consumes imported floor and wave data and spawns enemies into the battle state in deterministic order. Keep runtime state on ids and stats, while the scene layer uses the related `EnemyRow.PrefabKey` and effect/audio keys for presentation.

**Step 4: Run test to verify it passes**

Confirm floor wave data produces expected enemy composition and difficulty pacing for floors `1-3`.

### Task 5: Defer demonization to phase 2 after the playable MVP

**Files:**
- Modify later after MVP stabilization

**Step 1: Keep the hook only**

Reserve combat events and extension seams only if needed, but do not block the first playable loop on this feature.

**Step 2: Move to backlog**

Document demonization as post-MVP work once floors `1-3` are stable and fun.

### Task 6: Connect the rules layer to the battle scene

**Files:**
- Create: `Assets/Scripts/Combat/Presentation/BattleLoopController.cs`
- Create: `Assets/Scripts/Combat/Presentation/PlayerBattlePresenter.cs`
- Create: `Assets/Scripts/Combat/Presentation/BattleHudPresenter.cs`
- Create: `Assets/Prefabs/Combat/PlayerBattleRoot.prefab`
- Create: `Assets/Prefabs/Combat/EnemyBattleRoot.prefab`
- Test: `Assets/Tests/EditMode/Combat/BattleLoopControllerTests.cs`

**Step 1: Write the failing test**

Add tests that verify the controller consumes frame input, advances battle state, and publishes clear/defeat events.

**Step 2: Run test to verify it fails**

Run controller tests.
Expected: scene adapter missing.

**Step 3: Write minimal implementation**

Drive the pure simulation from Unity update ticks, then sync transforms, HUD values, and animation triggers. Instantiate data-driven combat visuals through `IAddressableAssetProvider` using imported keys instead of direct prefab references stored on scene components.

**Step 4: Run test to verify it passes**

Confirm scene glue stays thin and the domain logic remains independently testable.

### Task 7: Add combat object pooling after first visible spawn

**Files:**
- Create later after first visible spawn verification

**Step 1: Scope the first pooled objects**

Start with repeated combat objects that are likely to churn during battle, such as enemies, hit VFX, floating damage text, or projectiles if they exist by then.

**Step 2: Add pooling only after spawn path is proven**

Do not block the first visual combat milestone on pooling. Once one enemy reliably spawns from imported data and Addressables keys, replace direct repeated creation/destruction with a pool-backed lifecycle.

**Step 3: Verify runtime behavior**

Confirm pooled reuse does not break enemy identity, spawn order, despawn cleanup, or later Addressables release rules.
