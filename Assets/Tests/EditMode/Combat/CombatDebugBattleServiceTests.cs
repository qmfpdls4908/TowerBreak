using System.Collections.Generic;

using NUnit.Framework;

namespace TowerBreak.Combat.Tests
{
    public sealed class CombatDebugBattleServiceTests
    {
        [Test]
        public void ApplyAttackToFirstEnemy_ReducesFirstEnemyHealth()
        {
            CombatDebugBattleService service = new(CreateState());

            CombatAttackResult result = service.ApplyAttackToFirstEnemy(attackDamage: 12);

            Assert.That(result.HasTarget, Is.True);
            Assert.That(result.TargetEnemyId, Is.EqualTo(101));
            Assert.That(result.TargetDefeated, Is.False);
            Assert.That(service.State.Enemies[0].Health, Is.EqualTo(18));
        }

        [Test]
        public void ApplyAttackToFirstEnemy_WhenFirstEnemyDies_RemovesItFromState()
        {
            CombatDebugBattleService service = new(CreateState());

            CombatAttackResult result = service.ApplyAttackToFirstEnemy(attackDamage: 30);

            Assert.That(result.HasTarget, Is.True);
            Assert.That(result.TargetEnemyId, Is.EqualTo(101));
            Assert.That(result.TargetDefeated, Is.True);
            Assert.That(service.State.Enemies.Count, Is.EqualTo(1));
            Assert.That(service.State.Enemies[0].EnemyId, Is.EqualTo(102));
        }

        [Test]
        public void ApplyAttackToFirstEnemy_WhenNoEnemies_ReturnsNoTargetResult()
        {
            CombatDebugBattleService service = new(CombatState.CreateInitial(3, 5, new List<CombatEnemyState>()));

            CombatAttackResult result = service.ApplyAttackToFirstEnemy(attackDamage: 10);

            Assert.That(result.HasTarget, Is.False);
            Assert.That(result.TargetEnemyId, Is.EqualTo(0));
        }

        [Test]
        public void AdvanceEnemyPressure_WhenThresholdCrossed_UpdatesStateAndReturnsWallDamageResult()
        {
            CombatDebugBattleService service = new(CreateState());

            CombatPressureResult result = service.AdvanceEnemyPressure(deltaTime: 1f, wallHitThreshold: 10f, wallDamagePerHit: 1);

            Assert.That(result.DidDamageWall, Is.True);
            Assert.That(result.WallDamageApplied, Is.EqualTo(1));
            Assert.That(result.EnteredDanger, Is.True);
            Assert.That(service.State.WallHealth, Is.EqualTo(4));
            Assert.That(service.State.IsDangerActive, Is.True);
        }

        [Test]
        public void AdvanceEnemyPressure_WhenWallFallsToZero_ReturnsWallDefeatedResult()
        {
            CombatDebugBattleService service = new(CombatState.CreateInitial(
                3,
                1,
                new List<CombatEnemyState>
                {
                    new(101, 30, 4.5f),
                    new(102, 65, 8f)
                }));

            CombatPressureResult result = service.AdvanceEnemyPressure(deltaTime: 1f, wallHitThreshold: 10f, wallDamagePerHit: 1);

            Assert.That(result.DefeatedWall, Is.True);
            Assert.That(service.State.IsWallDefeated, Is.True);
            Assert.That(service.State.WallHealth, Is.EqualTo(0));
        }

        [Test]
        public void ApplyPlayerGuard_ReducesPendingPressureAndReturnsResult()
        {
            CombatDebugBattleService service = new(CreateState());
            service.AdvanceEnemyPressure(deltaTime: 0.5f, wallHitThreshold: 10f, wallDamagePerHit: 1);

            CombatGuardResult result = service.ApplyPlayerGuard(pressureReduction: 3f);

            Assert.That(result.PressureReduced, Is.EqualTo(3f).Within(0.0001f));
            Assert.That(service.State.PendingEnemyPressure, Is.EqualTo(3.25f).Within(0.0001f));
        }

        private static CombatState CreateState()
        {
            return CombatState.CreateInitial(
                3,
                5,
                new List<CombatEnemyState>
                {
                    new(101, 30, 4.5f),
                    new(102, 65, 8f)
                });
        }
    }
}
