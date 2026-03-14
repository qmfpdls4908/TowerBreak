using System;

namespace TowerBreak.UIFlow.Growth
{
    public sealed class GrowthScreenPresenter
    {
        private readonly IGrowthStateReader _state;
        private readonly IGrowthFlowRouter _router;

        public GrowthScreenPresenter(IGrowthStateReader state, IGrowthFlowRouter router)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _router = router ?? throw new ArgumentNullException(nameof(router));
        }

        public bool HasEquippedWeapon => _state.EquippedWeaponId.HasValue;
        public bool HasCandidateWeapon => _state.CandidateWeaponId.HasValue;
        public int? EquippedWeaponId => _state.EquippedWeaponId;
        public int? CandidateWeaponId => _state.CandidateWeaponId;
        public bool IsComparisonAvailable => HasEquippedWeapon && HasCandidateWeapon;
        public int DeltaAttack => _state.DeltaAttack;
        public float DeltaAttackSpeed => _state.DeltaAttackSpeed;
        public float DeltaPushPower => _state.DeltaPushPower;

        public void OnEquip()
        {
            _router.Equip();
        }

        public void OnReroll()
        {
            _router.Reroll();
        }

        public void OnContinue()
        {
            _router.Continue();
        }
    }
}
