namespace TowerBreak.UIFlow.Lobby
{
    public interface ILobbyFlowRouter
    {
        void OpenChallenge();
        void OpenEquipment();
        void CloseEquipment();
        void OpenReroll();
        void CloseReroll();
    }
}
