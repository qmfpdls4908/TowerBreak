# Unity Excel GameData Converter Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Build a Unity Editor-first Excel import pipeline that reads workbook sheets, converts rows into strongly typed `[Serializable]` objects, and fills a `ScriptableObject`-based `GameData` asset through convention-first reflection mapping with small attribute-based overrides.

**Architecture:** Separate the pipeline into six layers: workbook reader, sheet model, reflection metadata cache, sheet/column mapper, row materializer, and `GameData` asset builder. Keep Excel I/O independent from Unity serialization rules so the workbook reader can be swapped without rewriting conversion logic, while parser and mapper registries provide controlled extensibility for custom types, nested objects, and project-specific conventions.

**Tech Stack:** Unity Editor, C#, ScriptableObject, reflection, attribute metadata, Excel reader library (recommended: ExcelDataReader or ClosedXML via editor-only assembly), NUnit, Unity Test Framework

---

## 1. Scope and non-goals

This module targets offline data import inside the Unity Editor. The Excel workbook is treated as a read-only source of truth and `GameData` is the generated Unity-side projection. Runtime Excel loading is out of scope for MVP. Bidirectional sync, spreadsheet editing from Unity, localized formulas, and live file watching are also out of scope.

The core design goal is convention over configuration:
- workbook sheet name -> `GameData` list field name
- header text -> row field name
- string cell -> parser pipeline -> typed field value

Attributes are reserved for exceptions only:
- `[Ignore]` skips a `GameData` field or row field from mapping
- `[Sheet("...")]` overrides the default sheet name for a `GameData` field
- `[Column("...")]` overrides the default header name for a row field

The importer must fail loudly when conventions break. Silent skips are allowed only for explicitly ignored members.

## 2. Recommended folder and assembly layout

**Runtime-facing types**
- `Assets/Scripts/GameData/Core/GameDataBase.cs`
- `Assets/Scripts/GameData/Attributes/IgnoreAttribute.cs`
- `Assets/Scripts/GameData/Attributes/SheetAttribute.cs`
- `Assets/Scripts/GameData/Attributes/ColumnAttribute.cs`
- `Assets/Scripts/GameData/Parsing/IExcelValueParser.cs`
- `Assets/Scripts/GameData/Parsing/IExcelCompositeParser.cs`
- `Assets/Scripts/GameData/Parsing/ExcelParserRegistry.cs`
- `Assets/Scripts/GameData/Model/WorkbookData.cs`
- `Assets/Scripts/GameData/Model/SheetData.cs`
- `Assets/Scripts/GameData/Model/CellValue.cs`
- `Assets/Scripts/GameData/Model/MappingMetadata.cs`

**Editor-only importer**
- `Assets/Scripts/GameData/Editor/Excel/IWorkbookReader.cs`
- `Assets/Scripts/GameData/Editor/Excel/ExcelWorkbookReader.cs`
- `Assets/Scripts/GameData/Editor/Mapping/GameDataMapBuilder.cs`
- `Assets/Scripts/GameData/Editor/Mapping/RowMapBuilder.cs`
- `Assets/Scripts/GameData/Editor/Mapping/ReflectionMetadataCache.cs`
- `Assets/Scripts/GameData/Editor/Conversion/RowObjectMaterializer.cs`
- `Assets/Scripts/GameData/Editor/Conversion/GameDataBuilder.cs`
- `Assets/Scripts/GameData/Editor/Import/ExcelImportService.cs`
- `Assets/Scripts/GameData/Editor/Import/GameDataAssetUpdater.cs`
- `Assets/Scripts/GameData/Editor/UI/GameDataImportMenu.cs`
- `Assets/Scripts/GameData/Editor/UI/GameDataImportWindow.cs` (optional after MVP)

**Tests**
- `Assets/Tests/EditMode/GameData/Mapping/*.cs`
- `Assets/Tests/EditMode/GameData/Parsing/*.cs`
- `Assets/Tests/EditMode/GameData/Import/*.cs`

Use separate asmdefs for runtime, editor, and tests so the Excel package stays editor-only.

## 3. End-to-end data flow

1. User selects an Excel file and target `GameData` asset type in the Editor.
2. `IWorkbookReader` reads workbook contents into neutral in-memory models: `WorkbookData`, `SheetData`, `CellValue`.
3. `ReflectionMetadataCache` inspects the requested `GameData` type and discovers importable list fields.
4. `GameDataMapBuilder` matches workbook sheets to `GameData` fields.
5. For each matched sheet, `RowMapBuilder` matches header columns to row-type fields.
6. `RowObjectMaterializer` parses each row into a row object instance using parser registry + reflection assignment.
7. `GameDataBuilder` creates or updates the `ScriptableObject`, allocates lists, and assigns converted row objects.
8. `GameDataAssetUpdater` marks the asset dirty, saves, and logs import summary.

This separation keeps each stage testable in isolation and makes failures easy to localize.

## 4. Neutral workbook model

Do not let the rest of the system depend on the chosen Excel library API. Normalize workbook contents first.

```csharp
[Serializable]
public sealed class WorkbookData
{
    public IReadOnlyList<SheetData> Sheets { get; }
    public string SourcePath { get; }
}

[Serializable]
public sealed class SheetData
{
    public string Name { get; }
    public IReadOnlyList<string> Headers { get; }
    public IReadOnlyList<IReadOnlyList<CellValue>> Rows { get; }
}

[Serializable]
public readonly struct CellValue
{
    public readonly object RawValue;
    public readonly string DisplayText;
}
```

Rules:
- first row is always treated as header row
- headers are trimmed before mapping
- empty trailing rows may be dropped by reader
- merged cells and formulas should be flattened to resolved values when possible
- workbook reader must preserve original sheet order for deterministic diagnostics

## 5. Core type constraints

### `GameData`

Requirements:
- must inherit `ScriptableObject`
- importable members must be instance fields only for MVP
- importable members must be `List<T>`
- each `T` must be a `[Serializable]` row type

Why field-only first:
- aligns with Unity serialization behavior
- avoids property setters with side effects
- simplifies reflection caching and validation

### Row types

Requirements:
- marked with `[Serializable]`
- materialized through parameterless construction or `FormatterServices.GetUninitializedObject` fallback only if explicitly needed; prefer parameterless constructors for clarity
- field-based mapping for MVP
- nested custom serializable types allowed through parser pipeline or nested-object materializer

## 6. Attribute contract

```csharp
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Class)]
public sealed class IgnoreAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Field)]
public sealed class ColumnAttribute : Attribute
{
    public string Name { get; }
    public ColumnAttribute(string name) => Name = name;
}

[AttributeUsage(AttributeTargets.Field)]
public sealed class SheetAttribute : Attribute
{
    public string Name { get; }
    public SheetAttribute(string name) => Name = name;
}
```

Application rules:
- `GameData` list field + `[Sheet]`: override sheet name
- row field + `[Column]`: override column name
- row field + `[Ignore]`: exclude from assignment and missing-column validation
- `GameData` list field + `[Ignore]`: exclude from sheet matching and missing-sheet validation
- class-level `[Ignore]`: reserve for future support; MVP may validate but reject ignored root row types to avoid unclear semantics

If class-level `[Ignore]` is kept, define the behavior clearly: class is not importable as a root mapped type.

## 7. Reflection-based mapping rules

### Sheet -> `GameData` field

For each instance field on `TGameData`:
1. skip if `[Ignore]`
2. validate field type is `List<TRow>`
3. determine expected sheet name:
   - `[Sheet("...")]` if present
   - else field name
4. match workbook sheet by exact normalized name
5. if missing, throw unless field is explicitly optional in a later extension

Normalization recommendation:
- default: exact match after trim
- optional future policy object can add case-insensitive or culture-insensitive behavior

### Column -> row field

For each importable field on `TRow`:
1. skip if `[Ignore]`
2. determine expected column name:
   - `[Column("...")]` if present
   - else field name
3. find unique header index in sheet header row
4. if missing, throw
5. if duplicated, throw before row parsing begins

Recommendation: require unique headers per sheet. Duplicate headers are too error-prone for convention mapping.

## 8. Mapping metadata cache

Reflection is acceptable for editor-time imports, but metadata should still be cached to reduce repeated work and simplify code.

Suggested cached models:

```csharp
public sealed class GameDataFieldMap
{
    public FieldInfo Field;
    public string SheetName;
    public Type RowType;
}

public sealed class RowFieldMap
{
    public FieldInfo Field;
    public string ColumnName;
    public Type FieldType;
    public bool IsIgnored;
}
```

`ReflectionMetadataCache` responsibilities:
- validate `GameData` type once
- validate each row type once
- cache importable field lists by `Type`
- expose deterministic ordering for repeatable imports

Cache invalidation can be simple static dictionary reset on domain reload.

## 9. Parser system design

The parser system must handle primitive types and project-specific complex types without turning row materialization into a giant `switch`.

### Base interfaces

```csharp
public interface IExcelValueParser
{
    bool CanParse(Type targetType);
    object Parse(in ExcelParseContext context);
}

public interface IExcelCompositeParser
{
    bool CanParse(Type targetType);
    object Parse(in ExcelCompositeParseContext context);
}

public readonly struct ExcelParseContext
{
    public Type TargetType { get; init; }
    public string Text { get; init; }
    public object RawValue { get; init; }
    public string SheetName { get; init; }
    public int RowIndex { get; init; }
    public string ColumnName { get; init; }
}
```

Recommended split:
- `IExcelValueParser`: single-cell -> target type
- `IExcelCompositeParser`: multi-field or nested-object parsing when a type needs structured interpretation beyond one cell

### Registry

```csharp
public sealed class ExcelParserRegistry
{
    public void Register(IExcelValueParser parser);
    public void RegisterComposite(IExcelCompositeParser parser);
    public bool TryParse(in ExcelParseContext context, out object value);
}
```

Resolution order:
1. exact parser registered for target type
2. generic built-in parsers (nullable, enum, primitives)
3. composite parser
4. nested object strategy
5. fail with `ExcelTypeParseException`

### Built-in parser set for MVP

- `string`
- `int`, `long`, `float`, `double`, `decimal`
- `bool` (`true/false`, `1/0`, `yes/no` configurable)
- `enum` (case-insensitive text)
- `DateTime` if needed by project
- `Vector2`, `Vector3`, `Vector4`
- nullable versions of supported primitives/enums

### Complex type strategies

Support these in order of practicality:

1. **Single-cell custom parser**
   - Example: `RewardRange` parsed from `"10~20"`
   - Most maintainable for domain-specific types

2. **Structured text parser**
   - Example: `Vector3` from `"1,2,3"`
   - Keeps sheet flat and designer-friendly

3. **Nested prefix columns**
   - Example: row field `Stats`, nested columns `Stats.Hp`, `Stats.Speed`
   - Good for serializable nested classes/structs
   - Implement as extension stage, not MVP, because it changes header resolution rules

4. **JSON-in-cell parser**
   - Useful escape hatch for rare complex payloads
   - Keep disabled by default to avoid spreadsheet readability collapse

Recommendation: ship MVP with single-cell parsers + `Vector3` + enum + nullable support. Add nested prefix parsing in phase 2 only if real data demands it.

## 10. Row object materialization

`RowObjectMaterializer` converts one sheet row into one typed row object.

Algorithm:
1. instantiate `TRow`
2. iterate cached row field maps
3. get header index for each field
4. read `CellValue`
5. create `ExcelParseContext`
6. parse via registry
7. assign parsed value with reflection
8. wrap parser failures with row/column/sheet context

Recommended behavior for empty cells:
- `string` -> `string.Empty` or raw empty string, choose one policy and keep it consistent
- nullable types -> `null`
- reference types with custom parser -> parser decides
- non-nullable value types -> parse failure unless a default-value policy is explicitly introduced

Avoid implicit defaulting for missing required data. It hides spreadsheet mistakes.

## 11. `GameData` builder and asset update policy

`GameDataBuilder` should not know how the workbook was read. It only accepts mapping outputs and writes lists into the target asset.

Responsibilities:
- create new `ScriptableObject` if asset does not exist
- clear and replace target lists on reimport
- preserve non-imported fields on `GameData`
- assign converted `List<TRow>` instances directly to matching fields

`GameDataAssetUpdater` responsibilities:
- load existing asset by path
- create if missing
- call `Undo.RecordObject` when updating existing asset
- mark dirty and `AssetDatabase.SaveAssets()`
- optionally trigger `AssetDatabase.Refresh()` only when required

Full replace per imported list is simpler and less error-prone than partial row diffing.

## 12. Editor execution entry points

MVP entry points:
- `Tools/GameData/Import Excel -> Selected Asset`
- `Tools/GameData/Create Or Update From Excel...`

Recommended menu flow:
1. select workbook file
2. select `GameData` type or target asset
3. run import
4. show summary dialog with imported sheet counts and row counts

Phase 2 options:
- custom inspector button on `GameData` asset
- import preset asset storing workbook path + target asset path + parser profile
- batch import menu for multiple game data assets

Keep all Excel library references in editor assembly only.

## 13. Exception policy and error model

Use explicit exception types so import failures are actionable and testable.

Suggested hierarchy:

```csharp
public class ExcelImportException : Exception
{
    public string SourcePath { get; }
}

public sealed class ExcelSheetMappingException : ExcelImportException { }
public sealed class ExcelColumnMappingException : ExcelImportException { }
public sealed class ExcelRowConversionException : ExcelImportException { }
public sealed class ExcelTypeParseException : ExcelImportException { }
public sealed class ExcelSchemaValidationException : ExcelImportException { }
```

Mandatory diagnostic fields:
- source workbook path
- `GameData` type
- sheet name
- sheet index
- row index in Excel coordinates (1-based, including header awareness)
- column name
- column index
- target member name
- target type
- raw cell text/value

Recommended message format:

```text
[ExcelImport] Type parse failed.
Workbook: Data/GameData.xlsx
GameData: MasterGameData
Sheet: MonsterData (index 0)
Row: 12
Column: AttackPower (index 4)
Target Member: MonsterRow.AttackPower
Target Type: System.Int32
Raw Value: "ten"
Reason: Cannot parse Int32 from non-numeric text.
```

Failure rules:
- missing required sheet -> fail import
- duplicate mapped sheets -> fail import
- duplicate headers in one sheet -> fail import
- missing required column -> fail import
- unsupported target type without parser -> fail import
- parser throws -> wrap and rethrow with context
- empty workbook -> fail import

Accumulate schema validation errors where useful before row parsing starts, but stop immediately on row conversion failure to keep diagnostics focused.

## 14. Example domain model

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GameData/MasterGameData")]
public sealed class MasterGameData : ScriptableObject
{
    public List<MonsterRow> MonsterData;

    [Sheet("ItemData")]
    public List<ItemRow> Items;

    [Ignore]
    public List<DebugOnlyRow> DebugRows;
}

[Serializable]
public sealed class MonsterRow
{
    public int Id;
    public string Name;
    public MonsterGrade Grade;
    public Vector3 SpawnOffset;

    [Column("AttackPower")]
    public int Attack;

    [Ignore]
    public string EditorMemo;
}

[Serializable]
public sealed class ItemRow
{
    public int Id;
    public string Name;
    public ItemType Type;
    public RewardRange RewardRange;
}
```

## 15. Example import API

```csharp
public static class GameDataImporter
{
    public static TGameData ImportFromExcel<TGameData>(
        string excelPath,
        string assetPath,
        ExcelImportOptions options = null)
        where TGameData : ScriptableObject
    {
        var service = ExcelImportServiceFactory.CreateDefault(options);
        return service.Import<TGameData>(excelPath, assetPath);
    }
}
```

Usage:

```csharp
var asset = GameDataImporter.ImportFromExcel<MasterGameData>(
    excelPath: "Assets/Data/GameData.xlsx",
    assetPath: "Assets/Data/MasterGameData.asset");
```

## 16. Testing strategy

Prioritize EditMode tests because the importer is editor-first.

Test categories:
- workbook reader returns normalized headers and rows
- `GameData` field discovery respects `[Sheet]` and `[Ignore]`
- row field discovery respects `[Column]` and `[Ignore]`
- built-in parsers handle valid and invalid values
- enum parser handles case variants
- `Vector3` parser handles malformed token counts
- missing sheet/column errors contain exact context
- duplicate headers are rejected before parsing
- asset update replaces lists and preserves unrelated fields
- custom parser registration overrides default parser behavior when intended

Use small fixture workbooks placed under test assets or generate workbook abstractions directly in unit tests for mapping/conversion layers.

## 17. Performance and maintainability notes

### Performance
- editor-only reflection cost is acceptable, but cache metadata and parser lookups
- normalize headers once per sheet
- avoid repeated `FieldInfo.SetValue` boxing overhead where possible in later optimization phase with compiled delegates
- batch asset writes and save once per import

### Serialization
- use fields, not properties, for Unity serialization compatibility
- avoid unsupported collection types in `GameData`
- verify nested custom types are `[Serializable]`

### Editor separation
- keep Excel package and import UI in editor asmdef only
- runtime assembly should contain attributes, models, and parser contracts that row types can reference safely

### Maintainability
- centralize conventions in `ExcelImportOptions`
- do not mix workbook reading and row conversion logic
- keep exception factories/helpers so all messages share one format

## 18. Tower Breaker initial workbook schema

Use the converter MVP against one concrete gameplay asset first so the pipeline is proven on real game content instead of abstract sample rows.

Addressables note:
- keep content references as plain string fields in imported row classes
- do not use `AssetReference` inside the Excel row schema for MVP
- use a consistent `*Key` suffix such as `PrefabKey`, `IconKey`, `PortraitKey`, `VfxKey`, `SfxKey`, `BgmKey`
- let the importer treat these as ordinary strings; runtime validation can verify the key exists in Addressables

Recommended import target:
- `Assets/Scripts/Core/Data/TowerBreakerGameData.cs`
- `Assets/Data/GameData/TowerBreakerGameData.asset`

Recommended first workbook:
- `Assets/Data/Design/TowerBreaker-MVP.xlsx`

Recommended sheet -> row class layout for the first playable MVP:

```text
Floors           -> FloorRow
FloorWaves       -> FloorWaveRow
Enemies          -> EnemyRow
Weapons          -> WeaponRow
RewardTables     -> RewardTableRow
RewardEntries    -> RewardEntryRow
EnhancementCosts -> EnhancementCostRow
RerollCosts      -> RerollCostRow
```

Recommended responsibilities per row type:

- `FloorRow`: floor id, display name, base reward table id, recommended power, battle backdrop key, battle BGM key
- `FloorWaveRow`: floor id, wave index, enemy id, spawn order, spawn time, quantity
- `EnemyRow`: enemy id, archetype, hp, pressure, move speed, attack cadence, armor flag, prefab key, portrait key, hit VFX key, hit SFX key, death VFX key
- `WeaponRow`: weapon id, archetype, rarity, base attack, attack speed, push power, reroll group id, icon key, attack VFX key, hit SFX key, equip SFX key
- `RewardTableRow`: reward table id, guaranteed gold, weapon drop chance, fallback reward id, reward popup SFX key
- `RewardEntryRow`: reward table id, reward id, reward type, target item id, weight, quantity min/max, icon key
- `EnhancementCostRow`: level, gold cost, material cost, attack bonus, pressure bonus
- `RerollCostRow`: rarity or reroll tier, gold cost, roll count, min bonus, max bonus

Why include all eight up front:
- combat and meta both depend on the same workbook contract
- row-class extraction becomes the true schema definition for future content work
- once these sheets import correctly, designers can add floors and tune balance without waiting on code changes

For the first playable MVP, it is acceptable to leave some imported columns temporarily unused in runtime code as long as the schema is stable and validated.

Recommended validation policy for key fields:
- empty optional keys are allowed only for clearly optional visuals or sounds
- required combat and reward visuals should fail validation when their key is empty
- duplicate logical ids are invalid even if Addressables keys differ
- editor validation may check that each required key resolves to at least one Addressables entry before playtesting

## 19. MVP implementation sequence

### Task 1: Create runtime contracts and attributes

**Files:**
- Create: `Assets/Scripts/GameData/Attributes/IgnoreAttribute.cs`
- Create: `Assets/Scripts/GameData/Attributes/ColumnAttribute.cs`
- Create: `Assets/Scripts/GameData/Attributes/SheetAttribute.cs`
- Create: `Assets/Scripts/GameData/Parsing/IExcelValueParser.cs`
- Create: `Assets/Scripts/GameData/Model/WorkbookData.cs`

**Step 1: Write failing tests**

Add tests that validate attribute discovery and type constraints for `ScriptableObject` + `List<T>` + `[Serializable]` row classes.

**Step 2: Run tests to verify failure**

Run EditMode tests for metadata validation.
Expected: missing runtime contracts or validation failures.

**Step 3: Implement minimal contracts**

Add attributes, workbook model DTOs, and parser interfaces with no editor dependencies.

**Step 4: Run tests to verify pass**

Run metadata tests again.
Expected: attribute/type validation tests pass.

### Task 2: Add reflection metadata cache and mapping builders

**Files:**
- Create: `Assets/Scripts/GameData/Editor/Mapping/ReflectionMetadataCache.cs`
- Create: `Assets/Scripts/GameData/Editor/Mapping/GameDataMapBuilder.cs`
- Create: `Assets/Scripts/GameData/Editor/Mapping/RowMapBuilder.cs`
- Test: `Assets/Tests/EditMode/GameData/Mapping/GameDataMappingTests.cs`

**Step 1: Write failing tests**

Cover automatic sheet matching, `[Sheet]`, `[Column]`, `[Ignore]`, duplicate header detection, and invalid field-type rejection.

**Step 2: Run test to verify failure**

Run mapping tests.
Expected: builder/cache types do not exist.

**Step 3: Implement minimal mapping layer**

Build cached metadata extraction plus deterministic mapping validation.

**Step 4: Run tests to verify pass**

Run mapping tests.
Expected: automatic mapping and override rules pass.

### Task 3: Implement parser registry and built-in parsers

**Files:**
- Create: `Assets/Scripts/GameData/Parsing/ExcelParserRegistry.cs`
- Create: `Assets/Scripts/GameData/Parsing/Builtin/*.cs`
- Test: `Assets/Tests/EditMode/GameData/Parsing/ExcelParserTests.cs`

**Step 1: Write failing tests**

Add coverage for primitives, nullable values, enums, bool synonyms, and `Vector3` parsing.

**Step 2: Run tests to verify failure**

Run parser tests.
Expected: registry and built-in parsers missing.

**Step 3: Implement minimal parser pipeline**

Register exact-type and fallback parsers, then emit contextual parse errors.

**Step 4: Run tests to verify pass**

Run parser tests.
Expected: built-in parsing scenarios pass and invalid inputs fail with context.

### Task 4: Implement row object materializer

**Files:**
- Create: `Assets/Scripts/GameData/Editor/Conversion/RowObjectMaterializer.cs`
- Test: `Assets/Tests/EditMode/GameData/Import/RowObjectMaterializerTests.cs`

**Step 1: Write failing tests**

Add tests for row instantiation, field assignment, empty-cell handling, and wrapped parse exceptions.

**Step 2: Run tests to verify failure**

Run row conversion tests.
Expected: materializer missing.

**Step 3: Implement minimal materializer**

Convert neutral sheet rows into typed objects using cached field maps + parser registry.

**Step 4: Run tests to verify pass**

Run row conversion tests.
Expected: typed row creation works and errors include row/column context.

### Task 5: Implement workbook reader abstraction and concrete Excel reader

**Files:**
- Create: `Assets/Scripts/GameData/Editor/Excel/IWorkbookReader.cs`
- Create: `Assets/Scripts/GameData/Editor/Excel/ExcelWorkbookReader.cs`
- Test: `Assets/Tests/EditMode/GameData/Import/ExcelWorkbookReaderTests.cs`

**Step 1: Write failing tests**

Cover header extraction, blank-row trimming, and deterministic sheet ordering from sample workbook fixtures.

**Step 2: Run tests to verify failure**

Run reader tests.
Expected: workbook reader missing or fixture import fails.

**Step 3: Implement minimal reader**

Adapt chosen Excel package into `WorkbookData` and `SheetData`.

**Step 4: Run tests to verify pass**

Run reader tests.
Expected: workbook fixtures normalize correctly.

### Task 6: Implement `GameDataBuilder`, import service, and asset updater

**Files:**
- Create: `Assets/Scripts/GameData/Editor/Conversion/GameDataBuilder.cs`
- Create: `Assets/Scripts/GameData/Editor/Import/ExcelImportService.cs`
- Create: `Assets/Scripts/GameData/Editor/Import/GameDataAssetUpdater.cs`
- Test: `Assets/Tests/EditMode/GameData/Import/ExcelImportServiceTests.cs`

**Step 1: Write failing tests**

Cover end-to-end import into a `ScriptableObject`, list replacement, preserved unrelated fields, and missing-sheet failures.

**Step 2: Run tests to verify failure**

Run import service tests.
Expected: end-to-end import path missing.

**Step 3: Implement minimal orchestration layer**

Orchestrate reader -> mapper -> materializer -> asset update flow.

**Step 4: Run tests to verify pass**

Run import tests.
Expected: asset creation/update works.

### Task 7: Add editor menu and import UX

**Files:**
- Create: `Assets/Scripts/GameData/Editor/UI/GameDataImportMenu.cs`
- Optional: `Assets/Scripts/GameData/Editor/UI/GameDataImportWindow.cs`

**Step 1: Implement minimal menu command**

Provide an editor menu for selecting workbook and target asset path.

**Step 2: Verify manual workflow**

Use a sample workbook to create/update a `GameData` asset inside the editor.

### Task 8: Extension phase after MVP stabilization

**Files:**
- Modify: parser registry, row mapper, import options, editor UI
- Test: add dedicated tests per extension

**Step 1: Add nested prefix parsing**

Support headers like `Stats.Hp` and `Stats.Speed` for nested serializable members.

**Step 2: Add import preset assets**

Store workbook path, target asset path, target type, and parser profile.

**Step 3: Add batch import and optional validation summary**

Allow importing multiple `GameData` assets in one pass.

## 20. Practical operating guidelines for Unity projects

- use one workbook per `GameData` when ownership is clear; avoid giant all-in-one sheets unless the team already prefers them
- keep sheet names stable because they are part of the schema contract
- document allowed text formats for custom parsers so designers do not guess
- avoid formulas that produce locale-sensitive strings for numeric fields
- keep import code deterministic and free of gameplay dependencies
- if row counts grow large, add profiling before optimizing; editor import speed is usually dominated by Excel I/O, not reflection

## 21. Recommended defaults

- exact trimmed name matching for sheets and columns
- unique headers required
- fail on missing sheet/column
- fail on empty cell for non-nullable value type
- custom parsers registered explicitly in importer bootstrap
- editor-only import pipeline with runtime-safe contracts

This plan is sufficient to start implementation immediately while keeping the first version small, testable, and extensible.
