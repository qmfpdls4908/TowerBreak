namespace TowerBreak.Combat
{
    public readonly struct CombatPressureResult
    {
        public CombatPressureResult(bool didDamageWall, int wallDamageApplied, bool enteredDanger, bool defeatedWall)
        {
            DidDamageWall = didDamageWall;
            WallDamageApplied = wallDamageApplied;
            EnteredDanger = enteredDanger;
            DefeatedWall = defeatedWall;
        }

        public bool DidDamageWall { get; }

        public int WallDamageApplied { get; }

        public bool EnteredDanger { get; }

        public bool DefeatedWall { get; }
    }
}
