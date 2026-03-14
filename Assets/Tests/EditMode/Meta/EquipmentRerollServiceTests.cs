using System;
using System.Collections.Generic;

using NUnit.Framework;

using TowerBreak.GameData.TowerBreaker;
using TowerBreak.Meta.Equipment;
using TowerBreak.Meta.State;

namespace TowerBreak.Meta.Tests
{
    public sealed class EquipmentRerollServiceTests
    {
        // ---- helpers ----

        private static WeaponRow MakeWeapon(int id = 1, WeaponRarity rarity = WeaponRarity.Common)
        {
            return new WeaponRow { Id = id, Rarity = rarity, BaseAttack = 10, AttackSpeed = 1.0f, PushPower = 1.0f };
        }

        private static RerollCostRow MakeCostRow(
            WeaponRarity rarity = WeaponRarity.Common,
            int goldCost = 50,
            int rollCount = 2,
            int minBonus = 1,
            int maxBonus = 5)
        {
            return new RerollCostRow
            {
                Rarity = rarity,
                GoldCost = goldCost,
                RollCount = rollCount,
                MinBonus = minBonus,
                MaxBonus = maxBonus
            };
        }

        // ---- RerollCostPolicy.FindRow ----

        [Test]
        public void FindRow_KnownRarity_ReturnsCorrectRow()
        {
            List<RerollCostRow> rows = new()
            {
                MakeCostRow(WeaponRarity.Common, goldCost: 50),
                MakeCostRow(WeaponRarity.Rare, goldCost: 120)
            };

            RerollCostRow row = RerollCostPolicy.FindRow(WeaponRarity.Rare, rows);

            Assert.That(row.GoldCost, Is.EqualTo(120));
            Assert.That(row.Rarity, Is.EqualTo(WeaponRarity.Rare));
        }

        [Test]
        public void FindRow_UnknownRarity_ThrowsInvalidOperationException()
        {
            List<RerollCostRow> rows = new()
            {
                MakeCostRow(WeaponRarity.Common)
            };

            Assert.Throws<InvalidOperationException>(() =>
                RerollCostPolicy.FindRow(WeaponRarity.Legendary, rows));
        }

        [Test]
        public void FindRow_NullCostRows_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                RerollCostPolicy.FindRow(WeaponRarity.Common, null));
        }

        // ---- EquipmentRerollService null guards ----

        [Test]
        public void Reroll_NullEquipment_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                EquipmentRerollService.Reroll(
                    null,
                    MakeWeapon(),
                    new List<RerollCostRow> { MakeCostRow() },
                    new PlayerWalletState(100),
                    new Random(1)));
        }

        [Test]
        public void Reroll_NullWeaponRow_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                EquipmentRerollService.Reroll(
                    new OwnedEquipment(1, 1),
                    null,
                    new List<RerollCostRow> { MakeCostRow() },
                    new PlayerWalletState(100),
                    new Random(1)));
        }

        [Test]
        public void Reroll_NullCostRows_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                EquipmentRerollService.Reroll(
                    new OwnedEquipment(1, 1),
                    MakeWeapon(),
                    null,
                    new PlayerWalletState(100),
                    new Random(1)));
        }

        [Test]
        public void Reroll_NullWallet_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                EquipmentRerollService.Reroll(
                    new OwnedEquipment(1, 1),
                    MakeWeapon(),
                    new List<RerollCostRow> { MakeCostRow() },
                    null,
                    new Random(1)));
        }

        [Test]
        public void Reroll_NullRandom_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                EquipmentRerollService.Reroll(
                    new OwnedEquipment(1, 1),
                    MakeWeapon(),
                    new List<RerollCostRow> { MakeCostRow() },
                    new PlayerWalletState(100),
                    null));
        }

        // ---- invalid cost row data ----

        [Test]
        public void Reroll_InvalidRollCount_ThrowsInvalidOperationException()
        {
            List<RerollCostRow> rows = new() { MakeCostRow(rollCount: 0) };

            Assert.Throws<InvalidOperationException>(() =>
                EquipmentRerollService.Reroll(
                    new OwnedEquipment(1, 1),
                    MakeWeapon(),
                    rows,
                    new PlayerWalletState(100),
                    new Random(1)));
        }

        [Test]
        public void Reroll_MinBonusExceedsMaxBonus_ThrowsInvalidOperationException()
        {
            List<RerollCostRow> rows = new() { MakeCostRow(minBonus: 10, maxBonus: 5) };

            Assert.Throws<InvalidOperationException>(() =>
                EquipmentRerollService.Reroll(
                    new OwnedEquipment(1, 1),
                    MakeWeapon(),
                    rows,
                    new PlayerWalletState(100),
                    new Random(1)));
        }

        // ---- gold cost deduction ----

        [Test]
        public void Reroll_SufficientGold_DeductsGoldFromWallet()
        {
            PlayerWalletState wallet = new(200);
            List<RerollCostRow> rows = new() { MakeCostRow(goldCost: 80) };

            EquipmentRerollService.Reroll(
                new OwnedEquipment(1, 1),
                MakeWeapon(),
                rows,
                wallet,
                new Random(1));

            Assert.That(wallet.Gold, Is.EqualTo(120));
        }

        [Test]
        public void Reroll_InsufficientGold_ThrowsInvalidOperationException()
        {
            PlayerWalletState wallet = new(30);
            List<RerollCostRow> rows = new() { MakeCostRow(goldCost: 50) };

            Assert.Throws<InvalidOperationException>(() =>
                EquipmentRerollService.Reroll(
                    new OwnedEquipment(1, 1),
                    MakeWeapon(),
                    rows,
                    wallet,
                    new Random(1)));
        }

        [Test]
        public void Reroll_InsufficientGold_DoesNotDeductGold()
        {
            PlayerWalletState wallet = new(30);
            List<RerollCostRow> rows = new() { MakeCostRow(goldCost: 50) };

            try
            {
                EquipmentRerollService.Reroll(
                    new OwnedEquipment(1, 1),
                    MakeWeapon(),
                    rows,
                    wallet,
                    new Random(1));
            }
            catch (InvalidOperationException) { }

            Assert.That(wallet.Gold, Is.EqualTo(30));
        }

        // ---- identity preservation ----

        [Test]
        public void Reroll_PreservesInstanceId()
        {
            OwnedEquipment equipment = new(42, 7);
            List<RerollCostRow> rows = new() { MakeCostRow() };

            RerollResult result = EquipmentRerollService.Reroll(
                equipment,
                MakeWeapon(id: 7),
                rows,
                new PlayerWalletState(100),
                new Random(1));

            Assert.That(result.InstanceId, Is.EqualTo(42));
        }

        [Test]
        public void Reroll_PreservesWeaponId()
        {
            OwnedEquipment equipment = new(1, 7);
            List<RerollCostRow> rows = new() { MakeCostRow() };

            RerollResult result = EquipmentRerollService.Reroll(
                equipment,
                MakeWeapon(id: 7),
                rows,
                new PlayerWalletState(100),
                new Random(1));

            Assert.That(result.WeaponId, Is.EqualTo(7));
        }

        // ---- rolled stat output ----

        [Test]
        public void Reroll_RolledStatsCountMatchesRollCount()
        {
            List<RerollCostRow> rows = new() { MakeCostRow(rollCount: 3) };

            RerollResult result = EquipmentRerollService.Reroll(
                new OwnedEquipment(1, 1),
                MakeWeapon(),
                rows,
                new PlayerWalletState(100),
                new Random(1));

            Assert.That(result.RolledStats.Count, Is.EqualTo(3));
        }

        [Test]
        public void Reroll_RolledStatsWithinBounds()
        {
            List<RerollCostRow> rows = new() { MakeCostRow(rollCount: 10, minBonus: 3, maxBonus: 8) };

            RerollResult result = EquipmentRerollService.Reroll(
                new OwnedEquipment(1, 1),
                MakeWeapon(),
                rows,
                new PlayerWalletState(500),
                new Random(99));

            foreach (RolledStatValue stat in result.RolledStats)
            {
                Assert.That(stat.Value, Is.GreaterThanOrEqualTo(3));
                Assert.That(stat.Value, Is.LessThanOrEqualTo(8));
            }
        }

        [Test]
        public void Reroll_SameSeed_ReturnsDeterministicResult()
        {
            List<RerollCostRow> rows = new() { MakeCostRow(rollCount: 3, minBonus: 1, maxBonus: 10) };
            OwnedEquipment equipment = new(1, 1);
            WeaponRow weapon = MakeWeapon();
            PlayerWalletState wallet1 = new(500);
            PlayerWalletState wallet2 = new(500);

            RerollResult result1 = EquipmentRerollService.Reroll(equipment, weapon, rows, wallet1, new Random(77));
            RerollResult result2 = EquipmentRerollService.Reroll(equipment, weapon, rows, wallet2, new Random(77));

            Assert.That(result1.RolledStats.Count, Is.EqualTo(result2.RolledStats.Count));
            for (int i = 0; i < result1.RolledStats.Count; i++)
            {
                Assert.That(result1.RolledStats[i].Value, Is.EqualTo(result2.RolledStats[i].Value));
            }
        }

        [Test]
        public void Reroll_MinBonusEqualsMaxBonus_AlwaysReturnsSingleValue()
        {
            List<RerollCostRow> rows = new() { MakeCostRow(rollCount: 4, minBonus: 7, maxBonus: 7) };

            RerollResult result = EquipmentRerollService.Reroll(
                new OwnedEquipment(1, 1),
                MakeWeapon(),
                rows,
                new PlayerWalletState(200),
                new Random(1));

            foreach (RolledStatValue stat in result.RolledStats)
            {
                Assert.That(stat.Value, Is.EqualTo(7));
            }
        }
    }
}
