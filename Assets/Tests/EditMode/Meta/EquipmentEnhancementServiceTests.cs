using System;
using System.Collections.Generic;

using NUnit.Framework;

using TowerBreak.GameData.TowerBreaker;
using TowerBreak.Meta.Equipment;
using TowerBreak.Meta.State;

namespace TowerBreak.Meta.Tests
{
    public sealed class EquipmentEnhancementServiceTests
    {
        // ---- helpers ----

        private static WeaponRow MakeWeapon(int id = 1, int baseAttack = 10)
        {
            return new WeaponRow { Id = id, BaseAttack = baseAttack };
        }

        private static EnhancementCostRow MakeEnhancementCostRow(
            int level = 1,
            int goldCost = 100,
            int attackBonus = 10)
        {
            return new EnhancementCostRow
            {
                Level = level,
                GoldCost = goldCost,
                AttackBonus = attackBonus
            };
        }

        // ---- FindEnhancementCost ----

        [Test]
        public void FindEnhancementCost_ExistingLevel_ReturnsCorrectRow()
        {
            List<EnhancementCostRow> rows = new()
            {
                MakeEnhancementCostRow(level: 1, goldCost: 100),
                MakeEnhancementCostRow(level: 2, goldCost: 200),
                MakeEnhancementCostRow(level: 3, goldCost: 300)
            };

            EnhancementCostRow row = EquipmentEnhancementService.FindEnhancementCost(2, rows);

            Assert.That(row, Is.Not.Null);
            Assert.That(row.Level, Is.EqualTo(2));
            Assert.That(row.GoldCost, Is.EqualTo(200));
        }

        [Test]
        public void FindEnhancementCost_NonExistingLevel_ReturnsNull()
        {
            List<EnhancementCostRow> rows = new()
            {
                MakeEnhancementCostRow(level: 1)
            };

            EnhancementCostRow row = EquipmentEnhancementService.FindEnhancementCost(99, rows);

            Assert.That(row, Is.Null);
        }

        [Test]
        public void FindEnhancementCost_NullList_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                EquipmentEnhancementService.FindEnhancementCost(1, null));
        }

        // ---- GetCurrentAttackPower ----

        [Test]
        public void GetCurrentAttackPower_LevelZero_ReturnsBaseAttack()
        {
            WeaponRow weapon = MakeWeapon(baseAttack: 50);

            int attackPower = EquipmentEnhancementService.GetCurrentAttackPower(weapon, 0);

            Assert.That(attackPower, Is.EqualTo(50));
        }

        [Test]
        public void GetCurrentAttackPower_LevelOne_ReturnsBasePlusBonus()
        {
            WeaponRow weapon = MakeWeapon(baseAttack: 50);

            int attackPower = EquipmentEnhancementService.GetCurrentAttackPower(weapon, 1);

            Assert.That(attackPower, Is.EqualTo(60));
        }

        [Test]
        public void GetCurrentAttackPower_LevelThree_ReturnsBasePlusCumulativeBonus()
        {
            WeaponRow weapon = MakeWeapon(baseAttack: 50);

            int attackPower = EquipmentEnhancementService.GetCurrentAttackPower(weapon, 3);

            // Base 50 + (10*1 + 10*2 + 10*3) = 50 + 60 = 110
            Assert.That(attackPower, Is.EqualTo(110));
        }

        [Test]
        public void GetCurrentAttackPower_NullWeapon_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                EquipmentEnhancementService.GetCurrentAttackPower(null, 1));
        }

        // ---- TryEnhance null guards ----

        [Test]
        public void TryEnhance_NullEquipment_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                EquipmentEnhancementService.TryEnhance(
                    null,
                    MakeWeapon(),
                    new List<EnhancementCostRow> { MakeEnhancementCostRow() },
                    new PlayerWalletState(100)));
        }

        [Test]
        public void TryEnhance_NullWeaponRow_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                EquipmentEnhancementService.TryEnhance(
                    new OwnedEquipment(1, 1),
                    null,
                    new List<EnhancementCostRow> { MakeEnhancementCostRow() },
                    new PlayerWalletState(100)));
        }

        [Test]
        public void TryEnhance_NullCostRows_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                EquipmentEnhancementService.TryEnhance(
                    new OwnedEquipment(1, 1),
                    MakeWeapon(),
                    null,
                    new PlayerWalletState(100)));
        }

        [Test]
        public void TryEnhance_NullWallet_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                EquipmentEnhancementService.TryEnhance(
                    new OwnedEquipment(1, 1),
                    MakeWeapon(),
                    new List<EnhancementCostRow> { MakeEnhancementCostRow() },
                    null));
        }

        // ---- TryEnhance success cases ----

        [Test]
        public void TryEnhance_SufficientGold_ReturnsTrue()
        {
            PlayerWalletState wallet = new(200);
            OwnedEquipment equipment = new(1, 1);
            List<EnhancementCostRow> rows = new() { MakeEnhancementCostRow(level: 1, goldCost: 100) };

            bool success = EquipmentEnhancementService.TryEnhance(equipment, MakeWeapon(), rows, wallet);

            Assert.That(success, Is.True);
        }

        [Test]
        public void TryEnhance_SufficientGold_IncrementsLevel()
        {
            PlayerWalletState wallet = new(200);
            OwnedEquipment equipment = new(1, 1, enhancementLevel: 0);
            List<EnhancementCostRow> rows = new() { MakeEnhancementCostRow(level: 1, goldCost: 100) };

            EquipmentEnhancementService.TryEnhance(equipment, MakeWeapon(), rows, wallet);

            Assert.That(equipment.EnhancementLevel, Is.EqualTo(1));
        }

        [Test]
        public void TryEnhance_SufficientGold_DeductsGold()
        {
            PlayerWalletState wallet = new(200);
            OwnedEquipment equipment = new(1, 1);
            List<EnhancementCostRow> rows = new() { MakeEnhancementCostRow(level: 1, goldCost: 100) };

            EquipmentEnhancementService.TryEnhance(equipment, MakeWeapon(), rows, wallet);

            Assert.That(wallet.Gold, Is.EqualTo(100));
        }

        [Test]
        public void TryEnhance_MultipleLevels_WorksCorrectly()
        {
            PlayerWalletState wallet = new(1000);
            OwnedEquipment equipment = new(1, 1, enhancementLevel: 0);
            List<EnhancementCostRow> rows = new()
            {
                MakeEnhancementCostRow(level: 1, goldCost: 100),
                MakeEnhancementCostRow(level: 2, goldCost: 200),
                MakeEnhancementCostRow(level: 3, goldCost: 300)
            };

            EquipmentEnhancementService.TryEnhance(equipment, MakeWeapon(), rows, wallet);
            EquipmentEnhancementService.TryEnhance(equipment, MakeWeapon(), rows, wallet);
            EquipmentEnhancementService.TryEnhance(equipment, MakeWeapon(), rows, wallet);

            Assert.That(equipment.EnhancementLevel, Is.EqualTo(3));
            Assert.That(wallet.Gold, Is.EqualTo(400)); // 1000 - 100 - 200 - 300
        }

        // ---- TryEnhance failure cases ----

        [Test]
        public void TryEnhance_InsufficientGold_ReturnsFalse()
        {
            PlayerWalletState wallet = new(50);
            OwnedEquipment equipment = new(1, 1);
            List<EnhancementCostRow> rows = new() { MakeEnhancementCostRow(level: 1, goldCost: 100) };

            bool success = EquipmentEnhancementService.TryEnhance(equipment, MakeWeapon(), rows, wallet);

            Assert.That(success, Is.False);
        }

        [Test]
        public void TryEnhance_InsufficientGold_DoesNotChangeLevel()
        {
            PlayerWalletState wallet = new(50);
            OwnedEquipment equipment = new(1, 1, enhancementLevel: 0);
            List<EnhancementCostRow> rows = new() { MakeEnhancementCostRow(level: 1, goldCost: 100) };

            EquipmentEnhancementService.TryEnhance(equipment, MakeWeapon(), rows, wallet);

            Assert.That(equipment.EnhancementLevel, Is.EqualTo(0));
        }

        [Test]
        public void TryEnhance_InsufficientGold_DoesNotDeductGold()
        {
            PlayerWalletState wallet = new(50);
            OwnedEquipment equipment = new(1, 1);
            List<EnhancementCostRow> rows = new() { MakeEnhancementCostRow(level: 1, goldCost: 100) };

            EquipmentEnhancementService.TryEnhance(equipment, MakeWeapon(), rows, wallet);

            Assert.That(wallet.Gold, Is.EqualTo(50));
        }

        [Test]
        public void TryEnhance_NoCostRowForNextLevel_ReturnsFalse()
        {
            PlayerWalletState wallet = new(1000);
            OwnedEquipment equipment = new(1, 1, enhancementLevel: 5);
            List<EnhancementCostRow> rows = new()
            {
                MakeEnhancementCostRow(level: 1, goldCost: 100),
                MakeEnhancementCostRow(level: 2, goldCost: 200)
            };

            bool success = EquipmentEnhancementService.TryEnhance(equipment, MakeWeapon(), rows, wallet);

            Assert.That(success, Is.False);
        }

        [Test]
        public void TryEnhance_NoCostRowForNextLevel_DoesNotChangeLevel()
        {
            PlayerWalletState wallet = new(1000);
            OwnedEquipment equipment = new(1, 1, enhancementLevel: 5);
            List<EnhancementCostRow> rows = new()
            {
                MakeEnhancementCostRow(level: 1),
                MakeEnhancementCostRow(level: 2)
            };

            EquipmentEnhancementService.TryEnhance(equipment, MakeWeapon(), rows, wallet);

            Assert.That(equipment.EnhancementLevel, Is.EqualTo(5));
        }
    }
}
