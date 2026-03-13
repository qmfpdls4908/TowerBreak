# Tower Breaker Foundation And Bootstrap Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Create the shared gameplay foundation needed to wire input, scenes, services, Addressables-backed asset loading, and Excel-imported data assets for the playable MVP.

**Architecture:** Build a thin application layer that owns scene transitions, shared state, and runtime asset resolution, while domain modules remain plain C# where possible. Keep battle rules testable outside `MonoBehaviour` classes, use installers to register runtime services with the existing DI container, and isolate Addressables behind a small provider interface so data rows remain plain string keys.

**Tech Stack:** Unity 6, C#, Input System, Addressables, existing `TowerBreak.DI`, existing `TowerBreak.EventBus`, existing `TowerBreak.GameData`, NUnit

---

### Task 1: Create shared bootstrap assembly and installer surface

**Files:**
- Create: `Assets/Scripts/Core/TowerBreak.Core.asmdef`
- Create: `Assets/Scripts/Core/Application/GameBootstrap.cs`
- Create: `Assets/Scripts/Core/Application/GameSession.cs`
- Create: `Assets/Scripts/Core/Application/SceneFlowService.cs`
- Create: `Assets/Scripts/Core/Application/CoreInstaller.cs`
- Test: `Assets/Tests/EditMode/Core/GameBootstrapTests.cs`

**Step 1: Write the failing test**

Add tests that assert bootstrap registration creates one shared `GameSession`, registers a scene flow service, and rejects duplicate bootstrap initialization.

**Step 2: Run test to verify it fails**

Run Unity EditMode tests for `GameBootstrapTests`.
Expected: missing types or unresolved DI registrations.

**Step 3: Write minimal implementation**

Implement a bootstrap entry that registers session-scoped services into `DIContainer`, keeps track of current floor/highest cleared floor/session loadout, and exposes scene load requests through `SceneFlowService`.

**Step 4: Run test to verify it passes**

Run `GameBootstrapTests` again.
Expected: core bootstrap tests pass.

### Task 2: Normalize game state contracts

**Files:**
- Create: `Assets/Scripts/Core/State/FloorProgressState.cs`
- Create: `Assets/Scripts/Core/State/PlayerLoadoutState.cs`
- Create: `Assets/Scripts/Core/State/RunContext.cs`
- Create: `Assets/Scripts/Core/Events/FloorStartedEvent.cs`
- Create: `Assets/Scripts/Core/Events/FloorClearedEvent.cs`
- Create: `Assets/Scripts/Core/Events/PlayerDefeatedEvent.cs`
- Test: `Assets/Tests/EditMode/Core/GameSessionStateTests.cs`

**Step 1: Write the failing test**

Define tests for floor progression transitions, next-floor unlock updates, and active loadout replacement after growth changes.

**Step 2: Run test to verify it fails**

Run session state tests.
Expected: state types missing.

**Step 3: Write minimal implementation**

Add immutable or narrowly mutable state holders that make current floor, highest cleared floor, equipped weapon, and pending rewards explicit.

**Step 4: Run test to verify it passes**

Run the same tests and confirm state transitions are deterministic.

### Task 3: Refactor the input asset for the 3-button combat model

**Files:**
- Modify: `Assets/InputSystem_Actions.inputactions`
- Create: `Assets/Scripts/Core/Input/PlayerInputActionsFacade.cs`
- Create: `Assets/Scripts/Core/Input/CombatInputSnapshot.cs`
- Test: `Assets/Tests/EditMode/Core/CombatInputSnapshotTests.cs`

**Step 1: Write the failing test**

Add tests that translate raw actions into a normalized input snapshot with `AttackHeld`, `DashPressed`, and `GuardPressed` flags.

**Step 2: Run test to verify it fails**

Run input snapshot tests.
Expected: no facade exists for the required 3-button rules.

**Step 3: Write minimal implementation**

Adapt the current input asset so the player-facing combat map exposes `Attack`, `Dash`, and `Guard` actions while preserving UI navigation actions.

**Step 4: Run test to verify it passes**

Confirm snapshots are generated consistently and no combat logic depends directly on `InputAction.CallbackContext`.

### Task 4: Extract gameplay data classes and wire Excel-backed catalogs

**Files:**
- Create: `Assets/Scripts/Core/Data/TowerBreakerGameData.cs`
- Create: `Assets/Scripts/Core/Data/FloorRow.cs`
- Create: `Assets/Scripts/Core/Data/FloorWaveRow.cs`
- Create: `Assets/Scripts/Core/Data/EnemyRow.cs`
- Create: `Assets/Scripts/Core/Data/WeaponRow.cs`
- Create: `Assets/Scripts/Core/Data/RewardTableRow.cs`
- Create: `Assets/Scripts/Core/Data/RewardEntryRow.cs`
- Create: `Assets/Scripts/Core/Data/EnhancementCostRow.cs`
- Create: `Assets/Scripts/Core/Data/RerollCostRow.cs`
- Create: `Assets/Data/GameData/TowerBreakerGameData.asset`
- Create: `Assets/Data/Design/TowerBreaker-MVP.xlsx`
- Test: `Assets/Tests/EditMode/Core/GameDefinitionValidationTests.cs`

**Step 1: Write the failing test**

Add validation tests for required sheets and fields such as floor id, wave list presence, enemy ids, weapon archetype, reward entries, enhancement costs, reroll ranges, and non-empty required Addressables keys.

**Step 2: Run test to verify it fails**

Run definition validation tests.
Expected: workbook schema, row classes, or import target asset do not exist.

**Step 3: Write minimal implementation**

Create one import target `ScriptableObject` plus the row classes required by combat and meta, then verify the Excel pipeline can generate the first placeholder data set used by the MVP. Keep content references as plain strings such as `PrefabKey`, `IconKey`, `PortraitKey`, `BgmKey`, `SfxKey`, and `VfxKey`.

**Step 4: Run test to verify it passes**

Confirm invalid workbook content fails loudly and valid placeholder imports load correctly.

### Task 5: Add Addressables runtime loading surface

**Files:**
- Create: `Assets/Scripts/Core/Assets/IAddressableAssetProvider.cs`
- Create: `Assets/Scripts/Core/Assets/AddressableAssetProvider.cs`
- Create: `Assets/Scripts/Core/Assets/AddressableKeyValidator.cs`
- Create: `Assets/Scripts/Core/Application/AddressablesInstaller.cs`
- Test: `Assets/Tests/EditMode/Core/AddressableAssetProviderTests.cs`

**Step 1: Write the failing test**

Add tests for typed asset requests by string key, repeated-load caching behavior where appropriate, and loud failures when a required key is missing or empty.

**Step 2: Run test to verify it fails**

Run Addressables surface tests.
Expected: asset provider and validation surface missing.

**Step 3: Write minimal implementation**

Wrap Addressables behind one small provider interface that accepts string keys from imported data rows and resolves typed assets for scenes, presenters, audio, and effects.

**Step 4: Run test to verify it passes**

Confirm runtime systems can depend on the provider without directly depending on workbook schema details.

### Task 6: Create scene-level installers

**Files:**
- Create: `Assets/Scripts/Core/Application/LobbyInstaller.cs`
- Create: `Assets/Scripts/Core/Application/BattleInstaller.cs`
- Create: `Assets/Scenes/Lobby.unity`
- Create: `Assets/Scenes/Battle.unity`
- Test: `Assets/Tests/EditMode/Core/SceneInstallerTests.cs`

**Step 1: Write the failing test**

Add tests that verify lobby-only services and battle-only services are scoped correctly and removed when installers are destroyed.

**Step 2: Run test to verify it fails**

Run scene installer tests.
Expected: missing installers or unresolved service scope behavior.

**Step 3: Write minimal implementation**

Create installers that register scene-specific presenters/controllers without turning `GameSession` into a service locator.

**Step 4: Run test to verify it passes**

Confirm scoping behavior matches the existing DI module conventions.
