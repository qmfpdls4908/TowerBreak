using TowerBreak.UIFlow.Lobby;

namespace TowerBreak.Core.State
{
    public sealed class LobbyStateReader : ILobbyStateReader
    {
        private readonly PlayerSessionState _sessionState;

        public LobbyStateReader(PlayerSessionState sessionState)
        {
            _sessionState = sessionState;
        }

        public int CurrentFloorId => _sessionState.CurrentFloorId;

        public int Gold => _sessionState.Wallet.Gold;

        public int? EquippedWeaponId
        {
            get
            {
                int? instanceId = _sessionState.Inventory.EquippedWeaponInstanceId;
                if (!instanceId.HasValue)
                {
                    return null;
                }

                if (_sessionState.Inventory.TryGetEquipment(instanceId.Value, out Meta.State.OwnedEquipment equipment))
                {
                    return equipment.WeaponId;
                }

                return null;
            }
        }
    }
}
