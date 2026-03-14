using System;

namespace TowerBreak.Meta.State
{
    public sealed class PlayerWalletState
    {
        private int gold;

        public PlayerWalletState(int initialGold = 0)
        {
            if (initialGold < 0)
            {
                throw new ArgumentException("Initial gold cannot be negative.", nameof(initialGold));
            }

            gold = initialGold;
        }

        public int Gold => gold;

        public void AddGold(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentException("Amount cannot be negative.", nameof(amount));
            }

            gold += amount;
        }

        public bool TryDeductGold(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentException("Amount cannot be negative.", nameof(amount));
            }

            if (gold < amount)
            {
                return false;
            }

            gold -= amount;
            return true;
        }
    }
}
