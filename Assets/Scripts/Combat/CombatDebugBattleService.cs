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
    }
}
