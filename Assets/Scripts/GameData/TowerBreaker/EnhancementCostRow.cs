using System;

namespace TowerBreak.GameData.TowerBreaker
{
    [Serializable]
    public sealed class EnhancementCostRow
    {
        [Column("Level")]
        public int Level;

        [Column("GoldCost")]
        public int GoldCost;

        [Column("MaterialCost")]
        public int MaterialCost;

        [Column("AttackBonus")]
        public int AttackBonus;

        [Column("PressureBonus")]
        public float PressureBonus;
    }
}
