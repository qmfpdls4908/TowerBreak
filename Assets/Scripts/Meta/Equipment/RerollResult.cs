using System;
using System.Collections.Generic;

namespace TowerBreak.Meta.Equipment
{
    public sealed class RerollResult
    {
        public RerollResult(int instanceId, int weaponId, IReadOnlyList<RolledStatValue> rolledStats)
        {
            if (rolledStats == null)
            {
                throw new ArgumentNullException(nameof(rolledStats));
            }

            InstanceId = instanceId;
            WeaponId = weaponId;
            RolledStats = rolledStats;
        }

        public int InstanceId { get; }
        public int WeaponId { get; }
        public IReadOnlyList<RolledStatValue> RolledStats { get; }
    }
}
