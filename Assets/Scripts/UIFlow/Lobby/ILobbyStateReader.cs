namespace TowerBreak.UIFlow.Lobby
{
    public interface ILobbyStateReader
    {
        int CurrentFloorId { get; }
        int Gold { get; }
        int? EquippedWeaponId { get; }
    }
}
