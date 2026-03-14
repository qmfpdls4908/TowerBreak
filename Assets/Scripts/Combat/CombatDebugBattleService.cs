using System;

namespace TowerBreak.Combat
{
    public sealed class CombatDebugBattleService
    {
        public CombatDebugBattleService(CombatState state)
        {
            State = state ?? throw new ArgumentNullException(nameof(state));
        }

        public CombatState State { get; private set; }

        public CombatAttackResult ApplyAttackToFirstEnemy(int attackDamage)
        {
            if (State.Enemies.Count == 0)
            {
                return new CombatAttackResult(false, 0, false);
            }

            CombatEnemyState firstEnemy = State.Enemies[0];
            bool defeated = firstEnemy.Health <= attackDamage;
            State = State.ApplyPlayerAttack(firstEnemy.EnemyId, attackDamage);
            return new CombatAttackResult(true, firstEnemy.EnemyId, defeated);
        }

        public CombatGuardResult ApplyPlayerGuard(float pressureReduction)
        {
            float previousPressure = State.PendingEnemyPressure;
            State = State.ApplyPlayerGuard(pressureReduction);
            float actualReduction = previousPressure - State.PendingEnemyPressure;
            return new CombatGuardResult(actualReduction);
        }

        public CombatPressureResult AdvanceEnemyPressure(float deltaTime, float wallHitThreshold, int wallDamagePerHit)
        {
            CombatState previousState = State;
            State = State.AdvanceEnemyPressure(deltaTime, wallHitThreshold, wallDamagePerHit);

            int wallDamageApplied = previousState.WallHealth - State.WallHealth;
            bool enteredDanger = !previousState.IsDangerActive && State.IsDangerActive;
            bool defeatedWall = !previousState.IsWallDefeated && State.IsWallDefeated;

            return new CombatPressureResult(wallDamageApplied > 0, wallDamageApplied, enteredDanger, defeatedWall);
        }
    }
}
