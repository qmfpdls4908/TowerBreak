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
        public void ApplyWallDamage_WhenWallReachesZero_SetsWallDefeated()
        {
            CombatState state = CombatState.CreateInitial(3, 1, new List<CombatEnemyState>());

            CombatState updated = state.ApplyWallDamage(1);

            Assert.That(updated.IsWallDefeated, Is.True);
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

        [Test]
        public void AdvanceEnemyPressure_BeforeThreshold_AccumulatesPressureWithoutWallDamage()
        {
            CombatState state = CombatState.CreateInitial(
                3,
                5,
                new List<CombatEnemyState>
                {
                    new CombatEnemyState(101, 30, 4.5f),
                    new CombatEnemyState(102, 65, 8f)
                });

            CombatState updated = state.AdvanceEnemyPressure(deltaTime: 0.5f, wallHitThreshold: 10f, wallDamagePerHit: 1);

            Assert.That(updated.PendingEnemyPressure, Is.EqualTo(6.25f).Within(0.0001f));
            Assert.That(updated.WallHealth, Is.EqualTo(5));
            Assert.That(updated.IsDangerActive, Is.False);
        }

        [Test]
        public void AdvanceEnemyPressure_WhenThresholdCrossed_ReducesWallHealthAndActivatesDanger()
        {
            CombatState state = CombatState.CreateInitial(
                3,
                5,
                new List<CombatEnemyState>
                {
                    new CombatEnemyState(101, 30, 4.5f),
                    new CombatEnemyState(102, 65, 8f)
                });

            CombatState updated = state.AdvanceEnemyPressure(deltaTime: 1f, wallHitThreshold: 10f, wallDamagePerHit: 1);

            Assert.That(updated.PendingEnemyPressure, Is.EqualTo(2.5f).Within(0.0001f));
            Assert.That(updated.WallHealth, Is.EqualTo(4));
            Assert.That(updated.IsDangerActive, Is.True);
        }

        [Test]
        public void AdvanceEnemyPressure_WhenWallHealthFallsToZero_SetsWallDefeated()
        {
            CombatState state = CombatState.CreateInitial(
                3,
                1,
                new List<CombatEnemyState>
                {
                    new CombatEnemyState(101, 30, 4.5f),
                    new CombatEnemyState(102, 65, 8f)
                });

            CombatState updated = state.AdvanceEnemyPressure(deltaTime: 1f, wallHitThreshold: 10f, wallDamagePerHit: 1);

            Assert.That(updated.WallHealth, Is.EqualTo(0));
            Assert.That(updated.IsWallDefeated, Is.True);
        }

        [Test]
        public void ApplyPlayerGuard_ReducesPendingPressure()
        {
            CombatState state = CombatState.CreateInitial(
                3,
                5,
                new List<CombatEnemyState>
                {
                    new CombatEnemyState(101, 30, 4.5f),
                    new CombatEnemyState(102, 65, 8f)
                });
            state = state.AdvanceEnemyPressure(deltaTime: 0.5f, wallHitThreshold: 10f, wallDamagePerHit: 1);

            CombatState updated = state.ApplyPlayerGuard(pressureReduction: 3f);

            Assert.That(updated.PendingEnemyPressure, Is.EqualTo(3.25f).Within(0.0001f));
            Assert.That(updated.WallHealth, Is.EqualTo(5));
            Assert.That(updated.IsDangerActive, Is.False);
        }

        [Test]
        public void ApplyPlayerGuard_WhenReductionExceedsPending_ClampsToZero()
        {
            CombatState state = CombatState.CreateInitial(
                3,
                5,
                new List<CombatEnemyState>
                {
                    new CombatEnemyState(101, 30, 4.5f),
                    new CombatEnemyState(102, 65, 8f)
                });
            state = state.AdvanceEnemyPressure(deltaTime: 0.5f, wallHitThreshold: 10f, wallDamagePerHit: 1);

            CombatState updated = state.ApplyPlayerGuard(pressureReduction: 20f);

            Assert.That(updated.PendingEnemyPressure, Is.EqualTo(0f));
        }

        [Test]
        public void ApplyPlayerGuard_WhenDangerActive_DoesNotClearDangerFlag()
        {
            CombatState state = CombatState.CreateInitial(
                3,
                5,
                new List<CombatEnemyState>
                {
                    new CombatEnemyState(101, 30, 4.5f),
                    new CombatEnemyState(102, 65, 8f)
                });
            state = state.AdvanceEnemyPressure(deltaTime: 1f, wallHitThreshold: 10f, wallDamagePerHit: 1);
            Assert.That(state.IsDangerActive, Is.True);

            CombatState updated = state.ApplyPlayerGuard(pressureReduction: 100f);

            Assert.That(updated.IsDangerActive, Is.True);
            Assert.That(updated.PendingEnemyPressure, Is.EqualTo(0f));
        }
    }
}
