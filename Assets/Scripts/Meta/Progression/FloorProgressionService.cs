using System;
using System.Collections.Generic;

using TowerBreak.GameData.TowerBreaker;

namespace TowerBreak.Meta.Progression
{
    public static class FloorProgressionService
    {
        public static FloorProgressionResult Advance(
            int currentFloorId,
            BattleOutcome outcome,
            IReadOnlyList<FloorRow> floors)
        {
            if (floors == null)
            {
                throw new ArgumentNullException(nameof(floors));
            }

            if (floors.Count == 0)
            {
                throw new InvalidOperationException("Floor list cannot be empty.");
            }

            int currentIndex = FindIndex(currentFloorId, floors);

            if (currentIndex < 0)
            {
                throw new InvalidOperationException(
                    $"Floor ID {currentFloorId} not found in floor list.");
            }

            if (outcome == BattleOutcome.Fail)
            {
                return new FloorProgressionResult(currentFloorId, null, false, false);
            }

            bool isLast = currentIndex == floors.Count - 1;

            if (isLast)
            {
                return new FloorProgressionResult(currentFloorId, null, true, false);
            }

            int nextFloorId = floors[currentIndex + 1].Id;
            return new FloorProgressionResult(currentFloorId, nextFloorId, false, true);
        }

        private static int FindIndex(int floorId, IReadOnlyList<FloorRow> floors)
        {
            for (int i = 0; i < floors.Count; i++)
            {
                if (floors[i].Id == floorId)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
