using System.Collections.Generic;

namespace TowerBreak.Combat
{
    public sealed class WaveSpawnPlan
    {
        public WaveSpawnPlan(int floorId, IReadOnlyList<WaveSpawnEntry> entries)
        {
            FloorId = floorId;
            Entries = entries;
        }

        public int FloorId { get; }

        public IReadOnlyList<WaveSpawnEntry> Entries { get; }
    }
}
