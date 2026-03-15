using NUnit.Framework;
using System;
using TowerBreak.UIFlow.Battle;

namespace TowerBreak.UIFlow.Tests
{
    public sealed class BattleInputPresenterTests
    {
        private class MockBattleInputView : IBattleInputView
        {
            public event Action OnAttackClicked;
            public event Action OnGuardClicked;
            public event Action OnDashClicked;
            
            public bool AttackButtonEnabled { get; private set; }
            public bool GuardButtonEnabled { get; private set; }
            public bool DashButtonEnabled { get; private set; }
            
            public void SetAttackButtonEnabled(bool enabled)
            {
                AttackButtonEnabled = enabled;
            }
            
            public void SetGuardButtonEnabled(bool enabled)
            {
                GuardButtonEnabled = enabled;
            }
            
            public void SetDashButtonEnabled(bool enabled)
            {
                DashButtonEnabled = enabled;
            }
            
            public void SimulateAttackClick() => OnAttackClicked?.Invoke();
            public void SimulateGuardClick() => OnGuardClicked?.Invoke();
            public void SimulateDashClick() => OnDashClicked?.Invoke();
        }
        
        private class MockPlayerActionHandler : IPlayerActionHandler
        {
            public int CurrentActionId { get; set; } = PlayerActionIds.None;
            public bool IsActionInProgress => CurrentActionId != PlayerActionIds.None;
            public int LastPerformedActionId { get; set; } = PlayerActionIds.None;
            
            public bool CanPerformAction(int actionTypeId)
            {
                return actionTypeId != PlayerActionIds.None && !IsActionInProgress;
            }
            
            public void PerformAction(int actionTypeId)
            {
                if (CanPerformAction(actionTypeId))
                {
                    CurrentActionId = actionTypeId;
                    LastPerformedActionId = actionTypeId;
                }
            }
        }
        
        private MockBattleInputView mockView;
        private MockPlayerActionHandler mockHandler;
        private BattleInputPresenter presenter;
        
        [SetUp]
        public void Setup()
        {
            mockView = new MockBattleInputView();
            mockHandler = new MockPlayerActionHandler();
            presenter = new BattleInputPresenter(mockView, mockHandler);
        }
        
        [TearDown]
        public void TearDown()
        {
            presenter?.Dispose();
        }
        
        [Test]
        public void Constructor_InitializesButtonStates()
        {
            Assert.IsTrue(mockView.AttackButtonEnabled);
            Assert.IsTrue(mockView.GuardButtonEnabled);
            Assert.IsTrue(mockView.DashButtonEnabled);
        }
        
        [Test]
        public void OnAttackClicked_PerformsAttackAction()
        {
            mockView.SimulateAttackClick();
            
            Assert.AreEqual(PlayerActionIds.Attack, mockHandler.LastPerformedActionId);
        }
        
        [Test]
        public void OnGuardClicked_PerformsGuardAction()
        {
            mockView.SimulateGuardClick();
            
            Assert.AreEqual(PlayerActionIds.Guard, mockHandler.LastPerformedActionId);
        }
        
        [Test]
        public void OnDashClicked_PerformsDashAction()
        {
            mockView.SimulateDashClick();
            
            Assert.AreEqual(PlayerActionIds.Dash, mockHandler.LastPerformedActionId);
        }
        
        [Test]
        public void Update_WhenActionInProgress_DisablesButtons()
        {
            mockHandler.CurrentActionId = PlayerActionIds.Attack;
            
            presenter.Update();
            
            Assert.IsFalse(mockView.AttackButtonEnabled);
            Assert.IsFalse(mockView.GuardButtonEnabled);
            Assert.IsFalse(mockView.DashButtonEnabled);
        }
        
        [Test]
        public void OnAttackClicked_WhenActionInProgress_DoesNotPerform()
        {
            mockHandler.CurrentActionId = PlayerActionIds.Guard;
            mockHandler.LastPerformedActionId = PlayerActionIds.None;
            
            mockView.SimulateAttackClick();
            
            Assert.AreEqual(PlayerActionIds.None, mockHandler.LastPerformedActionId);
        }
        
        [Test]
        public void Dispose_UnsubscribesFromEvents()
        {
            presenter.Dispose();
            mockHandler.LastPerformedActionId = PlayerActionIds.None;
            
            mockView.SimulateAttackClick();
            
            Assert.AreEqual(PlayerActionIds.None, mockHandler.LastPerformedActionId);
        }
        
        [Test]
        public void Constructor_NullView_ThrowsArgumentNullException()
        {
            Assert.Throws<System.ArgumentNullException>(() =>
            {
                new BattleInputPresenter(null, mockHandler);
            });
        }
        
        [Test]
        public void Constructor_NullHandler_ThrowsArgumentNullException()
        {
            Assert.Throws<System.ArgumentNullException>(() =>
            {
                new BattleInputPresenter(mockView, null);
            });
        }
    }
}
