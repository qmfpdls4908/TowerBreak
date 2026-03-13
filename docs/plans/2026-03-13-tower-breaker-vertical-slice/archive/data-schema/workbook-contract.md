# TowerBreaker MVP Workbook Contract

## Reference Documents

- `docs/plans/2026-03-13-tower-breaker-vertical-slice/00-overview.md`
- `docs/plans/2026-03-13-unity-excel-gamedata-converter.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/archive/data-schema/README.md`

## Workbook Rules

- Workbook target: `Assets/Data/Design/TowerBreaker-MVP.xlsx`
- Sheet names must match the `SheetAttribute` names exactly
- Header names must match the `ColumnAttribute` names exactly
- The first row of each sheet is the header row
- Enum columns use enum member names, not display labels
- Addressables references are stored as plain string keys

## Sheet Contracts

### Floors

Headers:
- `Id`
- `DisplayName`
- `RewardTableId`
- `RecommendedPower`
- `BattleBackdropKey`
- `BattleBgmKey`

### FloorWaves

Headers:
- `FloorId`
- `WaveIndex`
- `EnemyId`
- `SpawnOrder`
- `SpawnTime`
- `Quantity`

### Enemies

Headers:
- `Id`
- `Archetype`
- `Health`
- `Pressure`
- `MoveSpeed`
- `AttackCadence`
- `IsArmored`
- `PrefabKey`
- `PortraitKey`
- `HitVfxKey`
- `HitSfxKey`
- `DeathVfxKey`

### Weapons

Headers:
- `Id`
- `Archetype`
- `Rarity`
- `BaseAttack`
- `AttackSpeed`
- `PushPower`
- `RerollGroupId`
- `IconKey`
- `AttackVfxKey`
- `HitSfxKey`
- `EquipSfxKey`

### RewardTables

Headers:
- `Id`
- `GuaranteedGold`
- `WeaponDropChance`
- `FallbackRewardId`
- `RewardPopupSfxKey`

### RewardEntries

Headers:
- `RewardTableId`
- `RewardId`
- `RewardType`
- `TargetItemId`
- `Weight`
- `QuantityMin`
- `QuantityMax`
- `IconKey`

### EnhancementCosts

Headers:
- `Level`
- `GoldCost`
- `MaterialCost`
- `AttackBonus`
- `PressureBonus`

### RerollCosts

Headers:
- `Rarity`
- `GoldCost`
- `RollCount`
- `MinBonus`
- `MaxBonus`

## Initial Enum Tokens

### EnemyArchetype

- `BasicMelee`
- `ArmoredPusher`
- `Support`

### WeaponArchetype

- `Claw`
- `Lance`

### WeaponRarity

- `Common`
- `Rare`
- `Epic`
- `Legendary`

### RewardType

- `Gold`
- `Weapon`
- `Material`
