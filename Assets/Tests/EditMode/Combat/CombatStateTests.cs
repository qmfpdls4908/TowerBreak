using System.Collections.Generic;

using NUnit.Framework;

namespace TowerBreak.Combat.Tests
{
    public sealed class CombatStateTests
    {
        [Test]
        public void CreateInitialState_SetsPlayerWallAndEnemyValues()
        {
            List<CombatEnemyState> enemies = new()
            {
                new CombatEnemyState(101, 30, 4.5f),
                new CombatEnemyState(102, 65, 8f)
            };

            CombatState state = CombatState.CreateInitial(playerHealth: 3, wallHealth: 5, enemies: enemies);

            Assert.That(state.PlayerHealth, Is.EqualTo(3));
            Assert.That(state.WallHealth, Is.EqualTo(5));
            Assert.That(state.ElapsedTime, Is.EqualTo(0f));
            Assert.That(state.Enemies.Count, Is.EqualTo(2));
            Assert.That(state.Enemies[0].EnemyId, Is.EqualTo(101));
        }

        [Test]
        public void AdvanceTime_ReturnsNewStateWithUpdatedElapsedTime()
        {
            CombatState state = CombatState.CreateInitial(3, 5, new List<CombatEnemyState>());

            CombatState updated = state.AdvanceTime(0.5f);

            Assert.That(updated.ElapsedTime, Is.EqualTo(0.5f));
            Assert.That(state.ElapsedTime, Is.EqualTo(0f));
        }

        [Test]
        public void ApplyWallDamage_ReducesWallHealthButNotBelowZero()
        {
            CombatState state = CombatState.CreateInitial(3, 2, new List<CombatEnemyState>());

            CombatState updated = state.ApplyWallDamage(5);

            Assert.That(updated.WallHealth, Is.EqualTo(0));
        }

        [Test]
        public void ApplyPlayerAttack_ReducesTargetEnemyHealth()
        {
            CombatState state = CombatState.CreateInitial(
                3,
                5,
                new List<CombatEnemyState>
                {
                    new CombatEnemyState(101, 30, 4.5f),
                    new CombatEnemyState(102, 65, 8f)
                });

            CombatState updated = state.ApplyPlayerAttack(enemyId: 101, attackDamage: 12);

            Assert.That(updated.Enemies[0].Health, Is.EqualTo(18));
            Assert.That(updated.Enemies[1].Health, Is.EqualTo(65));
        }

        [Test]
        public void ApplyPlayerAttack_RemovesEnemyWhenHealthFallsToZeroOrBelow()
        {
            CombatState state = CombatState.CreateInitial(
                3,
                5,
                new List<CombatEnemyState>
                {
                    new CombatEnemyState(101, 10, 4.5f),
                    new CombatEnemyState(102, 65, 8f)
                });

            CombatState updated = state.ApplyPlayerAttack(enemyId: 101, attackDamage: 12);

            Assert.That(updated.Enemies.Count, Is.EqualTo(1));
            Assert.That(updated.Enemies[0].EnemyId, Is.EqualTo(102));
        }
    }
}
