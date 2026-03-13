# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

> **Also read `AGENTS.md`** — the authoritative agentic coding guide for this repo (assembly layout, code style, milestone workflow, anti-patterns).

---

## Commands

### Fast compile (no Unity required)
```bash
dotnet build TowerBreak.sln
```
Covers: DI, GameData, GameData.Editor, and their test projects.

### Unity EditMode tests (full suite)
```powershell
"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -quit -projectPath "C:\Users\admin\Desktop\Fork\TowerBreak" -runTests -testPlatform EditMode -logFile "Logs\editmode-tests.log" -testResults "Logs\editmode-tests.xml"
```
> Unity must not be open while running this — the editor locks the project.

### Filter to a single test class or method
Append to the above command:
```powershell
-testFilter "TowerBreak.DI.Tests.DIContainerTests"
-testFilter "TowerBreak.DI.Tests.DIContainerTests.ResolveFromRegistered_ReturnsNewestRegisteredContainerValue"
```

### Setup / tools
```powershell
Tools/setup-excel-reader.ps1          # Download ExcelDataReader NuGet DLLs → Assets/Plugins/Editor/
Tools/create_towerbreaker_workbook.py # Regenerate TowerBreaker-MVP.xlsx from scratch
```

### Import game data
Editor menu: **GameData > Import TowerBreaker Workbook** — reads `TowerBreaker-MVP.xlsx` and writes `Assets/Resources/TowerBreakerGameData.asset`.

---

## Architecture

TowerBreak is a Unity 6 (6000.3.11f1) tower-defense / roguelike in active development. Target scope: Floors 1–3, 2 weapon archetypes, 2 enemy archetypes.

### Module overview

| Assembly | Description | Status |
|---|---|---|
| `TowerBreak.DI` | Lightweight DI container (type+key registration, `[DIInject]` attribute, global static registry) | ✅ Stable |
| `TowerBreak.EventBus` | Generic typed pub/sub (`EventBus<T>`); no UnityEngine references | ✅ Stable |
| `TowerBreak.GameData` | Excel → ScriptableObject pipeline; 8 row types (Floor, Enemy, Weapon, Reward, etc.) | ✅ Stable |
| `TowerBreak.Combat` | Wave planning, enemy spawning, object pooling | 🔄 Active |
| `TowerBreak.Core` | Bootstrap, GameSession, scene flow, input facade | ⏳ Planned |
| `TowerBreak.Meta` | Inventory, rewards, equipment enhancement/reroll | ⏳ Planned |
| `TowerBreak.UIFlow` | Presenter/view pattern for all screens | ⏳ Planned |

### Data flow
```
Excel (.xlsx)
  → ExcelWorkbookReader (Editor-only)
  → TowerBreakerGameData.asset (ScriptableObject)
  → DI Container (registration at scene/global scope)
  → Domain services (pure C#, no UnityEngine)
  → EventBus (cross-module notifications)
  → Presenters (scene binding, Addressables loading)
  → MonoBehaviours (animation, FX, Unity lifecycle)
```

### Key design rules
- **DI for wiring:** Cross-module dependencies go through explicit DI registration — never new singletons or static globals.
- **EventBus for notifications:** Prefer `EventBus<T>.Publish()` for cross-module events over direct callbacks.
- **Domain/Presentation split:** Combat rules and state live in plain C# with no UnityEngine imports; presenters and spawners handle Unity binding.
- **Addressables key indirection:** Content rows store string keys (`PrefabKey`, `IconKey`, etc.), loaded at runtime via `IAddressableAssetProvider`.

### Current active work
Enemy spawning proof-of-concept in `SampleScene` — `WaveSpawnPlanner` → `EnemySpawnPresenter` → `PooledCombatInstantiator`. Real Addressables groups are not yet populated; a debug stub provider (`CombatDebugAddressableAssetProvider`) is used in the meantime.

---

## Code conventions

- **Allman braces** (opening brace on new line)
- **Block-scoped namespaces** (not file-scoped)
- **One public type per file**; prefer `public sealed class`
- **Test naming:** `MethodOrScenario_Condition_ExpectedOutcome`
- Fail fast with specific exception messages; validate at public API boundaries only
- Minimal inline comments — prefer clear naming

---

## Key reference documents

| Path | Purpose |
|---|---|
| `AGENTS.md` | Full agentic coding guide (style, assemblies, milestone workflow) |
| `docs/2026-03-13-tower-breaker-vertical-slice-architecture.md` | Complete target architecture for the vertical slice |
| `docs/plans/2026-03-13-tower-breaker-vertical-slice/` | Per-milestone implementation plans and archives |
