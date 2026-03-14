namespace TowerBreak.Core.Events
{
    public readonly struct CombatEndedEvent
    {
        public bool IsVictory { get; }
        public int FloorId { get; }

        public CombatEndedEvent(bool isVictory, int floorId)
        {
            IsVictory = isVictory;
            FloorId = floorId;
        }
    }
}
