using System;
using System.Reflection;

using NUnit.Framework;

namespace TowerBreak.GameData.Tests
{
    public sealed class TowerBreakerGameDataMappingTests
    {
        private static readonly Assembly RuntimeAssembly = typeof(WorkbookData).Assembly;

        [Test]
        public void TowerBreakerGameData_UsesExpectedSheetNames()
        {
            Type gameDataType = RuntimeAssembly.GetType("TowerBreak.GameData.TowerBreaker.TowerBreakerGameData");
            Assert.That(gameDataType, Is.Not.Null);

            AssertSheetName(gameDataType, "Floors", "Floors");
            AssertSheetName(gameDataType, "FloorWaves", "FloorWaves");
            AssertSheetName(gameDataType, "Enemies", "Enemies");
            AssertSheetName(gameDataType, "Weapons", "Weapons");
            AssertSheetName(gameDataType, "RewardTables", "RewardTables");
            AssertSheetName(gameDataType, "RewardEntries", "RewardEntries");
            AssertSheetName(gameDataType, "EnhancementCosts", "EnhancementCosts");
            AssertSheetName(gameDataType, "RerollCosts", "RerollCosts");
        }

        [Test]
        public void TowerBreakerRows_UseExpectedColumnNames()
        {
            AssertColumnName("TowerBreak.GameData.TowerBreaker.FloorRow", "Id", "Id");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.FloorRow", "DisplayName", "DisplayName");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.FloorRow", "RewardTableId", "RewardTableId");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.FloorRow", "RecommendedPower", "RecommendedPower");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.FloorRow", "BattleBackdropKey", "BattleBackdropKey");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.FloorRow", "BattleBgmKey", "BattleBgmKey");

            AssertColumnName("TowerBreak.GameData.TowerBreaker.FloorWaveRow", "FloorId", "FloorId");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.FloorWaveRow", "WaveIndex", "WaveIndex");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.FloorWaveRow", "EnemyId", "EnemyId");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.FloorWaveRow", "SpawnOrder", "SpawnOrder");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.FloorWaveRow", "SpawnTime", "SpawnTime");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.FloorWaveRow", "Quantity", "Quantity");

            AssertColumnName("TowerBreak.GameData.TowerBreaker.EnemyRow", "Id", "Id");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.EnemyRow", "Archetype", "Archetype");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.EnemyRow", "Health", "Health");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.EnemyRow", "Pressure", "Pressure");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.EnemyRow", "MoveSpeed", "MoveSpeed");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.EnemyRow", "AttackCadence", "AttackCadence");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.EnemyRow", "IsArmored", "IsArmored");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.EnemyRow", "PrefabKey", "PrefabKey");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.EnemyRow", "PortraitKey", "PortraitKey");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.EnemyRow", "HitVfxKey", "HitVfxKey");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.EnemyRow", "HitSfxKey", "HitSfxKey");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.EnemyRow", "DeathVfxKey", "DeathVfxKey");

            AssertColumnName("TowerBreak.GameData.TowerBreaker.WeaponRow", "Id", "Id");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.WeaponRow", "Archetype", "Archetype");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.WeaponRow", "Rarity", "Rarity");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.WeaponRow", "BaseAttack", "BaseAttack");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.WeaponRow", "AttackSpeed", "AttackSpeed");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.WeaponRow", "PushPower", "PushPower");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.WeaponRow", "RerollGroupId", "RerollGroupId");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.WeaponRow", "IconKey", "IconKey");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.WeaponRow", "AttackVfxKey", "AttackVfxKey");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.WeaponRow", "HitSfxKey", "HitSfxKey");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.WeaponRow", "EquipSfxKey", "EquipSfxKey");

            AssertColumnName("TowerBreak.GameData.TowerBreaker.RewardTableRow", "Id", "Id");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.RewardTableRow", "GuaranteedGold", "GuaranteedGold");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.RewardTableRow", "WeaponDropChance", "WeaponDropChance");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.RewardTableRow", "FallbackRewardId", "FallbackRewardId");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.RewardTableRow", "RewardPopupSfxKey", "RewardPopupSfxKey");

            AssertColumnName("TowerBreak.GameData.TowerBreaker.RewardEntryRow", "RewardTableId", "RewardTableId");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.RewardEntryRow", "RewardId", "RewardId");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.RewardEntryRow", "RewardType", "RewardType");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.RewardEntryRow", "TargetItemId", "TargetItemId");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.RewardEntryRow", "Weight", "Weight");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.RewardEntryRow", "QuantityMin", "QuantityMin");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.RewardEntryRow", "QuantityMax", "QuantityMax");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.RewardEntryRow", "IconKey", "IconKey");

            AssertColumnName("TowerBreak.GameData.TowerBreaker.EnhancementCostRow", "Level", "Level");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.EnhancementCostRow", "GoldCost", "GoldCost");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.EnhancementCostRow", "MaterialCost", "MaterialCost");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.EnhancementCostRow", "AttackBonus", "AttackBonus");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.EnhancementCostRow", "PressureBonus", "PressureBonus");

            AssertColumnName("TowerBreak.GameData.TowerBreaker.RerollCostRow", "Rarity", "Rarity");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.RerollCostRow", "GoldCost", "GoldCost");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.RerollCostRow", "RollCount", "RollCount");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.RerollCostRow", "MinBonus", "MinBonus");
            AssertColumnName("TowerBreak.GameData.TowerBreaker.RerollCostRow", "MaxBonus", "MaxBonus");
        }

        private static void AssertSheetName(Type declaringType, string fieldName, string expectedName)
        {
            FieldInfo field = declaringType.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public);
            Assert.That(field, Is.Not.Null);

            SheetAttribute attribute = field.GetCustomAttribute<SheetAttribute>();
            Assert.That(attribute, Is.Not.Null, $"Field '{fieldName}' should define a SheetAttribute.");
            Assert.That(attribute.Name, Is.EqualTo(expectedName));
        }

        private static void AssertColumnName(string typeName, string fieldName, string expectedName)
        {
            Type type = RuntimeAssembly.GetType(typeName);
            Assert.That(type, Is.Not.Null);

            FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public);
            Assert.That(field, Is.Not.Null);

            ColumnAttribute attribute = field.GetCustomAttribute<ColumnAttribute>();
            Assert.That(attribute, Is.Not.Null, $"Field '{type.FullName}.{fieldName}' should define a ColumnAttribute.");
            Assert.That(attribute.Name, Is.EqualTo(expectedName));
        }
    }
}
