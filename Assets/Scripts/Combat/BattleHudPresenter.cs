namespace TowerBreak.Combat
{
    public static class BattleHudPresenter
    {
        public static string BuildStatusMessage(CombatState state)
        {
            if (state == null)
            {
                return string.Empty;
            }

            if (state.IsWallDefeated)
            {
                return $"DEFEAT - Wall {state.WallHealth}";
            }

            if (state.IsDangerActive)
            {
                return $"DANGER - Wall {state.WallHealth}";
            }

            return string.Empty;
        }
    }
}
