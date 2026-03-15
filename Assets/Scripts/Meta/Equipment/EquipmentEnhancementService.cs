using System;
using System.Collections.Generic;

using TowerBreak.GameData.TowerBreaker;
using TowerBreak.Meta.State;

namespace TowerBreak.Meta.Equipment
{
    public static class EquipmentEnhancementService
    {
        public static bool TryEnhance(
            OwnedEquipment equipment,
            WeaponRow weaponRow,
            IReadOnlyList<EnhancementCostRow> enhancementCosts,
            PlayerWalletState wallet)
        {
            if (equipment == null)
            {
                throw new ArgumentNullException(nameof(equipment));
            }

            if (weaponRow == null)
            {
                throw new ArgumentNullException(nameof(weaponRow));
            }

            if (enhancementCosts == null)
            {
                throw new ArgumentNullException(nameof(enhancementCosts));
            }

            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            int nextLevel = equipment.EnhancementLevel + 1;
            EnhancementCostRow costRow = FindEnhancementCost(nextLevel, enhancementCosts);

            if (costRow == null)
            {
                return false;
            }

            if (!wallet.TryDeductGold(costRow.GoldCost))
            {
                return false;
            }

            equipment.IncrementEnhancementLevel();
            return true;
        }

        public static EnhancementCostRow FindEnhancementCost(int level, IReadOnlyList<EnhancementCostRow> enhancementCosts)
        {
            if (enhancementCosts == null)
            {
                throw new ArgumentNullException(nameof(enhancementCosts));
            }

            for (int i = 0; i < enhancementCosts.Count; i++)
            {
                if (enhancementCosts[i].Level == level)
                {
                    return enhancementCosts[i];
                }
            }

            return null;
        }

        public static int GetCurrentAttackPower(WeaponRow weaponRow, int enhancementLevel)
        {
            if (weaponRow == null)
            {
                throw new ArgumentNullException(nameof(weaponRow));
            }

            if (enhancementLevel <= 0)
            {
                return weaponRow.BaseAttack;
            }

            int totalBonus = 0;
            for (int level = 1; level <= enhancementLevel; level++)
            {
                totalBonus += GetAttackBonusForLevel(level);
            }

            return weaponRow.BaseAttack + totalBonus;
        }

        private static int GetAttackBonusForLevel(int level)
        {
            return 10 * level;
        }
    }
}
