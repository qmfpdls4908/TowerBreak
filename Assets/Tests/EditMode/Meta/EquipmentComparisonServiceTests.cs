using System;
using System.Collections.Generic;

using NUnit.Framework;

using TowerBreak.GameData.TowerBreaker;
using TowerBreak.Meta.Equipment;
using TowerBreak.Meta.State;

namespace TowerBreak.Meta.Tests
{
    public sealed class EquipmentComparisonServiceTests
    {
        // --- EquipmentStatBlock ---

        [Test]
        public void EquipmentStatBlock_ExposesAttackAttackSpeedPushPower()
        {
            EquipmentStatBlock block = new(attack: 10, attackSpeed: 1.5f, pushPower: 2.0f);

            Assert.That(block.Attack, Is.EqualTo(10));
            Assert.That(block.AttackSpeed, Is.EqualTo(1.5f));
            Assert.That(block.PushPower, Is.EqualTo(2.0f));
        }

        // --- EquipmentStatBlock.FromWeaponRow ---

        [Test]
        public void FromWeaponRow_ReturnsStatBlockWithCorrectValues()
        {
            WeaponRow row = new()
            {
                Id = 101,
                BaseAttack = 25,
                AttackSpeed = 1.2f,
                PushPower = 1.5f
            };

            EquipmentStatBlock block = EquipmentStatBlock.FromWeaponRow(row);

            Assert.That(block.Attack, Is.EqualTo(25));
            Assert.That(block.AttackSpeed, Is.EqualTo(1.2f));
            Assert.That(block.PushPower, Is.EqualTo(1.5f));
        }

        [Test]
        public void FromWeaponRow_NullRow_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                EquipmentStatBlock.FromWeaponRow(null));
        }

        // --- EquipmentComparisonService.GetEquippedStatBlock ---

        [Test]
        public void GetEquippedStatBlock_WithEquippedWeapon_ReturnsStatBlock()
        {
            PlayerInventoryState inventory = new();
            inventory.AddEquipment(new OwnedEquipment(1, 101));
            inventory.EquipWeapon(1);

            List<WeaponRow> weaponRows = new()
            {
                new() { Id = 101, BaseAttack = 30, AttackSpeed = 1.0f, PushPower = 2.0f }
            };

            EquipmentStatBlock block = EquipmentComparisonService.GetEquippedStatBlock(inventory, weaponRows);

            Assert.That(block.Attack, Is.EqualTo(30));
        }

        [Test]
        public void GetEquippedStatBlock_NoEquippedWeapon_ReturnsZeroStatBlock()
        {
            PlayerInventoryState inventory = new();
            List<WeaponRow> weaponRows = new();

            EquipmentStatBlock block = EquipmentComparisonService.GetEquippedStatBlock(inventory, weaponRows);

            Assert.That(block.Attack, Is.EqualTo(0));
            Assert.That(block.AttackSpeed, Is.EqualTo(0f));
            Assert.That(block.PushPower, Is.EqualTo(0f));
        }

        [Test]
        public void GetEquippedStatBlock_EquippedWeaponNotInRows_ThrowsInvalidOperationException()
        {
            PlayerInventoryState inventory = new();
            inventory.AddEquipment(new OwnedEquipment(1, 999));
            inventory.EquipWeapon(1);

            List<WeaponRow> weaponRows = new();

            Assert.Throws<InvalidOperationException>(() =>
                EquipmentComparisonService.GetEquippedStatBlock(inventory, weaponRows));
        }

        [Test]
        public void GetEquippedStatBlock_NullInventory_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                EquipmentComparisonService.GetEquippedStatBlock(null, new List<WeaponRow>()));
        }

        [Test]
        public void GetEquippedStatBlock_NullWeaponRows_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                EquipmentComparisonService.GetEquippedStatBlock(new PlayerInventoryState(), null));
        }

        // --- EquipmentComparisonService.GetCandidateStatBlock ---

        [Test]
        public void GetCandidateStatBlock_ValidOwnedWeapon_ReturnsStatBlock()
        {
            PlayerInventoryState inventory = new();
            inventory.AddEquipment(new OwnedEquipment(1, 101));

            List<WeaponRow> weaponRows = new()
            {
                new() { Id = 101, BaseAttack = 40, AttackSpeed = 1.5f, PushPower = 3.0f }
            };

            EquipmentStatBlock block = EquipmentComparisonService.GetCandidateStatBlock(inventory, 1, weaponRows);

            Assert.That(block.Attack, Is.EqualTo(40));
            Assert.That(block.AttackSpeed, Is.EqualTo(1.5f));
            Assert.That(block.PushPower, Is.EqualTo(3.0f));
        }

        [Test]
        public void GetCandidateStatBlock_InstanceNotInInventory_ThrowsInvalidOperationException()
        {
            PlayerInventoryState inventory = new();
            List<WeaponRow> weaponRows = new();

            Assert.Throws<InvalidOperationException>(() =>
                EquipmentComparisonService.GetCandidateStatBlock(inventory, 999, weaponRows));
        }

        [Test]
        public void GetCandidateStatBlock_WeaponRowNotFound_ThrowsInvalidOperationException()
        {
            PlayerInventoryState inventory = new();
            inventory.AddEquipment(new OwnedEquipment(1, 999));

            List<WeaponRow> weaponRows = new();

            Assert.Throws<InvalidOperationException>(() =>
                EquipmentComparisonService.GetCandidateStatBlock(inventory, 1, weaponRows));
        }

        [Test]
        public void GetCandidateStatBlock_NullInventory_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                EquipmentComparisonService.GetCandidateStatBlock(null, 1, new List<WeaponRow>()));
        }

        [Test]
        public void GetCandidateStatBlock_NullWeaponRows_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                EquipmentComparisonService.GetCandidateStatBlock(new PlayerInventoryState(), 1, null));
        }

        // --- EquipmentComparisonService.Compare ---

        [Test]
        public void Compare_DifferentWeapons_ReturnsCorrectDelta()
        {
            EquipmentStatBlock current = new(attack: 30, attackSpeed: 1.0f, pushPower: 2.0f);
            EquipmentStatBlock candidate = new(attack: 40, attackSpeed: 1.5f, pushPower: 1.5f);

            EquipmentComparisonResult result = EquipmentComparisonService.Compare(current, candidate);

            Assert.That(result.Current.Attack, Is.EqualTo(30));
            Assert.That(result.Candidate.Attack, Is.EqualTo(40));
            Assert.That(result.DeltaAttack, Is.EqualTo(10));
            Assert.That(result.DeltaAttackSpeed, Is.EqualTo(0.5f));
            Assert.That(result.DeltaPushPower, Is.EqualTo(-0.5f));
        }

        [Test]
        public void Compare_SameWeapon_ReturnsZeroDelta()
        {
            EquipmentStatBlock current = new(attack: 50, attackSpeed: 2.0f, pushPower: 3.0f);

            EquipmentComparisonResult result = EquipmentComparisonService.Compare(current, current);

            Assert.That(result.DeltaAttack, Is.EqualTo(0));
            Assert.That(result.DeltaAttackSpeed, Is.EqualTo(0f));
            Assert.That(result.DeltaPushPower, Is.EqualTo(0f));
        }

        // --- Integration: Full comparison flow ---

        [Test]
        public void FullComparisonFlow_EquippedVsCandidate_ReturnsCorrectComparison()
        {
            PlayerInventoryState inventory = new();
            inventory.AddEquipment(new OwnedEquipment(1, 101));
            inventory.AddEquipment(new OwnedEquipment(2, 102));
            inventory.EquipWeapon(1);

            List<WeaponRow> weaponRows = new()
            {
                new() { Id = 101, BaseAttack = 30, AttackSpeed = 1.0f, PushPower = 2.0f },
                new() { Id = 102, BaseAttack = 50, AttackSpeed = 1.2f, PushPower = 2.5f }
            };

            EquipmentStatBlock equipped = EquipmentComparisonService.GetEquippedStatBlock(inventory, weaponRows);
            EquipmentStatBlock candidate = EquipmentComparisonService.GetCandidateStatBlock(inventory, 2, weaponRows);
            EquipmentComparisonResult comparison = EquipmentComparisonService.Compare(equipped, candidate);

            Assert.That(comparison.Current.Attack, Is.EqualTo(30));
            Assert.That(comparison.Candidate.Attack, Is.EqualTo(50));
            Assert.That(comparison.DeltaAttack, Is.EqualTo(20));
            Assert.That(comparison.DeltaAttackSpeed, Is.EqualTo(0.2f).Within(0.0001f));
            Assert.That(comparison.DeltaPushPower, Is.EqualTo(0.5f).Within(0.0001f));
        }

        [Test]
        public void FullComparisonFlow_NoEquippedWeapon_CandidateShowsAllPositiveDelta()
        {
            PlayerInventoryState inventory = new();
            inventory.AddEquipment(new OwnedEquipment(1, 101));

            List<WeaponRow> weaponRows = new()
            {
                new() { Id = 101, BaseAttack = 25, AttackSpeed = 1.0f, PushPower = 1.5f }
            };

            EquipmentStatBlock equipped = EquipmentComparisonService.GetEquippedStatBlock(inventory, weaponRows);
            EquipmentStatBlock candidate = EquipmentComparisonService.GetCandidateStatBlock(inventory, 1, weaponRows);
            EquipmentComparisonResult comparison = EquipmentComparisonService.Compare(equipped, candidate);

            Assert.That(comparison.DeltaAttack, Is.EqualTo(25));
            Assert.That(comparison.DeltaAttackSpeed, Is.EqualTo(1.0f));
            Assert.That(comparison.DeltaPushPower, Is.EqualTo(1.5f));
        }
    }
}
