namespace TowerBreak.UIFlow.Battle
{
    public interface IBattleHudStateReader
    {
        int PlayerHealth { get; }
        int FloorId { get; }
        int WaveNumber { get; }
        bool IsDangerActive { get; }
        bool IsWallDefeated { get; }
    }
}
