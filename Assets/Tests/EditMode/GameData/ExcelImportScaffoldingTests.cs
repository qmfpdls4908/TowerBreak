using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TowerBreak.GameData.Editor;
using UnityEngine;

namespace TowerBreak.GameData.Tests
{
    public sealed class ExcelImportScaffoldingTests
    {
        [Test]
        public void SheetAndColumnAttributes_ExposeConfiguredNames()
        {
            FieldInfo itemsField = typeof(TestGameData).GetField(nameof(TestGameData.Items));
            FieldInfo attackField = typeof(TestRow).GetField(nameof(TestRow.Attack));

            SheetAttribute sheetAttribute = itemsField?.GetCustomAttribute<SheetAttribute>();
            ColumnAttribute columnAttribute = attackField?.GetCustomAttribute<ColumnAttribute>();

            Assert.That(sheetAttribute, Is.Not.Null);
            Assert.That(sheetAttribute.Name, Is.EqualTo("ItemData"));
            Assert.That(columnAttribute, Is.Not.Null);
            Assert.That(columnAttribute.Name, Is.EqualTo("AttackPower"));
        }

        [Test]
        public void ExcelDependencyInfo_DeclaresExpectedDllNames()
        {
            Assert.That(ExcelDependencyInfo.RequiredDllNames, Is.EquivalentTo(new[]
            {
                "ExcelDataReader.dll",
                "ExcelDataReader.DataSet.dll",
                "System.Text.Encoding.CodePages.dll"
            }));
        }

        private sealed class TestGameData : ScriptableObject
        {
            [Sheet("ItemData")]
            public List<TestRow> Items;
        }

        [Serializable]
        private sealed class TestRow
        {
            [Column("AttackPower")]
            public int Attack;
        }
    }
}
