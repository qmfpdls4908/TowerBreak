namespace TowerBreak.EventBus
{
    public readonly struct PlayerDefeatedEvent
    {
        public int FinalHealth { get; }
        
        public PlayerDefeatedEvent(int finalHealth)
        {
            FinalHealth = finalHealth;
        }
    }
}
