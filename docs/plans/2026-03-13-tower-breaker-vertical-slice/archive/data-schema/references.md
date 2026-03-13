# Data Schema References

## Reference Documents

- `docs/plans/2026-03-13-tower-breaker-vertical-slice/00-overview.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/01-foundation-and-bootstrap.md`
- `docs/plans/2026-03-13-unity-excel-gamedata-converter.md`
- `docs/plans/2026-03-13-implementation-workflow.md`
- `docs/Direction/2026-03-13-tower-breaker-storyboard.md`
- `docs/Direction/기획서.md`

## Existing Code References

- `Assets/Scripts/GameData/TowerBreak.GameData.asmdef`
- `Assets/Scripts/GameData/Attributes/SheetAttribute.cs`
- `Assets/Scripts/GameData/Attributes/ColumnAttribute.cs`
- `Assets/Scripts/GameData/Attributes/IgnoreAttribute.cs`
- `Assets/Scripts/GameData/Model/WorkbookData.cs`
- `Assets/Scripts/GameData/Model/SheetData.cs`
- `Assets/Scripts/GameData/Model/CellValue.cs`
- `Assets/Scripts/GameData/Parsing/ExcelParseContext.cs`
- `Assets/Scripts/GameData/Parsing/ExcelParserRegistry.cs`

## Existing Test References

- `Assets/Tests/EditMode/GameData/ExcelImportScaffoldingTests.cs`
- `Assets/Tests/EditMode/GameData/TowerBreak.GameData.Tests.asmdef`

## New Work In This Milestone

- `Assets/Scripts/GameData/TowerBreaker/TowerBreakerGameData.cs`
- `Assets/Scripts/GameData/TowerBreaker/FloorRow.cs`
- `Assets/Scripts/GameData/TowerBreaker/FloorWaveRow.cs`
- `Assets/Scripts/GameData/TowerBreaker/EnemyRow.cs`
- `Assets/Scripts/GameData/TowerBreaker/WeaponRow.cs`
- `Assets/Scripts/GameData/TowerBreaker/RewardTableRow.cs`
- `Assets/Scripts/GameData/TowerBreaker/RewardEntryRow.cs`
- `Assets/Scripts/GameData/TowerBreaker/EnhancementCostRow.cs`
- `Assets/Scripts/GameData/TowerBreaker/RerollCostRow.cs`
- `Assets/Scripts/GameData/TowerBreaker/EnemyArchetype.cs`
- `Assets/Scripts/GameData/TowerBreaker/WeaponArchetype.cs`
- `Assets/Scripts/GameData/TowerBreaker/WeaponRarity.cs`
- `Assets/Scripts/GameData/TowerBreaker/RewardType.cs`
- `Assets/Tests/EditMode/GameData/TowerBreakerGameDataSchemaTests.cs`
- `Assets/Tests/EditMode/GameData/TowerBreakerGameDataMappingTests.cs`
- `Assets/Tests/EditMode/GameData/TowerBreakerWorkbookReadTests.cs`
- `Assets/Data/Design/TowerBreaker-MVP.xlsx`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/archive/data-schema/workbook-contract.md`
