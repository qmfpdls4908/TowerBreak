using System.Collections.Generic;

namespace TowerBreak.UIFlow.Results
{
    public interface IRewardResultsStateReader
    {
        int GoldEarned { get; }
        IReadOnlyList<int> GrantedWeaponIds { get; }
    }
}
