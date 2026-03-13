using System.Collections.Generic;

using UnityEngine;

namespace TowerBreak.GameData.TowerBreaker
{
    [CreateAssetMenu(menuName = "GameData/TowerBreakerGameData")]
    public sealed class TowerBreakerGameData : ScriptableObject
    {
        [Sheet("Floors")]
        public List<FloorRow> Floors;

        [Sheet("FloorWaves")]
        public List<FloorWaveRow> FloorWaves;

        [Sheet("Enemies")]
        public List<EnemyRow> Enemies;

        [Sheet("Weapons")]
        public List<WeaponRow> Weapons;

        [Sheet("RewardTables")]
        public List<RewardTableRow> RewardTables;

        [Sheet("RewardEntries")]
        public List<RewardEntryRow> RewardEntries;

        [Sheet("EnhancementCosts")]
        public List<EnhancementCostRow> EnhancementCosts;

        [Sheet("RerollCosts")]
        public List<RerollCostRow> RerollCosts;
    }
}
