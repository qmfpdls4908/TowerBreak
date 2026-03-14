using System;
using TowerBreak.Core.Events;
using TowerBreak.EventBus;
using TowerBreak.GameData.TowerBreaker;
using TowerBreak.Meta.Rewards;

namespace TowerBreak.Core.Bridge
{
    public sealed class CombatResultBridge
    {
        private readonly PlayerSessionState _sessionState;
        private readonly IGameDataProvider _gameDataProvider;
        private readonly EventBus<CombatEndedEvent> _combatEndedBus;

        public CombatResultBridge(
            PlayerSessionState sessionState,
            IGameDataProvider gameDataProvider,
            EventBus<CombatEndedEvent> combatEndedBus)
        {
            _sessionState = sessionState ?? throw new ArgumentNullException(nameof(sessionState));
            _gameDataProvider = gameDataProvider ?? throw new ArgumentNullException(nameof(gameDataProvider));
            _combatEndedBus = combatEndedBus ?? throw new ArgumentNullException(nameof(combatEndedBus));
        }

        public void Initialize()
        {
            _combatEndedBus.Subscribe(OnCombatEnded);
        }

        public void Dispose()
        {
            _combatEndedBus.Unsubscribe(OnCombatEnded);
        }

        private void OnCombatEnded(CombatEndedEvent evt)
        {
            if (!evt.IsVictory)
            {
                _sessionState.ClearLastBattleReward();
                return;
            }

            TowerBreakerGameData gameData = _gameDataProvider.GetGameData();
            FloorRow floor = FindFloor(evt.FloorId, gameData.Floors);

            if (floor == null)
            {
                _sessionState.ClearLastBattleReward();
                return;
            }

            RewardBundle reward = RewardResolver.Resolve(
                floor,
                gameData.RewardTables,
                gameData.RewardEntries,
                new Random(Guid.NewGuid().GetHashCode()));

            _sessionState.SetLastBattleReward(reward);
        }

        private static FloorRow FindFloor(int floorId, System.Collections.Generic.IReadOnlyList<FloorRow> floors)
        {
            for (int i = 0; i < floors.Count; i++)
            {
                if (floors[i].Id == floorId)
                {
                    return floors[i];
                }
            }

            return null;
        }
    }
}
