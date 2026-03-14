# Tower Breaker Vertical Slice Overview Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Build a playable MVP that starts with Excel-backed game data, uses Addressables-backed content keys for loadable assets, then covers title -> lobby -> tower entry -> one lane-pushing battle -> clear reward -> simple growth -> next floor preparation.

**Architecture:** Deliver the slice in two passes. First, extract all tunable gameplay data into explicit row classes and import them through the existing `GameData` Excel pipeline. Those row classes should store content references as plain string keys so designers can point to Addressables entries directly from Excel without Unity object references. Second, build the smallest playable runtime loop on top of those imported catalogs. Reuse the existing `DI` module for scene/service wiring, reuse `EventBus` for gameplay notifications, and keep combat/meta rules thin enough that new content can be added by editing sheets rather than rewriting systems.

**Tech Stack:** Unity 6, C#, URP 2D, Input System, uGUI, Addressables, existing `TowerBreak.DI`, existing `TowerBreak.EventBus`, existing `TowerBreak.GameData`, NUnit, Unity Test Framework

---

## 1. Scope of the first playable slice

The source direction documents define a clear playable loop:
- title screen with immediate goal framing
- lobby as the preparation hub
- floor entry transition
- simple side-view battle driven by three actions: dash, guard, attack
- danger state when the player is pushed into the left wall
- floor clear reward bundle
- equipment comparison and one-step growth before the next challenge

Before gameplay implementation, extract and lock the first-pass game-data schema for Excel conversion. The initial workbook should cover these tunable row types:
- `FloorRow`
- `FloorWaveRow`
- `EnemyRow`
- `WeaponRow`
- `RewardTableRow`
- `RewardEntryRow`
- `EnhancementCostRow`
- `RerollCostRow`

All content-facing rows should prefer string key fields with a `Key` suffix for Addressables-managed assets such as prefabs, icons, portraits, VFX, SFX, and BGM.

For the first playable MVP, avoid full production breadth. Ship enough systems to validate the core feel and let design iteration happen through Excel:
- one playable combat scene
- 2 weapon archetypes: `Claw`, `Lance`
- 2 enemy archetypes for MVP: basic melee, armored pusher
- floors `1-3` only
- one reward pass: gold + one weapon drop table
- one growth pass: equip + reroll
- enhancement, demonization, floor `10` checkpoint, and richer enemy variants move to post-MVP

Out of scope for this MVP:
- live service systems, ads, economy balancing, cloud save
- content for all `111` floors
- advanced AI trees, boss cinematics, localization pipeline
- final art polish, production HUD animation polish, analytics
- floor `10` checkpoint flow
- demonization comeback system
- enhancement and dismantle depth beyond placeholder hooks

## 2. Recommended project structure additions

Create new modules instead of mixing gameplay into existing utility assemblies.

**Runtime assemblies**
- `Assets/Scripts/Core/TowerBreak.Core.asmdef`
- `Assets/Scripts/Combat/TowerBreak.Combat.asmdef`
- `Assets/Scripts/Meta/TowerBreak.Meta.asmdef`
- `Assets/Scripts/UIFlow/TowerBreak.UIFlow.asmdef`

**Editor/data support**
- `Assets/Scripts/GameData/` existing module remains the source for progression tables and floor configs

**Tests**
- `Assets/Tests/EditMode/Core/TowerBreak.Core.Tests.asmdef`
- `Assets/Tests/EditMode/Combat/TowerBreak.Combat.Tests.asmdef`
- `Assets/Tests/EditMode/Meta/TowerBreak.Meta.Tests.asmdef`
- `Assets/Tests/EditMode/UIFlow/TowerBreak.UIFlow.Tests.asmdef`

**Scenes**
- `Assets/Scenes/Bootstrap.unity`
- `Assets/Scenes/Lobby.unity`
- `Assets/Scenes/Battle.unity`

**Prefabs and data**
- `Assets/Prefabs/UI/`
- `Assets/Prefabs/Combat/`
- `Assets/Data/GameData/`
- `Assets/Art/Placeholders/`

## 3. Delivery order

Implement in this exact order so the slice stays testable and designer-editable from the start:

### Task 1: Extract game data classes and finish Excel conversion first

**Files:**
- Follow: `docs/plans/2026-03-13-unity-excel-gamedata-converter.md`
- Follow: `docs/plans/2026-03-13-tower-breaker-vertical-slice/01-foundation-and-bootstrap.md`

**Step 1: Extract the row types and workbook schema**

Define the import target asset, the workbook sheet layout, and the minimum row classes required by combat and meta.

**Step 2: Verify Excel import on placeholder content**

Confirm one checked-in workbook can generate or update the initial gameplay catalogs without manual data entry in Unity.

### Task 2: Establish shared foundations on top of imported data

Follow the updated `01-foundation-and-bootstrap.md`.

### Task 3: Implement the smallest playable combat loop

Follow the updated `02-combat-loop.md`.

### Task 4: Implement minimal reward/growth and lobby-driven replay

Follow the updated `03-meta-progression.md` and `04-ui-flow-and-content.md`.

### Task 5: Integrate, verify, and keep phase-2 backlog separate

Follow the updated `05-integration-and-verification.md`.

## 3A. Milestone documentation workflow

Use the shared workflow in `docs/plans/2026-03-13-implementation-workflow.md` for every major implementation area in this feature.

Recommended active milestone folders for this vertical slice:
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/milestones/combat-loop/`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/milestones/meta-loop/`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/milestones/ui-flow/`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/milestones/integration/`

For each milestone:
- create the milestone folder first
- write `Reference Documents` before implementation
- research existing code/tests/asmdefs and record the findings
- update the checklist while coding and verifying
- move the milestone folder to `archive/` after implementation and verification are complete
- update this overview when a milestone starts, blocks, completes, or is archived

## 4. System interaction rules

- `Core` owns application state, scene flow, save/checkpoint coordination, and shared interfaces.
- `Combat` owns lane pressure, enemy waves, player actions, and battle outcome events.
- `Meta` owns inventory, equipment rolls, reinforcement, floor progression, and reward application.
- `UIFlow` only translates model/state into UI and forwards input to domain services.
- Cross-system communication should prefer typed events through `EventBus<T>` instead of direct references between unrelated features.
- Scene or session scoped services should be registered through `DIInstaller` classes.
- Data rows should not hold direct `UnityEngine.Object` references for gameplay content. Store Addressables string keys and resolve them at runtime through a small asset provider service.

## 5. Milestone acceptance criteria

The MVP is complete when all of these are true:
- core tunable data is loaded from Excel-imported `GameData` assets rather than hard-coded scene values
- content-facing prefabs, icons, VFX, and audio are resolved from imported string keys rather than serialized direct references
- player can launch from title to lobby to battle without manual scene setup
- battle supports hold attack, timed guard, dash recovery, enemy pushback, and wall HP loss
- clear flow grants rewards and returns to growth UI
- equipment changes modify the next battle outcome in a measurable way
- floors `1-3` can be cleared with only sheet edits for value tuning
- EditMode tests cover rules and Unity manual smoke steps are documented

## 5A. Milestone status board

- Active milestone: **none — vertical slice MVP complete**
- Archived milestones:
  - `data-schema` → `docs/plans/2026-03-13-tower-breaker-vertical-slice/archive/data-schema/`
  - `addressables-provider` → `docs/plans/2026-03-13-tower-breaker-vertical-slice/archive/addressables-provider/`
  - `combat-loop` → `docs/plans/2026-03-13-tower-breaker-vertical-slice/archive/combat-loop/`
  - `meta-loop` → `docs/plans/2026-03-13-tower-breaker-vertical-slice/archive/meta-loop/`
  - `ui-flow` → `docs/plans/2026-03-13-tower-breaker-vertical-slice/archive/ui-flow/`
  - `integration` → `docs/plans/2026-03-13-tower-breaker-vertical-slice/archive/integration/` *(ready for archive)*
- Current progress: **ALL PHASES COMPLETE** — vertical slice integration verified
- Phase completion:
  - Phase 1: Research and Documentation ✓
  - Phase 2: Core Infrastructure ✓ (15/15 tests)
  - Phase 3: Title -> Lobby Integration ✓ (15/15 tests)
  - Phase 4: Lobby -> Battle Integration ✓ (12/12 tests)
  - Phase 5: Battle -> Reward Integration ✓ (15/15 tests)
  - Phase 6: Reward -> Growth -> Lobby Integration ✓ (37/37 tests)
  - Phase 7: Full Loop Integration Test ✓ (11/11 tests)
  - Phase 8: Final Verification ✓ (323/323 tests across all suites)
- Final verified test counts:
  - UIFlow: **90/90** (`Logs/integration-uiflow-results.xml`)
  - Combat: **59/59** (`Logs/integration-combat-results.xml`)
  - Meta: **84/84** (`Logs/integration-meta-results.xml`)
  - Core: **90/90** (`Logs/integration-core-results.xml`)
  - **Total: 323/323 passing**
- Build note: `dotnet build TowerBreak.sln` deferred — `dotnet` not on PATH; Unity batchmode confirms compile success
- Next recommended action: archive `integration` milestone folder; begin phase-2 backlog (floor 4+ content, demonization, enhancement depth)

## 6. Implementation notes for the engineer

- Prefer placeholder sprites and simple rectangles first; do not block system work on art.
- Adapt `Assets/InputSystem_Actions.inputactions` instead of introducing a second input asset.
- Add `GameData` assets for floor configs, wave rows, enemy stats, weapon definitions, reward tables, enhancement costs, and reroll costs before wiring UI values.
- When a row needs to point at content, prefer fields such as `PrefabKey`, `IconKey`, `PortraitKey`, `BgmKey`, `SfxKey`, or `VfxKey` instead of serialized object references.
- Do not force every fixed bootstrap prefab into Addressables for MVP. Use Addressables first for data-driven content that changes by row.
- Keep battle simulation deterministic enough that core rules can be tested in EditMode without loading a scene.
- When Unity batch tests are blocked by an open editor instance, still run `dotnet build "TowerBreak.sln"` and document the deferred Unity command.
- Once the MVP loop feels good, expand by adding rows and sheets first, then only add code where the current schema truly blocks new content.
