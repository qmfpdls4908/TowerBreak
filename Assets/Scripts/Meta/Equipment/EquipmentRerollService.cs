using System;
using System.Collections.Generic;

using TowerBreak.GameData.TowerBreaker;
using TowerBreak.Meta.State;

namespace TowerBreak.Meta.Equipment
{
    public static class EquipmentRerollService
    {
        public static RerollResult Reroll(
            OwnedEquipment equipment,
            WeaponRow weaponRow,
            IReadOnlyList<RerollCostRow> costRows,
            PlayerWalletState wallet,
            Random random)
        {
            if (equipment == null)
            {
                throw new ArgumentNullException(nameof(equipment));
            }

            if (weaponRow == null)
            {
                throw new ArgumentNullException(nameof(weaponRow));
            }

            if (costRows == null)
            {
                throw new ArgumentNullException(nameof(costRows));
            }

            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            RerollCostRow costRow = RerollCostPolicy.FindRow(weaponRow.Rarity, costRows);

            ValidateCostRow(costRow);

            if (!wallet.TryDeductGold(costRow.GoldCost))
            {
                throw new InvalidOperationException(
                    $"Insufficient gold for reroll. Required: {costRow.GoldCost}, Available: {wallet.Gold}.");
            }

            List<RolledStatValue> rolledStats = new();
            int range = costRow.MaxBonus - costRow.MinBonus + 1;

            for (int i = 0; i < costRow.RollCount; i++)
            {
                int value = costRow.MinBonus + random.Next(range);
                rolledStats.Add(new RolledStatValue(value));
            }

            return new RerollResult(equipment.InstanceId, equipment.WeaponId, rolledStats);
        }

        private static void ValidateCostRow(RerollCostRow row)
        {
            if (row.RollCount <= 0)
            {
                throw new InvalidOperationException(
                    $"Invalid reroll cost row: RollCount ({row.RollCount}) must be greater than zero for rarity {row.Rarity}.");
            }

            if (row.MinBonus > row.MaxBonus)
            {
                throw new InvalidOperationException(
                    $"Invalid reroll cost row: MinBonus ({row.MinBonus}) cannot exceed MaxBonus ({row.MaxBonus}) for rarity {row.Rarity}.");
            }
        }
    }
}
