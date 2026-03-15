using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using TowerBreak.GameData.TowerBreaker;
using TowerBreak.Meta.State;
using TowerBreak.Meta.Equipment;

namespace TowerBreak.UIFlow.Lobby
{
    public sealed class EnhancementPresenter
    {
        private readonly ILobbyFlowRouter _router;
        private readonly PlayerInventoryState _inventory;
        private readonly PlayerWalletState _wallet;
        private readonly TowerBreakerGameData _gameData;

        public EnhancementPresenter(
            ILobbyFlowRouter router,
            PlayerInventoryState inventory,
            PlayerWalletState wallet,
            TowerBreakerGameData gameData)
        {
            _router = router ?? throw new ArgumentNullException(nameof(router));
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            _gameData = gameData ?? throw new ArgumentNullException(nameof(gameData));
        }

        public void OnClose()
        {
            _router.CloseReroll();
        }

        public int CurrentGold => _wallet.Gold;

        public List<EnhancementDisplayInfo> GetAllEquipmentForEnhancement()
        {
            var result = new List<EnhancementDisplayInfo>();

            Debug.Log($"[EnhancementPresenter] Inventory count: {_inventory.Equipment.Count}");

            foreach (var ownedEquipment in _inventory.Equipment)
            {
                Debug.Log($"[EnhancementPresenter] Processing equipment: InstanceId={ownedEquipment.InstanceId}, WeaponId={ownedEquipment.WeaponId}, Level={ownedEquipment.EnhancementLevel}");
                
                var weapon = _gameData.Weapons.Find(w => w.Id == ownedEquipment.WeaponId);
                if (weapon != null)
                {
                    Debug.Log($"[EnhancementPresenter] Found weapon: {weapon.Archetype}, BaseAttack={weapon.BaseAttack}");
                    var displayInfo = CreateEnhancementDisplayInfo(ownedEquipment, weapon);
                    result.Add(displayInfo);
                }
                else
                {
                    Debug.LogWarning($"[EnhancementPresenter] Weapon not found for WeaponId={ownedEquipment.WeaponId}");
                }
            }

            Debug.Log($"[EnhancementPresenter] Total displayable equipment: {result.Count}");
            return result;
        }

        public EnhancementResult TryEnhance(int instanceId)
        {
            if (!_inventory.TryGetEquipment(instanceId, out var ownedEquipment))
            {
                return EnhancementResult.CreateFailed("Equipment not found");
            }

            var weapon = _gameData.Weapons.Find(w => w.Id == ownedEquipment.WeaponId);
            if (weapon == null)
            {
                return EnhancementResult.CreateFailed("Weapon data not found");
            }

            int currentLevel = ownedEquipment.EnhancementLevel;
            var nextCostRow = EquipmentEnhancementService.FindEnhancementCost(currentLevel + 1, _gameData.EnhancementCosts);

            if (nextCostRow == null)
            {
                return EnhancementResult.CreateFailed("Maximum enhancement level reached");
            }

            if (_wallet.Gold < nextCostRow.GoldCost)
            {
                return EnhancementResult.CreateFailed($"Insufficient gold. Required: {nextCostRow.GoldCost}, Available: {_wallet.Gold}");
            }

            bool success = EquipmentEnhancementService.TryEnhance(
                ownedEquipment,
                weapon,
                _gameData.EnhancementCosts,
                _wallet);

            if (success)
            {
                int newAttackPower = EquipmentEnhancementService.GetCurrentAttackPower(weapon, ownedEquipment.EnhancementLevel);
                return EnhancementResult.CreateSuccess(
                    ownedEquipment.EnhancementLevel,
                    newAttackPower,
                    nextCostRow.GoldCost);
            }

            return EnhancementResult.CreateFailed("Enhancement failed");
        }

        private EnhancementDisplayInfo CreateEnhancementDisplayInfo(OwnedEquipment ownedEquipment, WeaponRow weapon)
        {
            int currentLevel = ownedEquipment.EnhancementLevel;
            int currentAttack = EquipmentEnhancementService.GetCurrentAttackPower(weapon, currentLevel);
            
            var nextCostRow = EquipmentEnhancementService.FindEnhancementCost(currentLevel + 1, _gameData.EnhancementCosts);
            int nextCost = nextCostRow?.GoldCost ?? 0;
            int nextAttack = nextCostRow != null 
                ? EquipmentEnhancementService.GetCurrentAttackPower(weapon, currentLevel + 1)
                : currentAttack;
            bool canEnhance = nextCostRow != null && _wallet.Gold >= nextCost;

            return new EnhancementDisplayInfo
            {
                InstanceId = ownedEquipment.InstanceId,
                WeaponId = ownedEquipment.WeaponId,
                WeaponName = weapon.Archetype.ToString(),
                CurrentLevel = currentLevel,
                CurrentAttack = currentAttack,
                NextLevel = currentLevel + 1,
                NextAttack = nextAttack,
                Cost = nextCost,
                CanEnhance = canEnhance,
                IsMaxLevel = nextCostRow == null
            };
        }
    }

    public struct EnhancementDisplayInfo
    {
        public int InstanceId;
        public int WeaponId;
        public string WeaponName;
        public int CurrentLevel;
        public int CurrentAttack;
        public int NextLevel;
        public int NextAttack;
        public int Cost;
        public bool CanEnhance;
        public bool IsMaxLevel;
    }

    public struct EnhancementResult
    {
        public bool Success { get; }
        public int NewLevel { get; }
        public int NewAttackPower { get; }
        public int Cost { get; }
        public string ErrorMessage { get; }

        private EnhancementResult(bool success, int newLevel, int newAttackPower, int cost, string errorMessage)
        {
            Success = success;
            NewLevel = newLevel;
            NewAttackPower = newAttackPower;
            Cost = cost;
            ErrorMessage = errorMessage;
        }

        public static EnhancementResult CreateSuccess(int newLevel, int newAttackPower, int cost)
        {
            return new EnhancementResult(true, newLevel, newAttackPower, cost, null);
        }

        public static EnhancementResult CreateFailed(string errorMessage)
        {
            return new EnhancementResult(false, 0, 0, 0, errorMessage);
        }
    }
}
