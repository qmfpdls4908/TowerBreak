# Data Schema Research

## Reference Documents

- `docs/plans/2026-03-13-tower-breaker-vertical-slice/00-overview.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/01-foundation-and-bootstrap.md`
- `docs/plans/2026-03-13-unity-excel-gamedata-converter.md`

## Code And Tests Researched

- `Assets/Scripts/GameData/TowerBreak.GameData.asmdef`
- `Assets/Tests/EditMode/GameData/TowerBreak.GameData.Tests.asmdef`
- `Assets/Tests/EditMode/GameData/ExcelImportScaffoldingTests.cs`
- `Assets/Scripts/GameData/Attributes/SheetAttribute.cs`
- `Assets/Scripts/GameData/Attributes/ColumnAttribute.cs`
- `Assets/Scripts/GameData/Attributes/IgnoreAttribute.cs`
- `Assets/Scripts/GameData/Parsing/ExcelParseContext.cs`
- `Assets/Scripts/GameData/Parsing/ExcelParserRegistry.cs`

## Research Findings

### Assembly boundary findings

- `TowerBreak.GameData` is a runtime assembly and already allows Unity engine references, so `ScriptableObject`-based import targets are valid here.
- `TowerBreak.GameData.Tests` is editor-only and already references the runtime and editor game-data assemblies, so it is the correct place for schema reflection tests.
- There is no separate gameplay runtime assembly yet for Tower Breaker systems, so placing the initial schema under `Assets/Scripts/GameData/TowerBreaker/` avoids unnecessary assembly churn.

### Existing implementation pattern findings

- The current import scaffolding test validates attributes with reflection, not full workbook import behavior, so a reflection-based schema test matches the existing maturity of the codebase.
- Existing runtime DTOs like `WorkbookData`, `SheetData`, and `CellValue` are small and serialization-friendly, which supports keeping the first gameplay schema simple and field-based.
- The repo conventions favor plain serialized fields for Unity data shapes, which aligns with the gameplay rows being simple `[Serializable]` classes.

### Schema-specific findings

- The agreed first workbook contract needs eight row types: floors, floor waves, enemies, weapons, reward tables, reward entries, enhancement costs, and reroll costs.
- Addressables-backed content should be represented only by string keys in the schema. The importer can treat those as normal strings, while runtime validation can verify key existence later.
- `FloorRow`, `EnemyRow`, `WeaponRow`, `RewardTableRow`, and `RewardEntryRow` need content-facing keys first; wave/cost rows do not currently require asset keys.
- The current attribute types are simple string-name markers, so the safest MVP contract is to keep workbook sheet names and header names identical to the C# field names.
- The first workbook can safely ship with placeholder rows as long as the ids are coherent across floors, waves, enemies, weapons, and reward tables.

### Current risk / blocker findings

- Unity-generated `.csproj` files are stale until Unity recompiles, so `dotnet test` is not currently a reliable way to validate newly added Unity test files in this workspace.
- CLI Unity EditMode runs are blocked if another Unity editor instance already has the project open.
- The current repo has a solid workbook reader, but not yet a completed generic pipeline from workbook sheets into a populated `TowerBreakerGameData.asset`.

## Implementation Guardrails

- Freeze field names deliberately before creating workbook headers.
- Do not add `AssetReference` fields to the row classes for MVP.
- Keep one production type per file.
- Keep workbook headers identical to `ColumnAttribute` values and change them only when both code and workbook contract docs are updated together.
