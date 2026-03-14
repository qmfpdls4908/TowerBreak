using System.Collections.Generic;
using TowerBreak.UIFlow.Results;

namespace TowerBreak.Core.State
{
    public sealed class RewardResultsStateReader : IRewardResultsStateReader
    {
        private readonly PlayerSessionState _sessionState;

        public RewardResultsStateReader(PlayerSessionState sessionState)
        {
            _sessionState = sessionState ?? throw new System.ArgumentNullException(nameof(sessionState));
        }

        public int GoldEarned => _sessionState.LastBattleReward?.GoldAmount ?? 0;

        public IReadOnlyList<int> GrantedWeaponIds => _sessionState.LastBattleReward?.GrantedWeaponIds ?? new List<int>();
    }
}
