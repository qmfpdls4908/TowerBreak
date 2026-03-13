# Tower Breaker UI Flow And Content Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Build the title, lobby, battle HUD, reward, and growth UI flow for a small playable MVP, using placeholder art, Excel-driven content, and Addressables-loaded presentation assets.

**Architecture:** Keep UI separated into presenter/view pairs that bind to explicit state and event streams rather than directly querying gameplay objects. Use one navigation coordinator per scene, keep text and numbers driven by definitions from `GameSession` and `GameData`, and resolve row-driven icons, portraits, and reward visuals from Addressables string keys rather than serialized sprite references.

**Tech Stack:** Unity 6, C#, uGUI, Input System UI support, Addressables, existing `TowerBreak.EventBus`, existing `TowerBreak.GameData`

---

### Task 1: Build the title screen and application entry flow

**Files:**
- Create: `Assets/Scripts/UIFlow/TowerBreak.UIFlow.asmdef`
- Create: `Assets/Scripts/UIFlow/Title/TitleScreenPresenter.cs`
- Create: `Assets/Scripts/UIFlow/Title/TitleScreenView.cs`
- Create: `Assets/Prefabs/UI/TitleScreen.prefab`
- Modify: `Assets/Scenes/Bootstrap.unity`
- Test: `Assets/Tests/EditMode/UIFlow/TitleScreenPresenterTests.cs`

**Step 1: Write the failing test**

Cover button actions for `Game Start` and a minimal disabled `Continue` state. Defer `Settings` until after MVP validation.

**Step 2: Run test to verify it fails**

Run title presenter tests.
Expected: title UI flow surface missing.

**Step 3: Write minimal implementation**

Create a title presenter that delegates scene changes and save checks to the bootstrap/application layer.

**Step 4: Run test to verify it passes**

Confirm title interactions are deterministic and not hard-coded to scene objects.

### Task 2: Build the lobby preparation hub

**Files:**
- Create: `Assets/Scripts/UIFlow/Lobby/LobbyPresenter.cs`
- Create: `Assets/Scripts/UIFlow/Lobby/LobbyView.cs`
- Create: `Assets/Scripts/UIFlow/Lobby/CurrentGoalPanelPresenter.cs`
- Create: `Assets/Prefabs/UI/LobbyScreen.prefab`
- Modify: `Assets/Scenes/Lobby.unity`
- Test: `Assets/Tests/EditMode/UIFlow/LobbyPresenterTests.cs`

**Step 1: Write the failing test**

Add tests for showing current combat power, currencies, equipped weapon summary, target floor, and button routing to challenge/equipment/reroll flows.

**Step 2: Run test to verify it fails**

Run lobby presenter tests.
Expected: lobby presenter/view missing.

**Step 3: Write minimal implementation**

Build a lobby presenter that reads from `GameSession` and meta services, then updates the hub UI without embedding progression logic in the view. Resolve equipped weapon icons and other row-driven visuals through the shared Addressables provider.

**Step 4: Run test to verify it passes**

Confirm the lobby accurately reflects current state after rewards and growth changes.

### Task 3: Build battle HUD and danger-state feedback

**Files:**
- Create: `Assets/Scripts/UIFlow/Battle/BattleHudView.cs`
- Create: `Assets/Scripts/UIFlow/Battle/BattleHudPresenter.cs`
- Create: `Assets/Scripts/UIFlow/Battle/DangerStateBannerPresenter.cs`
- Create: `Assets/Prefabs/UI/BattleHud.prefab`
- Modify: `Assets/Scenes/Battle.unity`
- Test: `Assets/Tests/EditMode/UIFlow/BattleHudPresenterTests.cs`

**Step 1: Write the failing test**

Cover heart display updates, floor/wave labels, and danger warning visibility from combat events.

**Step 2: Run test to verify it fails**

Run battle HUD tests.
Expected: HUD presenter missing.

**Step 3: Write minimal implementation**

Bind HUD widgets to combat state snapshots and event stream updates while keeping layout prefabs independent of combat logic. Load optional warning, portrait, or effect visuals from imported keys only where the MVP truly needs them.

**Step 4: Run test to verify it passes**

Confirm the HUD reacts to state changes with no direct domain mutation.

### Task 4: Build reward and growth screens

**Files:**
- Create: `Assets/Scripts/UIFlow/Results/RewardResultsPresenter.cs`
- Create: `Assets/Scripts/UIFlow/Growth/GrowthScreenPresenter.cs`
- Create: `Assets/Scripts/UIFlow/Growth/EquipmentListItemView.cs`
- Create: `Assets/Prefabs/UI/RewardResultsScreen.prefab`
- Create: `Assets/Prefabs/UI/GrowthScreen.prefab`
- Test: `Assets/Tests/EditMode/UIFlow/GrowthScreenPresenterTests.cs`

**Step 1: Write the failing test**

Cover reward display order, newly obtained equipment highlighting, compare panel output for `Claw` vs `Lance`, and action routing for equip/reroll.

**Step 2: Run test to verify it fails**

Run growth and reward presenter tests.
Expected: reward/growth UI surface missing.

**Step 3: Write minimal implementation**

Create views and presenters that consume meta progression services and update lobby-ready state before the next battle. Reward and equipment lists should render icons from imported string keys.

**Step 4: Run test to verify it passes**

Confirm UI actions change the underlying meta state and refresh the projected values.

### Task 5: Populate placeholder content for the first three floors

**Files:**
- Create: `Assets/Data/GameData/TowerBreakerGameData.asset`
- Create: `Assets/Data/Design/TowerBreaker-MVP.xlsx`
- Create: `Assets/Art/Placeholders/UI/`
- Create: `Assets/Art/Placeholders/Combat/`
- Test: `Assets/Tests/EditMode/UIFlow/VerticalSliceContentValidationTests.cs`

**Step 1: Write the failing test**

Add validation tests that require floors `1-3`, at least two weapon archetypes, two enemy archetypes, and enough reward/item definitions to complete the loop.

**Step 2: Run test to verify it fails**

Run content validation tests.
Expected: placeholder content missing or incomplete.

**Step 3: Write minimal implementation**

Create the placeholder workbook rows, assign Addressables keys for the minimum required prefabs, icons, VFX, and audio, import them into `GameData`, and add temporary visual assets that unblock the slice without pretending to be production art.

**Step 4: Run test to verify it passes**

Confirm the slice can boot, enter battle, clear a floor, and return to growth using only the checked-in imported placeholder content.
