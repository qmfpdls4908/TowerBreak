using System;
using System.Collections.Generic;

namespace TowerBreak.Meta.Rewards
{
    public sealed class RewardBundle
    {
        public RewardBundle(int goldAmount, IReadOnlyList<int> grantedWeaponIds)
        {
            if (goldAmount < 0)
            {
                throw new ArgumentException("Gold amount cannot be negative.", nameof(goldAmount));
            }

            GoldAmount = goldAmount;
            GrantedWeaponIds = grantedWeaponIds ?? throw new ArgumentNullException(nameof(grantedWeaponIds));
        }

        public int GoldAmount { get; }
        public IReadOnlyList<int> GrantedWeaponIds { get; }
    }
}
