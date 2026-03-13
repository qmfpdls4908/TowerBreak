# Data Schema Milestone

## Goal

Define and verify the first playable `TowerBreakerGameData` schema so gameplay tuning can move into Excel before the combat/meta MVP is implemented.

## Reference Documents

- `docs/plans/2026-03-13-tower-breaker-vertical-slice/00-overview.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/01-foundation-and-bootstrap.md`
- `docs/plans/2026-03-13-unity-excel-gamedata-converter.md`
- `docs/plans/2026-03-13-implementation-workflow.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/archive/data-schema/workbook-contract.md`

## Code And Tests Researched

- `Assets/Scripts/GameData/TowerBreak.GameData.asmdef`
- `Assets/Scripts/GameData/Attributes/SheetAttribute.cs`
- `Assets/Scripts/GameData/Attributes/ColumnAttribute.cs`
- `Assets/Scripts/GameData/Attributes/IgnoreAttribute.cs`
- `Assets/Scripts/GameData/Model/WorkbookData.cs`
- `Assets/Tests/EditMode/GameData/ExcelImportScaffoldingTests.cs`
- `Assets/Tests/EditMode/GameData/TowerBreak.GameData.Tests.asmdef`

## Research Findings

- The existing `TowerBreak.GameData` runtime assembly already holds import attributes and workbook/runtime-safe DTOs, so the first gameplay schema can live there without creating a new asmdef yet.
- The current test assembly already references both `TowerBreak.GameData` and `TowerBreak.GameData.Editor`, which is enough for schema validation tests.
- The current Excel import scaffolding is still minimal, so this milestone should focus on row class contracts and import target shape first, not the full importer pipeline.
- Addressables is already available through `Packages/manifest.json`, but the schema should store only string keys and avoid direct object references.

## Implementation Guardrails

- Keep gameplay content references as string `*Key` fields only.
- Keep schema code in runtime-safe `Assets/Scripts/GameData/`.
- Do not move importer/editor functionality into runtime assemblies.
- Prefer failing tests first when Unity test execution is available.
- If Unity is open and CLI tests are blocked, record the blocked status instead of pretending verification succeeded.

## Checklist

- Done: create `TowerBreakerGameData` import target under runtime `GameData`
- Done: add first-pass row classes for floors, waves, enemies, weapons, rewards, enhancement costs, and reroll costs
- Done: add initial enum types required by the schema
- Done: add schema validation test scaffold for the new gameplay data shape
- Done: verify the schema through targeted Unity EditMode execution
- Done: add sheet/column mapping attributes using explicit workbook sheet/header names
- Done: write the workbook header contract for the first MVP sheets
- Done: create the checked-in workbook file matching the documented headers

## Verification

- Passed: `dotnet build "TowerBreak.GameData.csproj"`
- Attempted: `dotnet build "TowerBreak.GameData.Tests.csproj"`
- Actual status: failed earlier due to file lock on `obj\Debug\TowerBreak.GameData.dll`
- Rechecked: `TowerBreak.GameData.csproj` and `TowerBreak.GameData.Tests.csproj` now both include the new runtime/test files
- Attempted: `dotnet test "TowerBreak.GameData.Tests.csproj" --filter "FullyQualifiedName~TowerBreakerGameDataSchemaTests" -v normal`
- Actual status: the command still stops after restore/build and does not provide usable Unity test execution evidence for the new fixture
- Root cause found: Unity 6 test CLI with `-runTests` plus `-quit` can terminate before results are written
- Passed: `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "C:\Users\admin\Desktop\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.GameData.Tests.TowerBreakerGameDataSchemaTests" -logFile "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\towerbreaker-schema-noquit.log" -testResults "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\towerbreaker-schema-noquit.xml"`
- Evidence: `Logs\towerbreaker-schema-noquit.xml` shows `total="3" passed="3" failed="0"`
- Passed: `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "C:\Users\admin\Desktop\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.GameData.Tests.TowerBreakerGameDataMappingTests" -logFile "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\towerbreaker-mapping-green.log" -testResults "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\towerbreaker-mapping-green.xml"`
- Evidence: `Logs\towerbreaker-mapping-green.xml` shows `total="2" passed="2" failed="0"`
- Passed: `dotnet build "TowerBreak.GameData.Tests.csproj"`
- Passed: workbook file exists at `Assets/Data/Design/TowerBreaker-MVP.xlsx`
- Passed: workbook contains the expected eight worksheet parts and matching workbook sheet names

## Manual Follow-Ups

- Use the corrected no-`-quit` Unity command for future targeted test runs in this repo
- If the importer gains stronger validation later, keep this workbook in sync with enum tokens, required headers, and row ids
- The current repo verifies workbook reading into `WorkbookData`; a later milestone can add full row-to-asset import if needed

## Sample Content Added

- Floors `1-3` were added with escalating recommended power and unique backdrop/BGM keys
- Two enemy archetypes were added: `BasicMelee`, `ArmoredPusher`
- Two common starter weapons were added: `Claw`, `Lance`
- Reward tables and reward entries were seeded to drop those weapons across the first three floors
- Enhancement and reroll cost placeholder rows were added so the meta loop has initial numeric tables to read

## Import Verification Added

- `TowerBreaker-MVP.xlsx` is now verified through the current `ExcelWorkbookReader`
- The reader successfully loads all eight sheets into `WorkbookData`
- The verification confirms header shape and sample row availability for `Floors`, `Enemies`, and `RewardEntries`
