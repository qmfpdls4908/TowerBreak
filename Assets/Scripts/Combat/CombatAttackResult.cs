namespace TowerBreak.Combat
{
    public readonly struct CombatAttackResult
    {
        public CombatAttackResult(bool hasTarget, int targetEnemyId, bool targetDefeated)
        {
            HasTarget = hasTarget;
            TargetEnemyId = targetEnemyId;
            TargetDefeated = targetDefeated;
        }

        public bool HasTarget { get; }

        public int TargetEnemyId { get; }

        public bool TargetDefeated { get; }
    }
}
