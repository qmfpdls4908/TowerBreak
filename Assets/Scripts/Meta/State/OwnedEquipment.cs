using System;

namespace TowerBreak.Meta.State
{
    public sealed class OwnedEquipment
    {
        public OwnedEquipment(int instanceId, int weaponId)
        {
            if (instanceId <= 0)
            {
                throw new ArgumentException("Instance ID must be greater than zero.", nameof(instanceId));
            }

            if (weaponId <= 0)
            {
                throw new ArgumentException("Weapon ID must be greater than zero.", nameof(weaponId));
            }

            InstanceId = instanceId;
            WeaponId = weaponId;
        }

        public int InstanceId { get; }
        public int WeaponId { get; }
    }
}
