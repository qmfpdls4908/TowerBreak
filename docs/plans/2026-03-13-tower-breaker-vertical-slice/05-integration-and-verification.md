# Tower Breaker Integration And Verification Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Integrate the Excel-backed and Addressables-backed playable MVP end to end, verify the core loop manually and automatically, and leave the repo ready for direct content iteration.

**Architecture:** Treat integration as a thin composition step over already-tested modules. Validation should prove scene flow, Addressables key resolution, and battle-to-growth feedback loops, while avoiding late-stage rewrites caused by hidden coupling.

**Tech Stack:** Unity 6, C#, NUnit, Unity Test Framework, `dotnet build`

---

### Task 1: Add end-to-end application flow tests

**Files:**
- Create: `Assets/Tests/EditMode/Core/VerticalSliceFlowTests.cs`
- Modify: `Assets/Scenes/Bootstrap.unity`
- Modify: `Assets/Scenes/Lobby.unity`
- Modify: `Assets/Scenes/Battle.unity`

**Step 1: Write the failing test**

Add tests for title -> lobby -> battle -> rewards -> growth -> next battle readiness, using imported placeholder data and test doubles for scene loading where needed.

**Step 2: Run test to verify it fails**

Run `VerticalSliceFlowTests`.
Expected: missing integration seams or scene flow gaps.

**Step 3: Write minimal implementation**

Fill the composition gaps only: missing registrations, stale event subscriptions, or state synchronization issues.

**Step 4: Run test to verify it passes**

Confirm the loop works as one coherent application flow.

### Task 2: Add regression coverage for critical design rules

**Files:**
- Create: `Assets/Tests/EditMode/Combat/DangerStateRegressionTests.cs`
- Create: `Assets/Tests/EditMode/Meta/FloorProgressionRegressionTests.cs`
- Create: `Assets/Tests/EditMode/UIFlow/RewardToLobbyRegressionTests.cs`

**Step 1: Write the failing test**

Cover the most failure-prone MVP rules from the design docs: wall HP loss, floor clear progression, reward application visibility in the lobby, and loud failure when a required Addressables key is missing.

**Step 2: Run test to verify it fails**

Run the regression fixtures.
Expected: at least one integration defect or missing behavior surface.

**Step 3: Write minimal implementation**

Fix the smallest composition issues required to satisfy the rules.

**Step 4: Run test to verify it passes**

Confirm the slice protects the intended player loop from regressions.

### Task 3: Run compile and Unity verification passes

**Files:**
- Modify: `docs/plans/2026-03-13-tower-breaker-vertical-slice/05-integration-and-verification.md`

**Step 1: Run solution build**

Run: `dotnet build "TowerBreak.sln"`
Expected: DI and GameData projects compile without analyzer errors. If newly added gameplay asmdefs do not appear in the generated solution yet, document that limitation.

**Step 2: Run targeted Unity EditMode tests**

Run the relevant Unity batchmode command for the new fixtures.
Expected: affected EditMode tests pass when the editor is not already locking the project.

**Step 3: Run manual smoke checklist**

Verify these in the editor:
- start from title screen
- continue button respects save existence
- lobby shows current floor, power, and resources
- battle starts with correct floor data
- required enemy prefab, icon, and audio keys resolve successfully in the playable path
- left-wall collision removes one heart
- guard and dash can recover the line
- floor clear grants rewards
- growth actions update next-battle stats
- next floor unlock is visible after clear

### Task 4: Record follow-up backlog after slice completion

**Files:**
- Modify: `docs/plans/2026-03-13-tower-breaker-vertical-slice/00-overview.md`

**Step 1: List phase-2 upgrades**

Document the next wave only after the MVP passes verification:
- floor `4+` content expansion through Excel rows
- demonization comeback system
- enhancement and dismantle depth
- boss logic for floor `10`
- expanded checkpoint/save UX

**Step 2: Keep backlog separate from MVP work**

Do not fold phase-2 backlog into the first implementation branch. Keep the accepted slice small and stable.
