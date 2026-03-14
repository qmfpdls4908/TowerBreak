using System;

using TowerBreak.Meta.State;

namespace TowerBreak.Meta.Rewards
{
    public static class BattleRewardService
    {
        public static void Apply(
            RewardBundle bundle,
            PlayerWalletState wallet,
            PlayerInventoryState inventory,
            Func<int> nextInstanceId)
        {
            if (bundle == null)
            {
                throw new ArgumentNullException(nameof(bundle));
            }

            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (inventory == null)
            {
                throw new ArgumentNullException(nameof(inventory));
            }

            if (nextInstanceId == null)
            {
                throw new ArgumentNullException(nameof(nextInstanceId));
            }

            wallet.AddGold(bundle.GoldAmount);

            for (int i = 0; i < bundle.GrantedWeaponIds.Count; i++)
            {
                int weaponId = bundle.GrantedWeaponIds[i];
                int instanceId = nextInstanceId();
                inventory.AddEquipment(new OwnedEquipment(instanceId, weaponId));
            }
        }
    }
}
