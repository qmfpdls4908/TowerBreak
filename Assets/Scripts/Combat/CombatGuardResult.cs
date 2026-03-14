namespace TowerBreak.Combat
{
    public readonly struct CombatGuardResult
    {
        public CombatGuardResult(float pressureReduced)
        {
            PressureReduced = pressureReduced;
        }

        public float PressureReduced { get; }
    }
}
