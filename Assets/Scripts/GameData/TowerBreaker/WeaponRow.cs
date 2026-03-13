using System;

namespace TowerBreak.GameData.TowerBreaker
{
    [Serializable]
    public sealed class WeaponRow
    {
        [Column("Id")]
        public int Id;

        [Column("Archetype")]
        public WeaponArchetype Archetype;

        [Column("Rarity")]
        public WeaponRarity Rarity;

        [Column("BaseAttack")]
        public int BaseAttack;

        [Column("AttackSpeed")]
        public float AttackSpeed;

        [Column("PushPower")]
        public float PushPower;

        [Column("RerollGroupId")]
        public int RerollGroupId;

        [Column("IconKey")]
        public string IconKey;

        [Column("AttackVfxKey")]
        public string AttackVfxKey;

        [Column("HitSfxKey")]
        public string HitSfxKey;

        [Column("EquipSfxKey")]
        public string EquipSfxKey;
    }
}
