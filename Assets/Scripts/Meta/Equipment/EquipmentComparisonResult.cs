namespace TowerBreak.Meta.Equipment
{
    public sealed class EquipmentComparisonResult
    {
        public EquipmentComparisonResult(EquipmentStatBlock current, EquipmentStatBlock candidate)
        {
            Current = current;
            Candidate = candidate;
            DeltaAttack = candidate.Attack - current.Attack;
            DeltaAttackSpeed = candidate.AttackSpeed - current.AttackSpeed;
            DeltaPushPower = candidate.PushPower - current.PushPower;
        }

        public EquipmentStatBlock Current { get; }
        public EquipmentStatBlock Candidate { get; }
        public int DeltaAttack { get; }
        public float DeltaAttackSpeed { get; }
        public float DeltaPushPower { get; }
    }
}
