using System;

namespace TowerBreak.Meta.State
{
    public sealed class OwnedEquipment
    {
        public OwnedEquipment(int instanceId, int weaponId, int enhancementLevel = 0)
        {
            if (instanceId <= 0)
            {
                throw new ArgumentException("Instance ID must be greater than zero.", nameof(instanceId));
            }

            if (weaponId <= 0)
            {
                throw new ArgumentException("Weapon ID must be greater than zero.", nameof(weaponId));
            }

            if (enhancementLevel < 0)
            {
                throw new ArgumentException("Enhancement level cannot be negative.", nameof(enhancementLevel));
            }

            InstanceId = instanceId;
            WeaponId = weaponId;
            EnhancementLevel = enhancementLevel;
        }

        public int InstanceId { get; }
        public int WeaponId { get; }
        public int EnhancementLevel { get; private set; }

        public void IncrementEnhancementLevel()
        {
            EnhancementLevel++;
        }
    }
}
