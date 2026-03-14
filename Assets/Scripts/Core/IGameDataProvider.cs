using TowerBreak.GameData.TowerBreaker;

namespace TowerBreak.Core
{
    public interface IGameDataProvider
    {
        TowerBreakerGameData GetGameData();
    }
}
