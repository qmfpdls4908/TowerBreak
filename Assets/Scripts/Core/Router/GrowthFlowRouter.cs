using System;
using System.Collections.Generic;
using TowerBreak.GameData.TowerBreaker;
using TowerBreak.Meta.Equipment;
using TowerBreak.Meta.Progression;
using TowerBreak.Meta.State;
using TowerBreak.UIFlow.Growth;

namespace TowerBreak.Core.Router
{
    public sealed class GrowthFlowRouter : IGrowthFlowRouter
    {
        private readonly PlayerSessionState _sessionState;
        private readonly IGameDataProvider _gameDataProvider;
        private readonly ISceneLoader _sceneLoader;
        private readonly Func<int> _instanceIdProvider;
        private readonly Random _random;

        public GrowthFlowRouter(
            PlayerSessionState sessionState,
            IGameDataProvider gameDataProvider,
            ISceneLoader sceneLoader,
            Func<int> instanceIdProvider,
            Random random)
        {
            _sessionState = sessionState ?? throw new ArgumentNullException(nameof(sessionState));
            _gameDataProvider = gameDataProvider ?? throw new ArgumentNullException(nameof(gameDataProvider));
            _sceneLoader = sceneLoader ?? throw new ArgumentNullException(nameof(sceneLoader));
            _instanceIdProvider = instanceIdProvider ?? throw new ArgumentNullException(nameof(instanceIdProvider));
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public void Equip()
        {
            var reward = _sessionState.LastBattleReward;
            if (reward == null || reward.GrantedWeaponIds.Count == 0)
            {
                throw new InvalidOperationException("No candidate weapon available to equip.");
            }

            int candidateWeaponId = reward.GrantedWeaponIds[0];
            int instanceId = _instanceIdProvider();
            OwnedEquipment newEquipment = new OwnedEquipment(instanceId, candidateWeaponId);
            _sessionState.Inventory.AddEquipment(newEquipment);
            _sessionState.Inventory.EquipWeapon(instanceId);
        }

        public void Reroll()
        {
            int? equippedInstanceId = _sessionState.Inventory.EquippedWeaponInstanceId;
            if (equippedInstanceId == null)
            {
                throw new InvalidOperationException("No equipped weapon to reroll.");
            }

            if (!_sessionState.Inventory.TryGetEquipment(equippedInstanceId.Value, out OwnedEquipment equipment))
            {
                throw new InvalidOperationException(
                    $"Equipped weapon instance {equippedInstanceId} not found in inventory.");
            }

            TowerBreakerGameData gameData = _gameDataProvider.GetGameData();
            WeaponRow weaponRow = FindWeaponRow(equipment.WeaponId, gameData.Weapons);

            if (weaponRow == null)
            {
                throw new InvalidOperationException(
                    $"Weapon row not found for weapon ID {equipment.WeaponId}.");
            }

            EquipmentRerollService.Reroll(equipment, weaponRow, gameData.RerollCosts, _sessionState.Wallet, _random);
        }

        public void Continue()
        {
            TowerBreakerGameData gameData = _gameDataProvider.GetGameData();
            BattleOutcome outcome = _sessionState.LastBattleReward != null
                ? BattleOutcome.Clear
                : BattleOutcome.Fail;

            FloorProgressionResult result = FloorProgressionService.Advance(
                _sessionState.CurrentFloorId, outcome, gameData.Floors);

            if (result.CanContinue)
            {
                _sessionState.AdvanceToNextFloor();
            }
            else
            {
                _sessionState.EndRun();
            }

            _sceneLoader.LoadScene("Lobby");
        }

        private static WeaponRow FindWeaponRow(int weaponId, IReadOnlyList<WeaponRow> rows)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i].Id == weaponId)
                {
                    return rows[i];
                }
            }

            return null;
        }
    }
}
