using System;

using NUnit.Framework;

using TowerBreak.Meta.State;

namespace TowerBreak.Meta.Tests
{
    public sealed class PlayerInventoryStateTests
    {
        // --- PlayerWalletState ---

        [Test]
        public void AddGold_IncreasesBalance()
        {
            PlayerWalletState wallet = new(initialGold: 10);

            wallet.AddGold(5);

            Assert.That(wallet.Gold, Is.EqualTo(15));
        }

        [Test]
        public void AddGold_MultipleAdds_Accumulates()
        {
            PlayerWalletState wallet = new();

            wallet.AddGold(100);
            wallet.AddGold(50);

            Assert.That(wallet.Gold, Is.EqualTo(150));
        }

        [Test]
        public void TryDeductGold_WhenSufficientBalance_DeductsAndReturnsTrue()
        {
            PlayerWalletState wallet = new(initialGold: 100);

            bool result = wallet.TryDeductGold(40);

            Assert.That(result, Is.True);
            Assert.That(wallet.Gold, Is.EqualTo(60));
        }

        [Test]
        public void TryDeductGold_WhenInsufficientBalance_DoesNotDeductAndReturnsFalse()
        {
            PlayerWalletState wallet = new(initialGold: 10);

            bool result = wallet.TryDeductGold(50);

            Assert.That(result, Is.False);
            Assert.That(wallet.Gold, Is.EqualTo(10));
        }

        [Test]
        public void TryDeductGold_ExactBalance_DeductsToZeroAndReturnsTrue()
        {
            PlayerWalletState wallet = new(initialGold: 30);

            bool result = wallet.TryDeductGold(30);

            Assert.That(result, Is.True);
            Assert.That(wallet.Gold, Is.EqualTo(0));
        }

        // --- PlayerInventoryState + OwnedEquipment ---

        [Test]
        public void AddEquipment_SingleItem_IncreasesCount()
        {
            PlayerInventoryState inventory = new();
            OwnedEquipment item = new(instanceId: 1, weaponId: 101);

            inventory.AddEquipment(item);

            Assert.That(inventory.Equipment.Count, Is.EqualTo(1));
        }

        [Test]
        public void AddEquipment_DuplicateInstanceId_ThrowsInvalidOperationException()
        {
            PlayerInventoryState inventory = new();
            inventory.AddEquipment(new OwnedEquipment(instanceId: 1, weaponId: 101));

            Assert.Throws<InvalidOperationException>(() =>
                inventory.AddEquipment(new OwnedEquipment(instanceId: 1, weaponId: 102)));
        }

        [Test]
        public void EquipWeapon_WhenOwned_SetsEquippedSlot()
        {
            PlayerInventoryState inventory = new();
            inventory.AddEquipment(new OwnedEquipment(instanceId: 1, weaponId: 101));

            inventory.EquipWeapon(instanceId: 1);

            Assert.That(inventory.EquippedWeaponInstanceId, Is.EqualTo(1));
        }

        [Test]
        public void EquipWeapon_WhenNotInInventory_ThrowsInvalidOperationException()
        {
            PlayerInventoryState inventory = new();

            Assert.Throws<InvalidOperationException>(() => inventory.EquipWeapon(instanceId: 99));
        }

        [Test]
        public void EquipWeapon_ReplacesExistingEquipped()
        {
            PlayerInventoryState inventory = new();
            inventory.AddEquipment(new OwnedEquipment(instanceId: 1, weaponId: 101));
            inventory.AddEquipment(new OwnedEquipment(instanceId: 2, weaponId: 102));
            inventory.EquipWeapon(instanceId: 1);

            inventory.EquipWeapon(instanceId: 2);

            Assert.That(inventory.EquippedWeaponInstanceId, Is.EqualTo(2));
        }

        [Test]
        public void OwnedEquipment_ExposesInstanceIdAndWeaponId()
        {
            OwnedEquipment item = new(instanceId: 42, weaponId: 7);

            Assert.That(item.InstanceId, Is.EqualTo(42));
            Assert.That(item.WeaponId, Is.EqualTo(7));
        }

        [Test]
        public void EquipWeapon_DefaultState_HasNoEquippedWeapon()
        {
            PlayerInventoryState inventory = new();

            Assert.That(inventory.EquippedWeaponInstanceId, Is.Null);
        }
    }
}
