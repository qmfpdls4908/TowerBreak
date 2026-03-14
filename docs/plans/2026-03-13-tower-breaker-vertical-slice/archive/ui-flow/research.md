# UI Flow Research

## Reference Documents

- `docs/plans/2026-03-13-tower-breaker-vertical-slice/00-overview.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/04-ui-flow-and-content.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/archive/meta-loop/README.md`

## Research Findings

- The system side of the vertical slice is largely ready: combat and meta are now covered by rule-focused EditMode tests, so UI flow can concentrate on projection and navigation.
- The UI plan already defines a sensible order: title first, then lobby, then battle HUD, then reward/growth views.
- Starting with title flow is the narrowest way to open the milestone because it only needs routing and disabled/available actions, not full battle or lobby composition.
- `UIFlow` should depend on existing domain assemblies and Addressables provider seams, but avoid becoming a second game-state layer.

## Initial Hypothesis

- The best first `ui-flow` task is Task 1 from `04-ui-flow-and-content.md`: create the `UIFlow` asmdef/test asmdef and the smallest title presenter tests before touching scene prefabs.

## Task 1 Findings (2026-03-14)

- `TowerBreak.UIFlow.asmdef` created at `Assets/Scripts/UIFlow/` with `noEngineReferences: false` (views will eventually need Unity).
- `TowerBreak.UIFlow.Tests.asmdef` created at `Assets/Tests/EditMode/UIFlow/` as Editor-only TestAssemblies.
- `ITitleFlowRouter` and `ITitleSaveStateReader` defined as plain interfaces; no scene names or Unity types exposed.
- `TitleScreenPresenter` is a pure C# sealed class; all routing delegated through interfaces.
- `IsContinueAvailable` delegates to `ITitleSaveStateReader.HasSaveData()` — no caching needed for MVP.
- `OnContinueGame` is a silent no-op when `IsContinueAvailable` is false (no exception, matches mobile UX convention).
- 8 presenter tests + 5 view tests = 13/13 passed.

## Task 3 Findings (2026-03-14)

- `CombatState` has `PlayerHealth`, `IsDangerActive`, `IsWallDefeated`, `WallHealth` — all needed for HUD display.
- `CombatState` has NO floor ID or wave index — those must come from `IBattleHudStateReader.FloorId` and `.WaveNumber`.
- Existing `TowerBreak.Combat.BattleHudPresenter` is a static utility (`BuildStatusMessage`) in a different namespace — no conflict with `TowerBreak.UIFlow.Battle.BattleHudPresenter`.
- `BattleHudPresenter` (UIFlow) has no router — HUD is display-only; player actions stay in Combat domain.
- `DangerBannerText` priority: `IsWallDefeated` → "DEFEAT" > `IsDangerActive` → "DANGER" > empty.
- `IsDangerBannerVisible` is `IsDangerActive || IsWallDefeated` — view animates visibility separately from text.
- 12 presenter tests + 6 view tests = 18/18 passed in first green run.

## Task 2 Findings (2026-03-14)

- `PlayerInventoryState` holds `EquippedWeaponInstanceId` (nullable int) — the weapon instance ID, not weapon row ID.
- `PlayerWalletState.Gold` is a plain int, directly readable.
- No "combat power" concept exists yet in meta; `EquippedWeaponId` (weapon row ID via OwnedEquipment.WeaponId) is the right MVP summary surface.
- `ILobbyStateReader` introduced in `TowerBreak.UIFlow.Lobby` namespace as a bridge: exposes `CurrentFloorId`, `Gold`, `EquippedWeaponId` (nullable int). This keeps `TowerBreak.UIFlow.asmdef` free of any `TowerBreak.Meta` reference.
- `LobbyPresenter` exposes `HasEquippedWeapon` (bool) and `EquippedWeaponId` (int?) computed from state reader — view decides how to display.
- `LobbyView.Refresh(LobbyPresenter)` follows exact same seam pattern as `TitleScreenView.Bind(TitleScreenPresenter)`.
- 12 presenter tests + 5 view tests = 17/17 passed in first green run.

## Task 4 Findings (2026-03-14)

- `RewardResultsPresenter` is display-only — no router methods beyond `OnContinue()`; battle reward data is already resolved by meta before the results screen opens.
- `IRewardResultsStateReader.GrantedWeaponIds` is `IReadOnlyList<int>` — the presenter computes `HasWeaponRewards` and `WeaponRewardCount` from `.Count`, keeping the view dumb.
- `GrowthScreenPresenter` has no reference to `EquipmentComparisonService`; instead, `IGrowthStateReader` exposes pre-computed deltas (`DeltaAttack`, `DeltaAttackSpeed`, `DeltaPushPower`) — reader is responsible for computing comparisons, not presenter.
- `IsComparisonAvailable` is `HasEquippedWeapon && HasCandidateWeapon` — view gates the comparison panel on this bool.
- `GrowthScreenView.Refresh` copies only `IsComparisonAvailable`, `DisplayedEquippedWeaponId`, `DisplayedCandidateWeaponId`, `DisplayedDeltaAttack` — the float deltas are reserved for a richer future view.
- `UIFlow` assembly still carries no reference to `TowerBreak.Meta`; the bridge is fully covered by the two new reader interfaces in `TowerBreak.UIFlow.Results` and `TowerBreak.UIFlow.Growth` namespaces.
- 8 presenter tests + 4 view tests (Results) + 14 presenter tests + 5 view tests (Growth) = 31/31 Task 4 tests; full UIFlow suite 79/79.

## Task 5 Findings (2026-03-14)

- `TowerBreakerGameData.asset` already existed at `Assets/Resources/TowerBreakerGameData.asset` with Floors (1-3) and Enemies (BasicMelee + ArmoredPusher) populated, but Weapons, RewardTables, RewardEntries, RerollCosts were all empty lists.
- `Resources.Load<TowerBreakerGameData>` works in EditMode batchmode tests without any special setup — asset is resolved from the Unity Resources folder as expected.
- Adding `TowerBreak.GameData` to the `TowerBreak.UIFlow.Tests.asmdef` references is sufficient for validation tests to access game data row types; no change to the runtime `TowerBreak.UIFlow.asmdef` is needed.
- Placeholder scenes (`Bootstrap.unity`, `Lobby.unity`, `Battle.unity`) created from the SampleScene settings-only structure (OcclusionCullingSettings + RenderSettings + LightmapSettings + NavMeshSettings, no GameObjects). Unity accepts these as valid scenes in batchmode.
- Scene existence checked with `System.IO.File.Exists(Path.Combine(Application.dataPath, "Scenes", "XXX.unity"))` — no need for AssetDatabase in the test.
- `Prefabs/UI/` directory checked with `Directory.Exists` — the directory only needs to exist for the test to pass; no prefab files are required at this stage.
- RerollCosts: only Common + Rare rows added for MVP (Epic and Legendary are post-MVP scope per overview).
- 11 content validation tests = 11/11 Task 5 tests; full UIFlow suite 90/90.
