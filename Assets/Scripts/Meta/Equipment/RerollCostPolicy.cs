using System;
using System.Collections.Generic;

using TowerBreak.GameData.TowerBreaker;

namespace TowerBreak.Meta.Equipment
{
    public static class RerollCostPolicy
    {
        public static RerollCostRow FindRow(WeaponRarity rarity, IReadOnlyList<RerollCostRow> costRows)
        {
            if (costRows == null)
            {
                throw new ArgumentNullException(nameof(costRows));
            }

            for (int i = 0; i < costRows.Count; i++)
            {
                if (costRows[i].Rarity == rarity)
                {
                    return costRows[i];
                }
            }

            throw new InvalidOperationException(
                $"No reroll cost row found for rarity {rarity}.");
        }
    }
}
