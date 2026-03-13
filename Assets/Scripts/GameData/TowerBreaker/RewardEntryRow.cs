using System;

namespace TowerBreak.GameData.TowerBreaker
{
    [Serializable]
    public sealed class RewardEntryRow
    {
        [Column("RewardTableId")]
        public int RewardTableId;

        [Column("RewardId")]
        public int RewardId;

        [Column("RewardType")]
        public RewardType RewardType;

        [Column("TargetItemId")]
        public int TargetItemId;

        [Column("Weight")]
        public int Weight;

        [Column("QuantityMin")]
        public int QuantityMin;

        [Column("QuantityMax")]
        public int QuantityMax;

        [Column("IconKey")]
        public string IconKey;
    }
}
