using System;

namespace TowerBreak.UIFlow.Battle
{
    public sealed class BattleHudPresenter
    {
        private readonly IBattleHudStateReader _state;

        public BattleHudPresenter(IBattleHudStateReader state)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
        }

        public int PlayerHealth => _state.PlayerHealth;

        public string FloorLabel => $"Floor {_state.FloorId}";

        public string WaveLabel => $"Wave {_state.WaveNumber}";

        public bool IsDangerBannerVisible => _state.IsDangerActive || _state.IsWallDefeated;

        public string DangerBannerText
        {
            get
            {
                if (_state.IsWallDefeated)
                {
                    return "DEFEAT";
                }

                if (_state.IsDangerActive)
                {
                    return "DANGER";
                }

                return string.Empty;
            }
        }
    }
}
