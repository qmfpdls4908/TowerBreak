# Data Schema Checklist

## Reference Documents

- `docs/plans/2026-03-13-tower-breaker-vertical-slice/00-overview.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/01-foundation-and-bootstrap.md`
- `docs/plans/2026-03-13-unity-excel-gamedata-converter.md`
- `docs/plans/2026-03-13-implementation-workflow.md`

## Checklist

- Done: create milestone folder and seed milestone docs
- Done: research existing `GameData` runtime/test assembly boundaries
- Done: add `TowerBreakerGameData` import target
- Done: add first-pass gameplay row classes
- Done: add enum types required by the schema
- Done: add `TowerBreakerGameDataSchemaTests` reflection-based validation fixture
- Done: verify Unity regenerated `.csproj` files including the new runtime/test source files
- Done: rerun Unity in batchmode without editor lock
- Done: identify the Unity CLI `-quit` issue that prevented XML generation
- Done: run targeted schema fixture with the corrected no-`-quit` command
- Done: record actual pass evidence for the fixture
- Done: finalize workbook headers and add import mapping attributes
- Done: add targeted mapping tests for sheet/header attributes
- Done: record actual pass evidence for mapping tests
- Done: write the workbook header contract document
- Done: create the checked-in workbook file matching the finalized headers
- Done: populate the workbook with initial placeholder rows for floors `1-3`, enemies, weapons, rewards, enhancement costs, and reroll costs
- Done: verify the current Excel reader can load the workbook and see the expected sheets, headers, and sample rows

## Verification

- Passed command: `dotnet build "TowerBreak.GameData.csproj"`
- Failed command: `dotnet build "TowerBreak.GameData.Tests.csproj"`
- Failure: `CS2012` file lock on `obj\Debug\TowerBreak.GameData.dll`
- Observed: `TowerBreak.GameData.csproj` now includes `Assets\Scripts\GameData\TowerBreaker\TowerBreakerGameData.cs`
- Observed: `TowerBreak.GameData.Tests.csproj` now includes `Assets\Tests\EditMode\GameData\TowerBreakerGameDataSchemaTests.cs`
- Attempted command: `dotnet test "TowerBreak.GameData.Tests.csproj" --filter "FullyQualifiedName~TowerBreakerGameDataSchemaTests" -v normal`
- Result: no usable Unity test execution evidence; command ends after restore/build output
- Root cause found: Unity 6 test CLI with `-runTests` plus `-quit` prevented result XML generation
- Passed command: `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "C:\Users\admin\Desktop\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.GameData.Tests.TowerBreakerGameDataSchemaTests" -logFile "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\towerbreaker-schema-noquit.log" -testResults "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\towerbreaker-schema-noquit.xml"`
- Evidence: `Logs\towerbreaker-schema-noquit.xml` shows `total="3" passed="3" failed="0"`
- Passed command: `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "C:\Users\admin\Desktop\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.GameData.Tests.TowerBreakerGameDataMappingTests" -logFile "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\towerbreaker-mapping-green.log" -testResults "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\towerbreaker-mapping-green.xml"`
- Evidence: `Logs\towerbreaker-mapping-green.xml` shows `total="2" passed="2" failed="0"`
- Passed command: `dotnet build "TowerBreak.GameData.Tests.csproj"`
- Passed check: `Assets/Data/Design/TowerBreaker-MVP.xlsx` exists
- Passed check: generated workbook contains eight worksheet XML parts and workbook sheet names matching the contract
- Passed command: `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "C:\Users\admin\Desktop\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.GameData.Tests.TowerBreakerWorkbookReadTests" -logFile "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\towerbreaker-workbook-read.log" -testResults "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\towerbreaker-workbook-read.xml"`
- Evidence: `Logs\towerbreaker-workbook-read.xml` shows `total="1" passed="1" failed="0"`

## Manual Follow-Ups

- Use the corrected no-`-quit` Unity command for future targeted schema verification
- Keep workbook sample content aligned with future schema changes
- If full row-to-asset import becomes necessary, add a later milestone for `WorkbookData -> TowerBreakerGameData.asset` import wiring
