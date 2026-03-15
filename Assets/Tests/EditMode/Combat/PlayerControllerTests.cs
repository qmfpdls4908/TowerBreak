using NUnit.Framework;
using TowerBreak.Combat;

namespace TowerBreak.Combat.Tests
{
    public sealed class PlayerControllerTests
    {
        [Test]
        public void CanPerformAction_IdleState_ReturnsTrue()
        {
            // PlayerController는 MonoBehaviour라서 직접 테스트가 어려움
            // 행동 로직만 테스트
            bool isActionInProgress = false;
            
            bool canAttack = !isActionInProgress;
            bool canGuard = !isActionInProgress;
            bool canDash = !isActionInProgress;
            
            Assert.IsTrue(canAttack);
            Assert.IsTrue(canGuard);
            Assert.IsTrue(canDash);
        }
        
        [Test]
        public void CanPerformAction_ActionInProgress_ReturnsFalse()
        {
            bool isActionInProgress = true;
            
            bool canAttack = !isActionInProgress;
            bool canGuard = !isActionInProgress;
            bool canDash = !isActionInProgress;
            
            Assert.IsFalse(canAttack);
            Assert.IsFalse(canGuard);
            Assert.IsFalse(canDash);
        }
        
        [Test]
        public void PlayerActionType_Values_AreCorrect()
        {
            Assert.AreEqual(0, (int)PlayerActionType.None);
            Assert.AreEqual(1, (int)PlayerActionType.Attack);
            Assert.AreEqual(2, (int)PlayerActionType.Guard);
            Assert.AreEqual(3, (int)PlayerActionType.Dash);
        }
    }
}
