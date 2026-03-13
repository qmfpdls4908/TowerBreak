using System;

namespace TowerBreak.GameData.TowerBreaker
{
    [Serializable]
    public sealed class FloorWaveRow
    {
        [Column("FloorId")]
        public int FloorId;

        [Column("WaveIndex")]
        public int WaveIndex;

        [Column("EnemyId")]
        public int EnemyId;

        [Column("SpawnOrder")]
        public int SpawnOrder;

        [Column("SpawnTime")]
        public float SpawnTime;

        [Column("Quantity")]
        public int Quantity;
    }
}
