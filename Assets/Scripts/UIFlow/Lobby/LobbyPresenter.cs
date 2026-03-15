using System;

using TowerBreak.Meta.State;

namespace TowerBreak.UIFlow.Lobby
{
    public sealed class LobbyPresenter
    {
        private readonly ILobbyFlowRouter _router;
        private readonly ILobbyStateReader _state;
        private readonly PlayerWalletState _wallet;

        public LobbyPresenter(ILobbyFlowRouter router, ILobbyStateReader state, PlayerWalletState wallet)
        {
            _router = router ?? throw new ArgumentNullException(nameof(router));
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
        }

        public int CurrentFloorId => _state.CurrentFloorId;
        public int Gold => _state.Gold;
        public bool HasEquippedWeapon => _state.EquippedWeaponId.HasValue;
        public int? EquippedWeaponId => _state.EquippedWeaponId;
        public PlayerWalletState WalletState => _wallet;

        public void OnOpenChallenge()
        {
            _router.OpenChallenge();
        }

        public void OnOpenEquipment()
        {
            _router.OpenEquipment();
        }

        public void OnOpenReroll()
        {
            _router.OpenReroll();
        }
    }
}
