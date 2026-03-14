using System;

namespace TowerBreak.UIFlow.Results
{
    public sealed class RewardResultsView
    {
        public int DisplayedGold { get; private set; }
        public int DisplayedWeaponRewardCount { get; private set; }

        public void Refresh(RewardResultsPresenter presenter)
        {
            if (presenter == null)
            {
                throw new ArgumentNullException(nameof(presenter));
            }

            DisplayedGold = presenter.GoldEarned;
            DisplayedWeaponRewardCount = presenter.WeaponRewardCount;
        }
    }
}
