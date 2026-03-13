using System;

namespace TowerBreak.GameData.TowerBreaker
{
    [Serializable]
    public sealed class RerollCostRow
    {
        [Column("Rarity")]
        public WeaponRarity Rarity;

        [Column("GoldCost")]
        public int GoldCost;

        [Column("RollCount")]
        public int RollCount;

        [Column("MinBonus")]
        public int MinBonus;

        [Column("MaxBonus")]
        public int MaxBonus;
    }
}
