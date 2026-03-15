using System;
using UnityEngine;
using TowerBreak.Core;
using TowerBreak.UIFlow.Lobby;

namespace TowerBreak.Core.Router
{
    public sealed class LobbyFlowRouter : ILobbyFlowRouter
    {
        private readonly ISceneLoader _sceneLoader;
        private readonly PlayerSessionState _sessionState;
        private readonly LobbySceneInitializer _lobbyInitializer;

        public LobbyFlowRouter(ISceneLoader sceneLoader, PlayerSessionState sessionState, LobbySceneInitializer lobbyInitializer = null)
        {
            _sceneLoader = sceneLoader ?? throw new ArgumentNullException(nameof(sceneLoader));
            _sessionState = sessionState ?? throw new ArgumentNullException(nameof(sessionState));
            _lobbyInitializer = lobbyInitializer;
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
            Debug.Log("[LobbyFlowRouter] Opening equipment screen");
            _lobbyInitializer?.CreateEquipmentUI();
        }

        public void CloseEquipment()
        {
            Debug.Log("[LobbyFlowRouter] Closing equipment screen");
            _lobbyInitializer?.CloseEquipmentUI();
        }

        public void OpenReroll()
        {
            Debug.Log("[LobbyFlowRouter] Opening reroll screen");
            // TODO: Reroll 화면 활성화
        }

        public void CloseReroll()
        {
            Debug.Log("[LobbyFlowRouter] Closing reroll screen");
            // TODO: Reroll 화면 비활성화
        }
    }
}
