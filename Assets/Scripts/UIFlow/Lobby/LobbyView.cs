using System;

namespace TowerBreak.UIFlow.Lobby
{
    public sealed class LobbyView
    {
        public int DisplayedFloorId { get; private set; }
        public int DisplayedGold { get; private set; }
        public bool IsWeaponEquipped { get; private set; }

        public void Refresh(LobbyPresenter presenter)
        {
            if (presenter == null)
            {
                throw new ArgumentNullException(nameof(presenter));
            }

            DisplayedFloorId = presenter.CurrentFloorId;
            DisplayedGold = presenter.Gold;
            IsWeaponEquipped = presenter.HasEquippedWeapon;
        }
    }
}
