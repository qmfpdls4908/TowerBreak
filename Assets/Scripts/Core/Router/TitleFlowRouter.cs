using System;
using TowerBreak.Core;
using TowerBreak.UIFlow.Title;

namespace TowerBreak.Core.Router
{
    public sealed class TitleFlowRouter : ITitleFlowRouter
    {
        private readonly ISceneLoader _sceneLoader;
        private readonly PlayerSessionState _sessionState;

        public TitleFlowRouter(ISceneLoader sceneLoader, PlayerSessionState sessionState)
        {
            _sceneLoader = sceneLoader ?? throw new ArgumentNullException(nameof(sceneLoader));
            _sessionState = sessionState ?? throw new ArgumentNullException(nameof(sessionState));
        }

        public void StartNewGame()
        {
            _sessionState.StartNewRun();
            _sceneLoader.LoadScene("Lobby");
        }

        public void ContinueGame()
        {
            if (!_sessionState.IsRunActive)
            {
                throw new InvalidOperationException("Cannot continue game when no run is active.");
            }

            _sceneLoader.LoadScene("Lobby");
        }
    }
}
