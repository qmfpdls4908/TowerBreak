namespace TowerBreak.Combat
{
    public interface IPlayerActionHandler
    {
        bool CanPerformAction(PlayerActionType actionType);
        void PerformAction(PlayerActionType actionType);
        PlayerActionType CurrentAction { get; }
        bool IsActionInProgress { get; }
    }
}
