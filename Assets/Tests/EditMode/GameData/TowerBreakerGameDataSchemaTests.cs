using System;
using System.Collections.Generic;
using System.Reflection;

using NUnit.Framework;

using UnityEngine;

namespace TowerBreak.GameData.Tests
{
    public sealed class TowerBreakerGameDataSchemaTests
    {
        private static readonly Assembly RuntimeAssembly = typeof(WorkbookData).Assembly;

        [Test]
        public void TowerBreakerGameData_ExposesExpectedImportLists()
        {
            Type gameDataType = GetRequiredType("TowerBreak.GameData.TowerBreaker.TowerBreakerGameData");

            Assert.That(typeof(ScriptableObject).IsAssignableFrom(gameDataType), Is.True);

            AssertListField(gameDataType, "Floors", "TowerBreak.GameData.TowerBreaker.FloorRow");
            AssertListField(gameDataType, "FloorWaves", "TowerBreak.GameData.TowerBreaker.FloorWaveRow");
            AssertListField(gameDataType, "Enemies", "TowerBreak.GameData.TowerBreaker.EnemyRow");
            AssertListField(gameDataType, "Weapons", "TowerBreak.GameData.TowerBreaker.WeaponRow");
            AssertListField(gameDataType, "RewardTables", "TowerBreak.GameData.TowerBreaker.RewardTableRow");
            AssertListField(gameDataType, "RewardEntries", "TowerBreak.GameData.TowerBreaker.RewardEntryRow");
            AssertListField(gameDataType, "EnhancementCosts", "TowerBreak.GameData.TowerBreaker.EnhancementCostRow");
            AssertListField(gameDataType, "RerollCosts", "TowerBreak.GameData.TowerBreaker.RerollCostRow");
        }

        [Test]
        public void TowerBreakerRows_AreSerializable()
        {
            AssertSerializableType("TowerBreak.GameData.TowerBreaker.FloorRow");
            AssertSerializableType("TowerBreak.GameData.TowerBreaker.FloorWaveRow");
            AssertSerializableType("TowerBreak.GameData.TowerBreaker.EnemyRow");
            AssertSerializableType("TowerBreak.GameData.TowerBreaker.WeaponRow");
            AssertSerializableType("TowerBreak.GameData.TowerBreaker.RewardTableRow");
            AssertSerializableType("TowerBreak.GameData.TowerBreaker.RewardEntryRow");
            AssertSerializableType("TowerBreak.GameData.TowerBreaker.EnhancementCostRow");
            AssertSerializableType("TowerBreak.GameData.TowerBreaker.RerollCostRow");
        }

        [Test]
        public void AddressableDrivenRows_ExposeExpectedKeyFields()
        {
            AssertStringField("TowerBreak.GameData.TowerBreaker.FloorRow", "BattleBackdropKey");
            AssertStringField("TowerBreak.GameData.TowerBreaker.FloorRow", "BattleBgmKey");
            AssertStringField("TowerBreak.GameData.TowerBreaker.EnemyRow", "PrefabKey");
            AssertStringField("TowerBreak.GameData.TowerBreaker.EnemyRow", "PortraitKey");
            AssertStringField("TowerBreak.GameData.TowerBreaker.EnemyRow", "HitVfxKey");
            AssertStringField("TowerBreak.GameData.TowerBreaker.EnemyRow", "HitSfxKey");
            AssertStringField("TowerBreak.GameData.TowerBreaker.EnemyRow", "DeathVfxKey");
            AssertStringField("TowerBreak.GameData.TowerBreaker.WeaponRow", "IconKey");
            AssertStringField("TowerBreak.GameData.TowerBreaker.WeaponRow", "AttackVfxKey");
            AssertStringField("TowerBreak.GameData.TowerBreaker.WeaponRow", "HitSfxKey");
            AssertStringField("TowerBreak.GameData.TowerBreaker.WeaponRow", "EquipSfxKey");
            AssertStringField("TowerBreak.GameData.TowerBreaker.RewardTableRow", "RewardPopupSfxKey");
            AssertStringField("TowerBreak.GameData.TowerBreaker.RewardEntryRow", "IconKey");
        }

        private static Type GetRequiredType(string fullName)
        {
            Type type = RuntimeAssembly.GetType(fullName);
            Assert.That(type, Is.Not.Null, $"Type '{fullName}' should exist.");
            return type;
        }

        private static void AssertListField(Type declaringType, string fieldName, string rowTypeName)
        {
            FieldInfo field = declaringType.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public);
            Assert.That(field, Is.Not.Null, $"Field '{declaringType.FullName}.{fieldName}' should exist.");

            Type rowType = GetRequiredType(rowTypeName);
            Type expectedFieldType = typeof(List<>).MakeGenericType(rowType);
            Assert.That(field.FieldType, Is.EqualTo(expectedFieldType));
        }

        private static void AssertSerializableType(string fullName)
        {
            Type type = GetRequiredType(fullName);
            Assert.That(type.IsDefined(typeof(SerializableAttribute), false), Is.True, $"Type '{fullName}' should be serializable.");
        }

        private static void AssertStringField(string typeName, string fieldName)
        {
            Type type = GetRequiredType(typeName);
            FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public);
            Assert.That(field, Is.Not.Null, $"Field '{type.FullName}.{fieldName}' should exist.");
            Assert.That(field.FieldType, Is.EqualTo(typeof(string)));
        }
    }
}
