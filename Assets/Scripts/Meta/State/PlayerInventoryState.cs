using System;
using System.Collections.Generic;
using System.Linq;

namespace TowerBreak.Meta.State
{
    public sealed class PlayerInventoryState
    {
        private readonly List<OwnedEquipment> equipment = new();
        private int? equippedWeaponInstanceId;

        public IReadOnlyList<OwnedEquipment> Equipment => equipment;
        public int? EquippedWeaponInstanceId => equippedWeaponInstanceId;

        public void AddEquipment(OwnedEquipment item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (equipment.Any(e => e.InstanceId == item.InstanceId))
            {
                throw new InvalidOperationException(
                    $"Equipment with instance ID {item.InstanceId} is already in inventory.");
            }

            equipment.Add(item);
        }

        public void EquipWeapon(int instanceId)
        {
            if (!equipment.Any(e => e.InstanceId == instanceId))
            {
                throw new InvalidOperationException(
                    $"Cannot equip: no equipment with instance ID {instanceId} in inventory.");
            }

            equippedWeaponInstanceId = instanceId;
        }

        public bool TryGetEquipment(int instanceId, out OwnedEquipment equipment)
        {
            equipment = this.equipment.FirstOrDefault(e => e.InstanceId == instanceId);
            return equipment != null;
        }
    }
}
