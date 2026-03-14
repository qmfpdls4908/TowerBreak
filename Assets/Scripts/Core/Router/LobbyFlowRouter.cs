using System;
using TowerBreak.Core;
using TowerBreak.UIFlow.Lobby;

namespace TowerBreak.Core.Router
{
    public sealed class LobbyFlowRouter : ILobbyFlowRouter
    {
        private readonly ISceneLoader _sceneLoader;
        private readonly PlayerSessionState _sessionState;

        public LobbyFlowRouter(ISceneLoader sceneLoader, PlayerSessionState sessionState)
        {
            _sceneLoader = sceneLoader ?? throw new ArgumentNullException(nameof(sceneLoader));
            _sessionState = sessionState ?? throw new ArgumentNullException(nameof(sessionState));
        }

        public void OpenChallenge()
        {
            if (!_sessionState.IsRunActive)
            {
                throw new InvalidOperationException("Cannot open challenge when no run is active.");
            }

            _sessionState.SetPendingBattleFloorId(_sessionState.CurrentFloorId);
            _sceneLoader.LoadScene("Battle");
        }

        public void OpenEquipment()
        {
            // TODO: Open equipment screen (Phase 2)
        }

        public void OpenReroll()
        {
            // TODO: Open reroll screen (Phase 2)
        }
    }
}
