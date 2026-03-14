using System;
using TowerBreak.Meta.Rewards;
using TowerBreak.Meta.State;

namespace TowerBreak.Core
{
    public sealed class PlayerSessionState
    {
        private readonly PlayerInventoryState _inventory;
        private readonly PlayerWalletState _wallet;

        public PlayerInventoryState Inventory => _inventory;
        public PlayerWalletState Wallet => _wallet;
        public int CurrentFloorId { get; private set; }
        public bool IsRunActive { get; private set; }
        public int? PendingBattleFloorId { get; private set; }
        public RewardBundle LastBattleReward { get; private set; }

        public PlayerSessionState(PlayerInventoryState inventory, PlayerWalletState wallet)
        {
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            CurrentFloorId = 1;
            IsRunActive = false;
        }

        public void StartNewRun()
        {
            CurrentFloorId = 1;
            IsRunActive = true;
        }

        public void ContinueRun(int floorId)
        {
            if (floorId < 1)
            {
                throw new ArgumentException("Floor ID must be >= 1", nameof(floorId));
            }

            CurrentFloorId = floorId;
            IsRunActive = true;
        }

        public void AdvanceToNextFloor()
        {
            if (!IsRunActive)
            {
                throw new InvalidOperationException("Cannot advance floor when no run is active.");
            }

            CurrentFloorId++;
        }

        public void EndRun()
        {
            IsRunActive = false;
        }

        public void SetPendingBattleFloorId(int floorId)
        {
            if (floorId < 1)
            {
                throw new ArgumentException("Floor ID must be >= 1", nameof(floorId));
            }

            PendingBattleFloorId = floorId;
        }

        public void ClearPendingBattleFloorId()
        {
            PendingBattleFloorId = null;
        }

        public void SetLastBattleReward(RewardBundle reward)
        {
            LastBattleReward = reward ?? throw new ArgumentNullException(nameof(reward));
        }

        public void ClearLastBattleReward()
        {
            LastBattleReward = null;
        }
    }
}
