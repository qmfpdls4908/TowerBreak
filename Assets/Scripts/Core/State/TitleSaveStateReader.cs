using TowerBreak.UIFlow.Title;

namespace TowerBreak.Core.State
{
    public sealed class TitleSaveStateReader : ITitleSaveStateReader
    {
        private readonly PlayerSessionState _sessionState;

        public TitleSaveStateReader(PlayerSessionState sessionState)
        {
            _sessionState = sessionState;
        }

        public bool HasSaveData()
        {
            return _sessionState.IsRunActive;
        }
    }
}
