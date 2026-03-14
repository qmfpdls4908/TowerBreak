using System;
using System.Collections.Generic;

using NUnit.Framework;

using TowerBreak.GameData.TowerBreaker;
using TowerBreak.Meta.Rewards;
using TowerBreak.Meta.State;

namespace TowerBreak.Meta.Tests
{
    public sealed class RewardResolverTests
    {
        // --- RewardBundle ---

        [Test]
        public void RewardBundle_ExposesGoldAndWeaponIds()
        {
            List<int> weaponIds = new() { 201, 202 };
            RewardBundle bundle = new(goldAmount: 50, grantedWeaponIds: weaponIds);

            Assert.That(bundle.GoldAmount, Is.EqualTo(50));
            Assert.That(bundle.GrantedWeaponIds, Is.EqualTo(weaponIds));
        }

        // --- RewardResolver ---

        [Test]
        public void Resolve_GuaranteedGold_ReturnsThatGoldAmount()
        {
            FloorRow floor = new() { Id = 1, RewardTableId = 10 };
            List<RewardTableRow> tables = new()
            {
                new() { Id = 10, GuaranteedGold = 50, WeaponDropChance = 0f, FallbackRewardId = 0 }
            };
            List<RewardEntryRow> entries = new();

            RewardBundle bundle = RewardResolver.Resolve(floor, tables, entries, new Random(0));

            Assert.That(bundle.GoldAmount, Is.EqualTo(50));
            Assert.That(bundle.GrantedWeaponIds, Is.Empty);
        }

        [Test]
        public void Resolve_WhenDropChanceIsOne_IncludesWeaponId()
        {
            FloorRow floor = new() { Id = 1, RewardTableId = 10 };
            List<RewardTableRow> tables = new()
            {
                new() { Id = 10, GuaranteedGold = 0, WeaponDropChance = 1f, FallbackRewardId = 0 }
            };
            List<RewardEntryRow> entries = new()
            {
                new() { RewardTableId = 10, RewardId = 1, RewardType = RewardType.Weapon, TargetItemId = 201, Weight = 1 }
            };

            RewardBundle bundle = RewardResolver.Resolve(floor, tables, entries, new Random(0));

            Assert.That(bundle.GrantedWeaponIds, Contains.Item(201));
        }

        [Test]
        public void Resolve_WhenDropChanceIsZero_GrantsNoWeapon()
        {
            FloorRow floor = new() { Id = 1, RewardTableId = 10 };
            List<RewardTableRow> tables = new()
            {
                new() { Id = 10, GuaranteedGold = 100, WeaponDropChance = 0f, FallbackRewardId = 0 }
            };
            List<RewardEntryRow> entries = new()
            {
                new() { RewardTableId = 10, RewardId = 1, RewardType = RewardType.Weapon, TargetItemId = 201, Weight = 1 }
            };

            RewardBundle bundle = RewardResolver.Resolve(floor, tables, entries, new Random(0));

            Assert.That(bundle.GrantedWeaponIds, Is.Empty);
            Assert.That(bundle.GoldAmount, Is.EqualTo(100));
        }

        [Test]
        public void Resolve_WhenDropFails_FallbackGoldEntry_AddsGoldToBundle()
        {
            FloorRow floor = new() { Id = 1, RewardTableId = 10 };
            List<RewardTableRow> tables = new()
            {
                new() { Id = 10, GuaranteedGold = 0, WeaponDropChance = 0f, FallbackRewardId = 2 }
            };
            List<RewardEntryRow> entries = new()
            {
                new() { RewardTableId = 10, RewardId = 1, RewardType = RewardType.Weapon, TargetItemId = 201, Weight = 1 },
                new() { RewardTableId = 10, RewardId = 2, RewardType = RewardType.Gold, TargetItemId = 0, Weight = 1, QuantityMin = 20, QuantityMax = 20 }
            };

            RewardBundle bundle = RewardResolver.Resolve(floor, tables, entries, new Random(0));

            Assert.That(bundle.GoldAmount, Is.EqualTo(20));
            Assert.That(bundle.GrantedWeaponIds, Is.Empty);
        }

        [Test]
        public void Resolve_WhenDropFails_FallbackWeaponEntry_GrantsWeapon()
        {
            FloorRow floor = new() { Id = 1, RewardTableId = 10 };
            List<RewardTableRow> tables = new()
            {
                new() { Id = 10, GuaranteedGold = 0, WeaponDropChance = 0f, FallbackRewardId = 2 }
            };
            List<RewardEntryRow> entries = new()
            {
                new() { RewardTableId = 10, RewardId = 2, RewardType = RewardType.Weapon, TargetItemId = 301, Weight = 1 }
            };

            RewardBundle bundle = RewardResolver.Resolve(floor, tables, entries, new Random(0));

            Assert.That(bundle.GrantedWeaponIds, Contains.Item(301));
        }

        [Test]
        public void Resolve_WeightedEntrySelection_ConsistentForSameSeed()
        {
            FloorRow floor = new() { Id = 1, RewardTableId = 10 };
            List<RewardTableRow> tables = new()
            {
                new() { Id = 10, GuaranteedGold = 0, WeaponDropChance = 1f, FallbackRewardId = 0 }
            };
            List<RewardEntryRow> entries = new()
            {
                new() { RewardTableId = 10, RewardId = 1, RewardType = RewardType.Weapon, TargetItemId = 201, Weight = 1 },
                new() { RewardTableId = 10, RewardId = 2, RewardType = RewardType.Weapon, TargetItemId = 202, Weight = 1 }
            };

            RewardBundle bundle1 = RewardResolver.Resolve(floor, tables, entries, new Random(42));
            RewardBundle bundle2 = RewardResolver.Resolve(floor, tables, entries, new Random(42));

            Assert.That(bundle1.GrantedWeaponIds.Count, Is.EqualTo(1));
            Assert.That(bundle1.GrantedWeaponIds[0], Is.EqualTo(bundle2.GrantedWeaponIds[0]));
        }

        [Test]
        public void Resolve_MissingRewardTable_ThrowsInvalidOperationException()
        {
            FloorRow floor = new() { Id = 1, RewardTableId = 999 };
            List<RewardTableRow> tables = new();
            List<RewardEntryRow> entries = new();

            Assert.Throws<InvalidOperationException>(() =>
                RewardResolver.Resolve(floor, tables, entries, new Random(0)));
        }

        [Test]
        public void Resolve_NullFloor_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                RewardResolver.Resolve(null, new List<RewardTableRow>(), new List<RewardEntryRow>(), new Random(0)));
        }

        [Test]
        public void Resolve_GuaranteedGoldPlusFallbackGold_AccumulatesCorrectly()
        {
            FloorRow floor = new() { Id = 1, RewardTableId = 10 };
            List<RewardTableRow> tables = new()
            {
                new() { Id = 10, GuaranteedGold = 30, WeaponDropChance = 0f, FallbackRewardId = 2 }
            };
            List<RewardEntryRow> entries = new()
            {
                new() { RewardTableId = 10, RewardId = 2, RewardType = RewardType.Gold, TargetItemId = 0, Weight = 1, QuantityMin = 15, QuantityMax = 15 }
            };

            RewardBundle bundle = RewardResolver.Resolve(floor, tables, entries, new Random(0));

            Assert.That(bundle.GoldAmount, Is.EqualTo(45));
        }

        // --- Error Cases ---

        [Test]
        public void Resolve_FallbackRewardId_SetButEntryNotFound_ThrowsInvalidOperationException()
        {
            FloorRow floor = new() { Id = 1, RewardTableId = 10 };
            List<RewardTableRow> tables = new()
            {
                new() { Id = 10, GuaranteedGold = 0, WeaponDropChance = 0f, FallbackRewardId = 999 }
            };
            List<RewardEntryRow> entries = new();

            Assert.Throws<InvalidOperationException>(() =>
                RewardResolver.Resolve(floor, tables, entries, new Random(0)));
        }

        [Test]
        public void Resolve_WeaponDropSucceeds_ButNoWeaponEntries_ThrowsInvalidOperationException()
        {
            FloorRow floor = new() { Id = 1, RewardTableId = 10 };
            List<RewardTableRow> tables = new()
            {
                new() { Id = 10, GuaranteedGold = 0, WeaponDropChance = 1f, FallbackRewardId = 0 }
            };
            List<RewardEntryRow> entries = new();

            Assert.Throws<InvalidOperationException>(() =>
                RewardResolver.Resolve(floor, tables, entries, new Random(0)));
        }

        [Test]
        public void Resolve_MalformedGoldEntry_QuantityMinGreaterThanQuantityMax_ThrowsInvalidOperationException()
        {
            FloorRow floor = new() { Id = 1, RewardTableId = 10 };
            List<RewardTableRow> tables = new()
            {
                new() { Id = 10, GuaranteedGold = 0, WeaponDropChance = 0f, FallbackRewardId = 1 }
            };
            List<RewardEntryRow> entries = new()
            {
                new() { RewardTableId = 10, RewardId = 1, RewardType = RewardType.Gold, TargetItemId = 0, Weight = 1, QuantityMin = 50, QuantityMax = 10 }
            };

            Assert.Throws<InvalidOperationException>(() =>
                RewardResolver.Resolve(floor, tables, entries, new Random(0)));
        }

        [Test]
        public void Resolve_MalformedGoldEntry_NegativeQuantityMin_ThrowsInvalidOperationException()
        {
            FloorRow floor = new() { Id = 1, RewardTableId = 10 };
            List<RewardTableRow> tables = new()
            {
                new() { Id = 10, GuaranteedGold = 0, WeaponDropChance = 0f, FallbackRewardId = 1 }
            };
            List<RewardEntryRow> entries = new()
            {
                new() { RewardTableId = 10, RewardId = 1, RewardType = RewardType.Gold, TargetItemId = 0, Weight = 1, QuantityMin = -5, QuantityMax = 10 }
            };

            Assert.Throws<InvalidOperationException>(() =>
                RewardResolver.Resolve(floor, tables, entries, new Random(0)));
        }

        [Test]
        public void Resolve_MalformedWeaponEntry_TargetItemIdInvalid_ThrowsInvalidOperationException()
        {
            FloorRow floor = new() { Id = 1, RewardTableId = 10 };
            List<RewardTableRow> tables = new()
            {
                new() { Id = 10, GuaranteedGold = 0, WeaponDropChance = 1f, FallbackRewardId = 0 }
            };
            List<RewardEntryRow> entries = new()
            {
                new() { RewardTableId = 10, RewardId = 1, RewardType = RewardType.Weapon, TargetItemId = 0, Weight = 1 }
            };

            Assert.Throws<InvalidOperationException>(() =>
                RewardResolver.Resolve(floor, tables, entries, new Random(0)));
        }

        // --- BattleRewardService ---

        [Test]
        public void Apply_GuaranteedGold_AddsGoldToWallet()
        {
            PlayerWalletState wallet = new();
            PlayerInventoryState inventory = new();
            RewardBundle bundle = new(goldAmount: 100, grantedWeaponIds: new List<int>());
            int nextId = 1;

            BattleRewardService.Apply(bundle, wallet, inventory, () => nextId++);

            Assert.That(wallet.Gold, Is.EqualTo(100));
        }

        [Test]
        public void Apply_WeaponGrant_AddsEquipmentToInventory()
        {
            PlayerWalletState wallet = new();
            PlayerInventoryState inventory = new();
            RewardBundle bundle = new(goldAmount: 0, grantedWeaponIds: new List<int> { 201 });
            int nextId = 1;

            BattleRewardService.Apply(bundle, wallet, inventory, () => nextId++);

            Assert.That(inventory.Equipment.Count, Is.EqualTo(1));
            Assert.That(inventory.Equipment[0].WeaponId, Is.EqualTo(201));
        }

        [Test]
        public void Apply_MultipleWeapons_AddsAllToInventory()
        {
            PlayerWalletState wallet = new();
            PlayerInventoryState inventory = new();
            RewardBundle bundle = new(goldAmount: 0, grantedWeaponIds: new List<int> { 201, 202 });
            int nextId = 1;

            BattleRewardService.Apply(bundle, wallet, inventory, () => nextId++);

            Assert.That(inventory.Equipment.Count, Is.EqualTo(2));
            Assert.That(inventory.Equipment[0].WeaponId, Is.EqualTo(201));
            Assert.That(inventory.Equipment[1].WeaponId, Is.EqualTo(202));
        }

        [Test]
        public void Apply_EmptyBundle_WalletAndInventoryUnchanged()
        {
            PlayerWalletState wallet = new(initialGold: 50);
            PlayerInventoryState inventory = new();
            RewardBundle bundle = new(goldAmount: 0, grantedWeaponIds: new List<int>());
            int nextId = 1;

            BattleRewardService.Apply(bundle, wallet, inventory, () => nextId++);

            Assert.That(wallet.Gold, Is.EqualTo(50));
            Assert.That(inventory.Equipment, Is.Empty);
        }

        [Test]
        public void Apply_GoldAndWeapon_UpdatesBothStateObjects()
        {
            PlayerWalletState wallet = new();
            PlayerInventoryState inventory = new();
            RewardBundle bundle = new(goldAmount: 75, grantedWeaponIds: new List<int> { 301 });
            int nextId = 10;

            BattleRewardService.Apply(bundle, wallet, inventory, () => nextId++);

            Assert.That(wallet.Gold, Is.EqualTo(75));
            Assert.That(inventory.Equipment.Count, Is.EqualTo(1));
            Assert.That(inventory.Equipment[0].WeaponId, Is.EqualTo(301));
            Assert.That(inventory.Equipment[0].InstanceId, Is.EqualTo(10));
        }
    }
}
