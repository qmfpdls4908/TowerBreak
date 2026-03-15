using System;

namespace TowerBreak.GameData.TowerBreaker
{
    [Serializable]
    public sealed class EnemyRow
    {
        [Column("Id")]
        public int Id;

        [Column("Archetype")]
        public EnemyArchetype Archetype;

        [Column("Health")]
        public int Health;

        [Column("Pressure")]
        public float Pressure;

        [Column("MoveSpeed")]
        public float MoveSpeed;

        [Column("AttackCadence")]
        public float AttackCadence;

        [Column("IsArmored")]
        public bool IsArmored;

        [Column("PrefabKey")]
        public string PrefabKey;

        [Column("PortraitKey")]
        public string PortraitKey;

        [Column("HitVfxKey")]
        public string HitVfxKey;

        [Column("HitSfxKey")]
        public string HitSfxKey;

        [Column("IsBoss")]
        public bool IsBoss;

        [Column("PushBackDistance")]
        public float PushBackDistance;

        [Column("DeathVfxKey")]
        public string DeathVfxKey;
    }
}
