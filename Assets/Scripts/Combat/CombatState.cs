using System;
using System.Collections.Generic;

namespace TowerBreak.Combat
{
    public sealed class CombatState
    {
        private CombatState(
            int playerHealth,
            int wallHealth,
            float elapsedTime,
            float pendingEnemyPressure,
            bool isDangerActive,
            bool isWallDefeated,
            bool isPlayerDefeated,
            IReadOnlyList<CombatEnemyState> enemies)
        {
            PlayerHealth = playerHealth;
            WallHealth = wallHealth;
            ElapsedTime = elapsedTime;
            PendingEnemyPressure = pendingEnemyPressure;
            IsDangerActive = isDangerActive;
            IsWallDefeated = isWallDefeated;
            IsPlayerDefeated = isPlayerDefeated;
            Enemies = enemies;
        }

        public int PlayerHealth { get; }

        public int WallHealth { get; }

        public float ElapsedTime { get; }

        public float PendingEnemyPressure { get; }

        public bool IsDangerActive { get; }

        public bool IsWallDefeated { get; }

        public bool IsPlayerDefeated { get; }

        public IReadOnlyList<CombatEnemyState> Enemies { get; }

        public static CombatState CreateInitial(int playerHealth, int wallHealth, IReadOnlyList<CombatEnemyState> enemies)
        {
            if (enemies == null)
            {
                throw new ArgumentNullException(nameof(enemies));
            }

            return new CombatState(playerHealth, wallHealth, 0f, 0f, false, wallHealth <= 0, playerHealth <= 0, new List<CombatEnemyState>(enemies));
        }

        public CombatState AdvanceTime(float deltaTime)
        {
            return new CombatState(PlayerHealth, WallHealth, ElapsedTime + deltaTime, PendingEnemyPressure, IsDangerActive, IsWallDefeated, IsPlayerDefeated, Enemies);
        }

        public CombatState ApplyWallDamage(int damage)
        {
            int nextWallHealth = WallHealth - damage;
            if (nextWallHealth < 0)
            {
                nextWallHealth = 0;
            }

            return new CombatState(PlayerHealth, nextWallHealth, ElapsedTime, PendingEnemyPressure, IsDangerActive, nextWallHealth == 0, IsPlayerDefeated, Enemies);
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

            return new CombatState(PlayerHealth, WallHealth, ElapsedTime, PendingEnemyPressure, IsDangerActive, IsWallDefeated, IsPlayerDefeated, updatedEnemies);
        }

        public CombatState ApplyPlayerGuard(float pressureReduction)
        {
            if (pressureReduction < 0f)
            {
                throw new ArgumentException("Pressure reduction must be zero or greater.", nameof(pressureReduction));
            }

            float nextPendingEnemyPressure = PendingEnemyPressure - pressureReduction;
            if (nextPendingEnemyPressure < 0f)
            {
                nextPendingEnemyPressure = 0f;
            }

            return new CombatState(PlayerHealth, WallHealth, ElapsedTime, nextPendingEnemyPressure, IsDangerActive, IsWallDefeated, IsPlayerDefeated, Enemies);
        }

        public CombatState AdvanceEnemyPressure(float deltaTime, float wallHitThreshold, int wallDamagePerHit)
        {
            if (deltaTime < 0f)
            {
                throw new ArgumentException("Delta time must be zero or greater.", nameof(deltaTime));
            }

            if (wallHitThreshold <= 0f)
            {
                throw new ArgumentException("Wall hit threshold must be greater than zero.", nameof(wallHitThreshold));
            }

            if (wallDamagePerHit <= 0)
            {
                throw new ArgumentException("Wall damage per hit must be greater than zero.", nameof(wallDamagePerHit));
            }

            float totalEnemyPressure = 0f;
            for (int i = 0; i < Enemies.Count; i++)
            {
                totalEnemyPressure += Enemies[i].Pressure;
            }

            float nextPendingEnemyPressure = PendingEnemyPressure + totalEnemyPressure * deltaTime;
            int wallHitCount = (int)(nextPendingEnemyPressure / wallHitThreshold);
            if (wallHitCount > 0)
            {
                nextPendingEnemyPressure -= wallHitCount * wallHitThreshold;
            }

            int nextWallHealth = WallHealth - wallHitCount * wallDamagePerHit;
            if (nextWallHealth < 0)
            {
                nextWallHealth = 0;
            }

            bool nextDangerActive = IsDangerActive || wallHitCount > 0;
            bool nextWallDefeated = IsWallDefeated || nextWallHealth == 0;

            return new CombatState(
                PlayerHealth,
                nextWallHealth,
                ElapsedTime,
                nextPendingEnemyPressure,
                nextDangerActive,
                nextWallDefeated,
                IsPlayerDefeated,
                Enemies);
        }

        public CombatState ApplyPlayerDamage(int damage)
        {
            if (damage < 0)
            {
                throw new ArgumentException("Damage must be zero or greater.", nameof(damage));
            }

            int nextPlayerHealth = PlayerHealth - damage;
            if (nextPlayerHealth < 0)
            {
                nextPlayerHealth = 0;
            }

            bool nextPlayerDefeated = IsPlayerDefeated || nextPlayerHealth == 0;

            return new CombatState(
                nextPlayerHealth,
                WallHealth,
                ElapsedTime,
                PendingEnemyPressure,
                IsDangerActive,
                IsWallDefeated,
                nextPlayerDefeated,
                Enemies);
        }
    }
}
