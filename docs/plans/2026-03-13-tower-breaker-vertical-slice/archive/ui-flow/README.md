# UI Flow Milestone

## Goal

Implement the smallest presenter/view-driven UI flow that turns the completed combat and meta services into a playable vertical slice: title -> lobby -> battle HUD -> reward/growth surfaces.

## Reference Documents

- `docs/plans/2026-03-13-tower-breaker-vertical-slice/00-overview.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/04-ui-flow-and-content.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/archive/combat-loop/README.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/archive/meta-loop/README.md`

## Code And Tests To Research First

- `Assets/Scripts/Combat/CombatSampleSceneBootstrap.cs`
- `Assets/Scripts/Meta/`
- `Assets/Scripts/GameData/Addressables/`
- `Assets/Scenes/`
- `Assets/Prefabs/UI/`

## Research Findings

- Combat and meta milestones are now pure-service complete, so UI flow can stay focused on presenter/view wiring rather than inventing new domain rules.
- `04-ui-flow-and-content.md` already breaks UI work into a minimal order: title, lobby, battle HUD, reward/growth, then placeholder content validation.
- The next smallest useful start is not a full scene stack; it is a `UIFlow` asmdef plus the first presenter tests for title or lobby routing.
- Since combat/meta logic already exists, `ui-flow` should mostly project existing state into views and forward user intent back to domain services.

## Implementation Guardrails

- Keep UI logic in a dedicated `UIFlow` runtime assembly under `Assets/Scripts/UIFlow/`.
- Prefer presenter/view pairs and avoid burying gameplay decisions in MonoBehaviours.
- Reuse existing combat/meta services instead of duplicating their rules in UI state.
- Keep art and prefab work placeholder-level until the flow itself is proven.

## Checklist

- Done: create `Assets/Scripts/UIFlow/TowerBreak.UIFlow.asmdef`
- Done: create `Assets/Tests/EditMode/UIFlow/TowerBreak.UIFlow.Tests.asmdef`
- Done: Task 1 — `TitleScreenPresenter` + `TitleScreenView` + interfaces — 13/13 tests
- Done: Task 2 — `LobbyPresenter` + `LobbyView` + `ILobbyFlowRouter` + `ILobbyStateReader` — 17/17 tests
- Done: Task 3 — `BattleHudPresenter` + `BattleHudView` + `IBattleHudStateReader` — 18/18 tests
- Done: Task 4 — `RewardResultsPresenter` + `RewardResultsView` + `GrowthScreenPresenter` + `GrowthScreenView` — 31/31 tests
- Done: Task 5 — `VerticalSliceContentValidationTests` + placeholder data/scenes/dir — 11/11 tests

## Verification

- 2026-03-14: Task 1 — RED `uiflow-view-red.log` CS0246 × 4 → GREEN `uiflow-view-full.xml` 13/13
- 2026-03-14: Task 2 — RED `lobby-red.log` CS0234/CS0246 × 10 → GREEN `lobby-uiflow-full.xml` 30/30
- 2026-03-14: Task 3 — RED `hud-red.log` CS0234/CS0246 × 8 → GREEN `hud-uiflow-full.xml` 48/48
- 2026-03-14: Task 4 — RED `reward-red.log` CS0234/CS0246 × 6+ → GREEN `task4-uiflow-full.xml` 79/79
- 2026-03-14: Task 5 — RED `content-red.xml` 3/11 (missing data/scenes/dir) → GREEN `task5-uiflow-full.xml` 90/90

## Manual Follow-Ups

- Pending: connect `TitleScreenPresenter` + `TitleScreenView` to Bootstrap scene and verify play-mode routing
- Pending: connect `LobbyPresenter` + `LobbyView` to Lobby scene and verify play-mode routing
- Pending: connect `BattleHudPresenter` + `BattleHudView` to Battle scene; wire `IBattleHudStateReader` to actual `CombatState`
- Pending: connect `RewardResultsPresenter` + `GrowthScreenPresenter` to post-battle flow; wire readers to actual Meta state
- Pending: add scene-level smoke steps once presenter/view pairs are wired to actual buttons in prefabs
