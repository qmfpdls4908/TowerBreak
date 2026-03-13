using System;
using System.Collections.Generic;
using System.Linq;

namespace TowerBreak.Combat
{
    public sealed class CombatState
    {
        private CombatState(int playerHealth, int wallHealth, float elapsedTime, IReadOnlyList<CombatEnemyState> enemies)
        {
            PlayerHealth = playerHealth;
            WallHealth = wallHealth;
            ElapsedTime = elapsedTime;
            Enemies = enemies;
        }

        public int PlayerHealth { get; }

        public int WallHealth { get; }

        public float ElapsedTime { get; }

        public IReadOnlyList<CombatEnemyState> Enemies { get; }

        public static CombatState CreateInitial(int playerHealth, int wallHealth, IReadOnlyList<CombatEnemyState> enemies)
        {
            if (enemies == null)
            {
                throw new ArgumentNullException(nameof(enemies));
            }

            return new CombatState(playerHealth, wallHealth, 0f, new List<CombatEnemyState>(enemies));
        }

        public CombatState AdvanceTime(float deltaTime)
        {
            return new CombatState(PlayerHealth, WallHealth, ElapsedTime + deltaTime, Enemies);
        }

        public CombatState ApplyWallDamage(int damage)
        {
            int nextWallHealth = WallHealth - damage;
            if (nextWallHealth < 0)
            {
                nextWallHealth = 0;
            }

            return new CombatState(PlayerHealth, nextWallHealth, ElapsedTime, Enemies);
        }

        public CombatState ApplyPlayerAttack(int enemyId, int attackDamage)
        {
            List<CombatEnemyState> updatedEnemies = new();

            for (int i = 0; i < Enemies.Count; i++)
            {
                CombatEnemyState enemy = Enemies[i];
                if (enemy.EnemyId != enemyId)
                {
                    updatedEnemies.Add(enemy);
                    continue;
                }

                int nextHealth = enemy.Health - attackDamage;
                if (nextHealth > 0)
                {
                    updatedEnemies.Add(new CombatEnemyState(enemy.EnemyId, nextHealth, enemy.Pressure));
                }
            }

            return new CombatState(PlayerHealth, WallHealth, ElapsedTime, updatedEnemies);
        }
    }
}
