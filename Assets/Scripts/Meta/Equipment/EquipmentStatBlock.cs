using System;

using TowerBreak.GameData.TowerBreaker;

namespace TowerBreak.Meta.Equipment
{
    public readonly struct EquipmentStatBlock
    {
        public EquipmentStatBlock(int attack, float attackSpeed, float pushPower)
        {
            Attack = attack;
            AttackSpeed = attackSpeed;
            PushPower = pushPower;
        }

        public int Attack { get; }
        public float AttackSpeed { get; }
        public float PushPower { get; }

        public static EquipmentStatBlock FromWeaponRow(WeaponRow row)
        {
            if (row == null)
            {
                throw new ArgumentNullException(nameof(row));
            }

            return new EquipmentStatBlock(row.BaseAttack, row.AttackSpeed, row.PushPower);
        }
    }
}
