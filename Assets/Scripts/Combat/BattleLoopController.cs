using System;

namespace TowerBreak.Combat
{
    public sealed class BattleLoopController
    {
        private readonly CombatDebugBattleService battleService;
        private readonly float pressureTickInterval;
        private readonly float wallHitThreshold;
        private readonly int wallDamagePerHit;

        private float pressureTickTimer;

        public BattleLoopController(
            CombatDebugBattleService battleService,
            float pressureTickInterval,
            float wallHitThreshold,
            int wallDamagePerHit)
        {
            this.battleService = battleService ?? throw new ArgumentNullException(nameof(battleService));

            if (pressureTickInterval <= 0f)
            {
                throw new ArgumentException("Pressure tick interval must be greater than zero.", nameof(pressureTickInterval));
            }

            if (wallHitThreshold <= 0f)
            {
                throw new ArgumentException("Wall hit threshold must be greater than zero.", nameof(wallHitThreshold));
            }

            if (wallDamagePerHit <= 0)
            {
                throw new ArgumentException("Wall damage per hit must be greater than zero.", nameof(wallDamagePerHit));
            }

            this.pressureTickInterval = pressureTickInterval;
            this.wallHitThreshold = wallHitThreshold;
            this.wallDamagePerHit = wallDamagePerHit;
        }

        public CombatState State
        {
            get { return battleService.State; }
        }

        public BattleLoopUpdateResult Update(float deltaTime)
        {
            if (deltaTime < 0f)
            {
                throw new ArgumentException("Delta time must be zero or greater.", nameof(deltaTime));
            }

            if (State.IsWallDefeated)
            {
                return default;
            }

            if (State.Enemies.Count == 0)
            {
                pressureTickTimer = 0f;
                return default;
            }

            pressureTickTimer += deltaTime;
            if (pressureTickTimer < pressureTickInterval)
            {
                return default;
            }

            CombatPressureResult pressureResult = battleService.AdvanceEnemyPressure(pressureTickTimer, wallHitThreshold, wallDamagePerHit);
            pressureTickTimer = 0f;

            return new BattleLoopUpdateResult(true, pressureResult);
        }

        public CombatAttackResult ApplyAttackToFirstEnemy(int attackDamage)
        {
            if (State.IsWallDefeated)
            {
                return new CombatAttackResult(false, 0, false);
            }

            return battleService.ApplyAttackToFirstEnemy(attackDamage);
        }

        public CombatGuardResult ApplyPlayerGuard(float pressureReduction)
        {
            if (State.IsWallDefeated)
            {
                return new CombatGuardResult(0f);
            }

            return battleService.ApplyPlayerGuard(pressureReduction);
        }
    }
}
