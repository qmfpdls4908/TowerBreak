using System;
using TowerBreak.EventBus;
using TowerBreak.DI;

namespace TowerBreak.UIFlow.Battle
{
    public interface IPlayerActionHandler
    {
        bool CanPerformAction(int actionTypeId);
        void PerformAction(int actionTypeId);
        int CurrentActionId { get; }
        bool IsActionInProgress { get; }
    }
    
    public static class PlayerActionIds
    {
        public const int None = 0;
        public const int Attack = 1;
        public const int Guard = 2;
        public const int Dash = 3;
    }
    
    public sealed class BattleInputPresenter : IDisposable
    {
        private readonly IBattleInputView _view;
        private readonly IPlayerActionHandler _playerActionHandler;
        private readonly EventBus<PlayerActionEvent> _playerActionEventBus;
        
        public BattleInputPresenter(
            IBattleInputView view,
            IPlayerActionHandler playerActionHandler)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _playerActionHandler = playerActionHandler ?? throw new ArgumentNullException(nameof(playerActionHandler));
            
            // EventBus 인스턴스를 DI에서 가져오기
            _playerActionEventBus = DIContainer.ResolveFromRegistered<EventBus<PlayerActionEvent>>();
            
            _view.OnAttackClicked += HandleAttackClicked;
            _view.OnGuardClicked += HandleGuardClicked;
            _view.OnDashClicked += HandleDashClicked;
            
            UpdateButtonStates();
        }
        
        public void Update()
        {
            UpdateButtonStates();
        }
        
        private void UpdateButtonStates()
        {
            _view.SetAttackButtonEnabled(_playerActionHandler.CanPerformAction(PlayerActionIds.Attack));
            _view.SetGuardButtonEnabled(_playerActionHandler.CanPerformAction(PlayerActionIds.Guard));
            _view.SetDashButtonEnabled(_playerActionHandler.CanPerformAction(PlayerActionIds.Dash));
        }
        
        private void HandleAttackClicked()
        {
            PublishAction(PlayerActionIds.Attack);
        }
        
        private void HandleGuardClicked()
        {
            PublishAction(PlayerActionIds.Guard);
        }
        
        private void HandleDashClicked()
        {
            PublishAction(PlayerActionIds.Dash);
        }
        
        private void PublishAction(int actionTypeId)
        {
            if (_playerActionHandler.CanPerformAction(actionTypeId))
            {
                if (_playerActionEventBus != null)
                {
                    _playerActionEventBus.Publish(new PlayerActionEvent(actionTypeId));
                }
                _playerActionHandler.PerformAction(actionTypeId);
                UpdateButtonStates();
            }
        }
        
        public void Dispose()
        {
            _view.OnAttackClicked -= HandleAttackClicked;
            _view.OnGuardClicked -= HandleGuardClicked;
            _view.OnDashClicked -= HandleDashClicked;
        }
    }
}
