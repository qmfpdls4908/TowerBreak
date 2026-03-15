using System;

namespace TowerBreak.UIFlow.Battle
{
    public interface IBattleInputView
    {
        event Action OnAttackClicked;
        event Action OnGuardClicked;
        event Action OnDashClicked;
        
        void SetAttackButtonEnabled(bool enabled);
        void SetGuardButtonEnabled(bool enabled);
        void SetDashButtonEnabled(bool enabled);
    }
}
