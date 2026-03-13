using TowerBreak.DI;
using TowerBreak.GameData.Addressables;

namespace TowerBreak.Combat
{
    public static class CombatProviderResolver
    {
        public static IAddressableAssetProvider Resolve()
        {
            if (DIContainer.TryResolveFromRegistered<IAddressableAssetProvider>(out IAddressableAssetProvider provider))
            {
                return provider;
            }

            return new CombatDebugAddressableAssetProvider();
        }
    }
}
