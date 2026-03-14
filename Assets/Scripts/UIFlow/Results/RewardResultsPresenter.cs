using System;
using System.Collections.Generic;

namespace TowerBreak.UIFlow.Results
{
    public sealed class RewardResultsPresenter
    {
        private readonly IRewardResultsStateReader _state;
        private readonly IRewardResultsFlowRouter _router;

        public RewardResultsPresenter(IRewardResultsStateReader state, IRewardResultsFlowRouter router)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _router = router ?? throw new ArgumentNullException(nameof(router));
        }

        public int GoldEarned => _state.GoldEarned;
        public IReadOnlyList<int> GrantedWeaponIds => _state.GrantedWeaponIds;
        public bool HasWeaponRewards => _state.GrantedWeaponIds.Count > 0;
        public int WeaponRewardCount => _state.GrantedWeaponIds.Count;

        public void OnContinue()
        {
            _router.Continue();
        }
    }
}
