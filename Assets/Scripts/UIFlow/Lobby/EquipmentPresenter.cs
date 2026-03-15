using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using TowerBreak.GameData.TowerBreaker;
using TowerBreak.Meta.State;
using TowerBreak.Meta.Equipment;

namespace TowerBreak.UIFlow.Lobby
{
    public sealed class EquipmentPresenter
    {
        private readonly ILobbyFlowRouter _router;
        private readonly PlayerInventoryState _inventory;
        private readonly PlayerWalletState _wallet;
        private readonly TowerBreakerGameData _gameData;

        public EquipmentPresenter(
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
            _router.CloseEquipment();
        }

        public void OnEquipWeapon(int instanceId)
        {
            try
            {
                _inventory.EquipWeapon(instanceId);
                Debug.Log($"[EquipmentPresenter] Equipped weapon instance {instanceId}");
            }
            catch (InvalidOperationException ex)
            {
                Debug.LogWarning($"[EquipmentPresenter] Failed to equip weapon: {ex.Message}");
            }
        }

        public void OnUnequipWeapon()
        {
            _inventory.UnequipWeapon();
            Debug.Log("[EquipmentPresenter] Unequipped weapon");
        }

        public bool IsWeaponEquipped(int instanceId)
        {
            return _inventory.EquippedWeaponInstanceId == instanceId;
        }

        public bool TryEnhanceWeapon(int instanceId)
        {
            if (!_inventory.TryGetEquipment(instanceId, out var ownedEquipment))
            {
                Debug.LogWarning($"[EquipmentPresenter] Cannot enhance: equipment {instanceId} not found");
                return false;
            }

            var weapon = _gameData.Weapons.Find(w => w.Id == ownedEquipment.WeaponId);
            if (weapon == null)
            {
                Debug.LogWarning($"[EquipmentPresenter] Cannot enhance: weapon {ownedEquipment.WeaponId} not found");
                return false;
            }

            bool success = EquipmentEnhancementService.TryEnhance(
                ownedEquipment,
                weapon,
                _gameData.EnhancementCosts,
                _wallet);

            if (success)
            {
                Debug.Log($"[EquipmentPresenter] Enhanced weapon {instanceId} to level {ownedEquipment.EnhancementLevel}");
            }
            else
            {
                Debug.Log($"[EquipmentPresenter] Failed to enhance weapon {instanceId}: insufficient gold or max level");
            }

            return success;
        }

        public EquipmentDisplayInfo? GetEquippedWeapon()
        {
            var equippedId = _inventory.EquippedWeaponInstanceId;
            if (!equippedId.HasValue)
            {
                return null;
            }

            if (!_inventory.TryGetEquipment(equippedId.Value, out var ownedEquipment))
            {
                return null;
            }

            var weapon = _gameData.Weapons.Find(w => w.Id == ownedEquipment.WeaponId);
            if (weapon == null)
            {
                return null;
            }

            return CreateEquipmentDisplayInfo(ownedEquipment, weapon);
        }

        public List<EquipmentDisplayInfo> GetAllEquipment()
        {
            var result = new List<EquipmentDisplayInfo>();

            foreach (var ownedEquipment in _inventory.Equipment)
            {
                var weapon = _gameData.Weapons.Find(w => w.Id == ownedEquipment.WeaponId);
                if (weapon != null)
                {
                    Debug.Log($"[EquipmentPresenter] Equipment: InstanceId={ownedEquipment.InstanceId}, WeaponId={ownedEquipment.WeaponId}, Archetype={weapon.Archetype}, Level={ownedEquipment.EnhancementLevel}");
                    result.Add(CreateEquipmentDisplayInfo(ownedEquipment, weapon));
                }
                else
                {
                    Debug.LogWarning($"[EquipmentPresenter] Weapon not found for WeaponId={ownedEquipment.WeaponId}");
                }
            }

            return result;
        }

        private EquipmentDisplayInfo CreateEquipmentDisplayInfo(OwnedEquipment ownedEquipment, WeaponRow weapon)
        {
            int nextLevel = ownedEquipment.EnhancementLevel + 1;
            var costRow = EquipmentEnhancementService.FindEnhancementCost(nextLevel, _gameData.EnhancementCosts);
            int nextCost = costRow?.GoldCost ?? 0;
            bool canEnhance = costRow != null && _wallet.Gold >= nextCost;

            // 무기 타입에 따라 이미지 경로 설정
            string spritePath = GetWeaponSpritePath(weapon.Archetype);

            return new EquipmentDisplayInfo
            {
                InstanceId = ownedEquipment.InstanceId,
                WeaponId = ownedEquipment.WeaponId,
                WeaponName = weapon.Archetype.ToString(),
                AttackPower = EquipmentEnhancementService.GetCurrentAttackPower(weapon, ownedEquipment.EnhancementLevel),
                EnhancementLevel = ownedEquipment.EnhancementLevel,
                NextEnhancementCost = nextCost,
                CanEnhance = canEnhance,
                WeaponSpritePath = spritePath
            };
        }

        private string GetWeaponSpritePath(WeaponArchetype archetype)
        {
            switch (archetype)
            {
                case WeaponArchetype.Claw:
                    return "Sprites/Player/claw_player";
                case WeaponArchetype.Lance:
                    return "Sprites/Player/lance_player";
                default:
                    return null;
            }
        }
    }
}
