using System;

namespace TowerBreak.GameData.TowerBreaker
{
    [Serializable]
    public sealed class FloorRow
    {
        [Column("Id")]
        public int Id;

        [Column("DisplayName")]
        public string DisplayName;

        [Column("RewardTableId")]
        public int RewardTableId;

        [Column("RecommendedPower")]
        public int RecommendedPower;

        [Column("BattleBackdropKey")]
        public string BattleBackdropKey;

        [Column("BattleBgmKey")]
        public string BattleBgmKey;
    }
}
