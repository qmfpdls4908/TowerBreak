using UnityEngine;

namespace TowerBreak.UIFlow.Lobby
{
    public struct EquipmentDisplayInfo
    {
        public int InstanceId;
        public int WeaponId;
        public string WeaponName;
        public int AttackPower;
        public int EnhancementLevel;
        public int NextEnhancementCost;
        public bool CanEnhance;
        public Sprite WeaponIcon;
    }
}
