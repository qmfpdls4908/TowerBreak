using System;

namespace TowerBreak.UIFlow.Growth
{
    public sealed class GrowthScreenView
    {
        public bool IsComparisonAvailable { get; private set; }
        public int? DisplayedEquippedWeaponId { get; private set; }
        public int? DisplayedCandidateWeaponId { get; private set; }
        public int DisplayedDeltaAttack { get; private set; }

        public void Refresh(GrowthScreenPresenter presenter)
        {
            if (presenter == null)
            {
                throw new ArgumentNullException(nameof(presenter));
            }

            IsComparisonAvailable = presenter.IsComparisonAvailable;
            DisplayedEquippedWeaponId = presenter.EquippedWeaponId;
            DisplayedCandidateWeaponId = presenter.CandidateWeaponId;
            DisplayedDeltaAttack = presenter.DeltaAttack;
        }
    }
}
