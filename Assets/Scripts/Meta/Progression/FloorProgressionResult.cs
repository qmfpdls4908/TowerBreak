namespace TowerBreak.Meta.Progression
{
    public sealed class FloorProgressionResult
    {
        public FloorProgressionResult(
            int currentFloorId,
            int? nextFloorId,
            bool isRunComplete,
            bool canContinue)
        {
            CurrentFloorId = currentFloorId;
            NextFloorId = nextFloorId;
            IsRunComplete = isRunComplete;
            CanContinue = canContinue;
        }

        public int CurrentFloorId { get; }
        public int? NextFloorId { get; }
        public bool IsRunComplete { get; }
        public bool CanContinue { get; }
    }
}
