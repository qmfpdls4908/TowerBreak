using System;

namespace TowerBreak.GameData.TowerBreaker
{
    [Serializable]
    public sealed class RewardTableRow
    {
        [Column("Id")]
        public int Id;

        [Column("GuaranteedGold")]
        public int GuaranteedGold;

        [Column("WeaponDropChance")]
        public float WeaponDropChance;

        [Column("FallbackRewardId")]
        public int FallbackRewardId;

        [Column("RewardPopupSfxKey")]
        public string RewardPopupSfxKey;
    }
}
