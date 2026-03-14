using System;

namespace TowerBreak.UIFlow.Battle
{
    public sealed class BattleHudView
    {
        public int DisplayedPlayerHealth { get; private set; }
        public string DisplayedFloorLabel { get; private set; }
        public string DisplayedWaveLabel { get; private set; }
        public bool IsDangerBannerVisible { get; private set; }
        public string DangerBannerText { get; private set; }

        public void Refresh(BattleHudPresenter presenter)
        {
            if (presenter == null)
            {
                throw new ArgumentNullException(nameof(presenter));
            }

            DisplayedPlayerHealth = presenter.PlayerHealth;
            DisplayedFloorLabel = presenter.FloorLabel;
            DisplayedWaveLabel = presenter.WaveLabel;
            IsDangerBannerVisible = presenter.IsDangerBannerVisible;
            DangerBannerText = presenter.DangerBannerText;
        }
    }
}
