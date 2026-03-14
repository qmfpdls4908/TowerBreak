using System;
using System.Collections.Generic;
using TowerBreak.GameData.TowerBreaker;
using TowerBreak.Meta.Equipment;
using TowerBreak.Meta.State;
using TowerBreak.UIFlow.Growth;

namespace TowerBreak.Core.State
{
    public sealed class GrowthStateReader : IGrowthStateReader
    {
        private readonly PlayerSessionState _sessionState;
        private readonly IGameDataProvider _gameDataProvider;

        public GrowthStateReader(PlayerSessionState sessionState, IGameDataProvider gameDataProvider)
        {
            _sessionState = sessionState ?? throw new ArgumentNullException(nameof(sessionState));
            _gameDataProvider = gameDataProvider ?? throw new ArgumentNullException(nameof(gameDataProvider));
        }

        public int? EquippedWeaponId
        {
            get
            {
                int? instanceId = _sessionState.Inventory.EquippedWeaponInstanceId;
                if (instanceId == null)
                {
                    return null;
                }

                if (!_sessionState.Inventory.TryGetEquipment(instanceId.Value, out OwnedEquipment eq))
                {
                    return null;
                }

                return eq.WeaponId;
            }
        }

        public int? CandidateWeaponId
        {
            get
            {
                var reward = _sessionState.LastBattleReward;
                if (reward == null || reward.GrantedWeaponIds.Count == 0)
                {
                    return null;
                }

                return reward.GrantedWeaponIds[0];
            }
        }

        public int DeltaAttack => GetComparison()?.DeltaAttack ?? 0;

        public float DeltaAttackSpeed => GetComparison()?.DeltaAttackSpeed ?? 0f;

        public float DeltaPushPower => GetComparison()?.DeltaPushPower ?? 0f;

        private EquipmentComparisonResult GetComparison()
        {
            if (!EquippedWeaponId.HasValue || !CandidateWeaponId.HasValue)
            {
                return null;
            }

            IReadOnlyList<WeaponRow> weaponRows = _gameDataProvider.GetGameData().Weapons;

            EquipmentStatBlock equippedBlock = EquipmentComparisonService.GetEquippedStatBlock(
                _sessionState.Inventory, weaponRows);

            WeaponRow candidateRow = FindWeaponRow(CandidateWeaponId.Value, weaponRows);
            if (candidateRow == null)
            {
                return null;
            }

            EquipmentStatBlock candidateBlock = EquipmentStatBlock.FromWeaponRow(candidateRow);
            return EquipmentComparisonService.Compare(equippedBlock, candidateBlock);
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
