namespace TowerBreak.UIFlow.Growth
{
    public interface IGrowthStateReader
    {
        int? EquippedWeaponId { get; }
        int? CandidateWeaponId { get; }
        int DeltaAttack { get; }
        float DeltaAttackSpeed { get; }
        float DeltaPushPower { get; }
    }
}
