using System;
using System.Collections.Generic;

using TowerBreak.GameData.TowerBreaker;

namespace TowerBreak.Meta.Rewards
{
    public static class RewardResolver
    {
        public static RewardBundle Resolve(
            FloorRow floor,
            IReadOnlyList<RewardTableRow> tables,
            IReadOnlyList<RewardEntryRow> entries,
            Random random)
        {
            if (floor == null)
            {
                throw new ArgumentNullException(nameof(floor));
            }

            if (tables == null)
            {
                throw new ArgumentNullException(nameof(tables));
            }

            if (entries == null)
            {
                throw new ArgumentNullException(nameof(entries));
            }

            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            RewardTableRow table = FindTable(floor.RewardTableId, tables);

            int totalGold = table.GuaranteedGold;
            List<int> weaponIds = new();

            List<RewardEntryRow> weaponEntries = GetWeaponEntries(table.Id, entries);

            double dropRoll = random.NextDouble();
            if (dropRoll < table.WeaponDropChance)
            {
                if (weaponEntries.Count == 0)
                {
                    throw new InvalidOperationException($"Weapon drop succeeded but no weapon entries found for table ID {table.Id}.");
                }
                RewardEntryRow selected = SelectWeighted(weaponEntries, random);
                ValidateWeaponEntry(selected);
                weaponIds.Add(selected.TargetItemId);
            }
            else if (table.FallbackRewardId > 0)
            {
                RewardEntryRow fallback = FindEntry(table.Id, table.FallbackRewardId, entries);
                if (fallback == null)
                {
                    throw new InvalidOperationException($"Fallback reward ID {table.FallbackRewardId} not found in table ID {table.Id}.");
                }
                ApplyEntry(fallback, ref totalGold, weaponIds, random);
            }

            return new RewardBundle(totalGold, weaponIds);
        }

        private static RewardTableRow FindTable(int tableId, IReadOnlyList<RewardTableRow> tables)
        {
            for (int i = 0; i < tables.Count; i++)
            {
                if (tables[i].Id == tableId)
                {
                    return tables[i];
                }
            }

            throw new InvalidOperationException($"No reward table found for ID {tableId}.");
        }

        private static List<RewardEntryRow> GetWeaponEntries(int tableId, IReadOnlyList<RewardEntryRow> entries)
        {
            List<RewardEntryRow> result = new();
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].RewardTableId == tableId && entries[i].RewardType == RewardType.Weapon)
                {
                    result.Add(entries[i]);
                }
            }

            return result;
        }

        private static RewardEntryRow FindEntry(int tableId, int rewardId, IReadOnlyList<RewardEntryRow> entries)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].RewardTableId == tableId && entries[i].RewardId == rewardId)
                {
                    return entries[i];
                }
            }

            return null;
        }

        private static void ApplyEntry(RewardEntryRow entry, ref int totalGold, List<int> weaponIds, Random random)
        {
            if (entry.RewardType == RewardType.Gold)
            {
                ValidateGoldEntry(entry);
                int quantity = entry.QuantityMin == entry.QuantityMax
                    ? entry.QuantityMin
                    : entry.QuantityMin + (int)(random.NextDouble() * (entry.QuantityMax - entry.QuantityMin + 1));
                totalGold += quantity;
            }
            else if (entry.RewardType == RewardType.Weapon)
            {
                ValidateWeaponEntry(entry);
                weaponIds.Add(entry.TargetItemId);
            }
        }

        private static void ValidateGoldEntry(RewardEntryRow entry)
        {
            if (entry.QuantityMin < 0)
            {
                throw new InvalidOperationException($"Invalid gold entry: QuantityMin ({entry.QuantityMin}) cannot be negative. RewardId={entry.RewardId}, TableId={entry.RewardTableId}");
            }

            if (entry.QuantityMin > entry.QuantityMax)
            {
                throw new InvalidOperationException($"Invalid gold entry: QuantityMin ({entry.QuantityMin}) cannot exceed QuantityMax ({entry.QuantityMax}). RewardId={entry.RewardId}, TableId={entry.RewardTableId}");
            }
        }

        private static void ValidateWeaponEntry(RewardEntryRow entry)
        {
            if (entry.TargetItemId <= 0)
            {
                throw new InvalidOperationException($"Invalid weapon entry: TargetItemId ({entry.TargetItemId}) must be greater than 0. RewardId={entry.RewardId}, TableId={entry.RewardTableId}");
            }
        }

        private static RewardEntryRow SelectWeighted(List<RewardEntryRow> entries, Random random)
        {
            int totalWeight = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                totalWeight += entries[i].Weight;
            }

            double roll = random.NextDouble() * totalWeight;
            double cumulative = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                cumulative += entries[i].Weight;
                if (roll < cumulative)
                {
                    return entries[i];
                }
            }

            return entries[entries.Count - 1];
        }
    }
}
