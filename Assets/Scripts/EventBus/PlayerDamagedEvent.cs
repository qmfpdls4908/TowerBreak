namespace TowerBreak.EventBus
{
    public readonly struct PlayerDamagedEvent
    {
        public float Damage { get; }
        
        public PlayerDamagedEvent(float damage)
        {
            Damage = damage;
        }
    }
}
