using System;
using System.Collections.Generic;
using System.Linq;

using TowerBreak.GameData.TowerBreaker;
using TowerBreak.Meta.State;

namespace TowerBreak.Meta.Equipment
{
    public static class EquipmentComparisonService
    {
        public static EquipmentStatBlock GetEquippedStatBlock(
            PlayerInventoryState inventory,
            IReadOnlyList<WeaponRow> weaponRows)
        {
            if (inventory == null)
            {
                throw new ArgumentNullException(nameof(inventory));
            }

            if (weaponRows == null)
            {
                throw new ArgumentNullException(nameof(weaponRows));
            }

            int? equippedInstanceId = inventory.EquippedWeaponInstanceId;
            if (equippedInstanceId == null)
            {
                return new EquipmentStatBlock(0, 0f, 0f);
            }

            OwnedEquipment equipped = inventory.Equipment
                .FirstOrDefault(e => e.InstanceId == equippedInstanceId);

            if (equipped == null)
            {
                throw new InvalidOperationException(
                    $"Equipped weapon with instance ID {equippedInstanceId} not found in inventory.");
            }

            WeaponRow row = weaponRows.FirstOrDefault(r => r.Id == equipped.WeaponId);
            if (row == null)
            {
                throw new InvalidOperationException(
                    $"Weapon row with ID {equipped.WeaponId} not found.");
            }

            return EquipmentStatBlock.FromWeaponRow(row);
        }

        public static EquipmentStatBlock GetCandidateStatBlock(
            PlayerInventoryState inventory,
            int instanceId,
            IReadOnlyList<WeaponRow> weaponRows)
        {
            if (inventory == null)
            {
                throw new ArgumentNullException(nameof(inventory));
            }

            if (weaponRows == null)
            {
                throw new ArgumentNullException(nameof(weaponRows));
            }

            OwnedEquipment candidate = inventory.Equipment
                .FirstOrDefault(e => e.InstanceId == instanceId);

            if (candidate == null)
            {
                throw new InvalidOperationException(
                    $"No owned equipment with instance ID {instanceId} in inventory.");
            }

            WeaponRow row = weaponRows.FirstOrDefault(r => r.Id == candidate.WeaponId);
            if (row == null)
            {
                throw new InvalidOperationException(
                    $"Weapon row with ID {candidate.WeaponId} not found.");
            }

            return EquipmentStatBlock.FromWeaponRow(row);
        }

        public static EquipmentComparisonResult Compare(
            EquipmentStatBlock current,
            EquipmentStatBlock candidate)
        {
            return new EquipmentComparisonResult(current, candidate);
        }
    }
}
