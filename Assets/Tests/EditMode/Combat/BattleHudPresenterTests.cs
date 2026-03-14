using System.Collections.Generic;

using NUnit.Framework;

namespace TowerBreak.Combat.Tests
{
    public sealed class BattleHudPresenterTests
    {
        [Test]
        public void BuildStatusMessage_WhenDangerActive_ReturnsDangerSummary()
        {
            CombatState state = CombatState.CreateInitial(
                3,
                4,
                new List<CombatEnemyState>
                {
                    new CombatEnemyState(101, 10, 1f)
                });
            state = state.AdvanceEnemyPressure(deltaTime: 1f, wallHitThreshold: 1f, wallDamagePerHit: 1);

            string message = BattleHudPresenter.BuildStatusMessage(state);

            Assert.That(message, Does.Contain("DANGER"));
            Assert.That(message, Does.Contain("Wall 3"));
        }

        [Test]
        public void BuildStatusMessage_WhenWallDefeated_ReturnsDefeatSummary()
        {
            CombatState state = CombatState.CreateInitial(3, 1, new List<CombatEnemyState>());
            state = state.ApplyWallDamage(1);

            string message = BattleHudPresenter.BuildStatusMessage(state);

            Assert.That(message, Does.Contain("DEFEAT"));
            Assert.That(message, Does.Contain("Wall 0"));
        }

        [Test]
        public void BuildStatusMessage_WhenStateIsSafe_ReturnsEmpty()
        {
            CombatState state = CombatState.CreateInitial(3, 5, new List<CombatEnemyState>());

            string message = BattleHudPresenter.BuildStatusMessage(state);

            Assert.That(message, Is.Empty);
        }

        [Test]
        public void BuildStatusMessage_WhenStateIsNull_ReturnsEmpty()
        {
            string message = BattleHudPresenter.BuildStatusMessage(null);

            Assert.That(message, Is.Empty);
        }
    }
}
