namespace TowerBreak.EventBus
{
    public readonly struct PlayerActionEvent
    {
        public int ActionTypeId { get; }
        
        public PlayerActionEvent(int actionTypeId)
        {
            ActionTypeId = actionTypeId;
        }
    }
}
