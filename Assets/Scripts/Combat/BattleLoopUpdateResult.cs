namespace TowerBreak.Combat
{
    public readonly struct BattleLoopUpdateResult
    {
        public BattleLoopUpdateResult(bool didAdvancePressure, CombatPressureResult pressureResult)
        {
            DidAdvancePressure = didAdvancePressure;
            PressureResult = pressureResult;
        }

        public bool DidAdvancePressure { get; }

        public CombatPressureResult PressureResult { get; }
    }
}
