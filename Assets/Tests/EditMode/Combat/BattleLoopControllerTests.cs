using System.Collections.Generic;

using NUnit.Framework;

namespace TowerBreak.Combat.Tests
{
    public sealed class BattleLoopControllerTests
    {
        [Test]
        public void Update_WhenIntervalNotReached_DoesNotAdvanceEnemyPressure()
        {
            BattleLoopController controller = CreateController();

            BattleLoopUpdateResult result = controller.Update(deltaTime: 0.25f);

            Assert.That(result.DidAdvancePressure, Is.False);
            Assert.That(controller.State.PendingEnemyPressure, Is.EqualTo(0f));
            Assert.That(controller.State.WallHealth, Is.EqualTo(5));
        }

        [Test]
        public void Update_WhenIntervalReached_AdvancesEnemyPressureAndReturnsResult()
        {
            BattleLoopController controller = CreateController();

            BattleLoopUpdateResult result = controller.Update(deltaTime: 1f);

            Assert.That(result.DidAdvancePressure, Is.True);
            Assert.That(result.PressureResult.DidDamageWall, Is.True);
            Assert.That(result.PressureResult.WallDamageApplied, Is.EqualTo(1));
            Assert.That(controller.State.WallHealth, Is.EqualTo(4));
            Assert.That(controller.State.IsDangerActive, Is.True);
        }

        [Test]
        public void Update_WhenDeltaTimeIsNegative_ThrowsArgumentException()
        {
            BattleLoopController controller = CreateController();

            Assert.That(
                () => controller.Update(deltaTime: -0.1f),
                Throws.ArgumentException.With.Message.Contains("Delta time must be zero or greater."));
        }

        [Test]
        public void ApplyAttackToFirstEnemy_WhenWallAlreadyDefeated_ReturnsNoTarget()
        {
            BattleLoopController controller = CreateController(initialWallHealth: 0);

            CombatAttackResult result = controller.ApplyAttackToFirstEnemy(attackDamage: 10);

            Assert.That(result.HasTarget, Is.False);
            Assert.That(controller.State.Enemies.Count, Is.EqualTo(2));
        }

        [Test]
        public void ApplyGuard_WhenWallAlreadyDefeated_ReturnsZeroReduction()
        {
            BattleLoopController controller = CreateController(initialWallHealth: 0);

            CombatGuardResult result = controller.ApplyPlayerGuard(pressureReduction: 5f);

            Assert.That(result.PressureReduced, Is.EqualTo(0f));
            Assert.That(controller.State.PendingEnemyPressure, Is.EqualTo(0f));
        }

        private static BattleLoopController CreateController(int initialWallHealth = 5)
        {
            CombatState state = CombatState.CreateInitial(
                playerHealth: 3,
                wallHealth: initialWallHealth,
                enemies: new List<CombatEnemyState>
                {
                    new(101, 30, 4.5f),
                    new(102, 65, 8f)
                });

            return new BattleLoopController(
                new CombatDebugBattleService(state),
                pressureTickInterval: 0.5f,
                wallHitThreshold: 10f,
                wallDamagePerHit: 1);
        }
    }
}
