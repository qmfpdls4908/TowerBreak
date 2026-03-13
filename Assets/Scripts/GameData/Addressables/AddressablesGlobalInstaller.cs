using TowerBreak.DI;

using UnityEngine;

namespace TowerBreak.GameData.Addressables
{
    public sealed class AddressablesGlobalInstaller : DIGlobalInstaller
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeOnLoad()
        {
            Install();
        }

        public static void Install()
        {
            DIGlobalContext.Install(new AddressablesGlobalInstaller());
        }

        protected override void InstallBindings(DIContainer container)
        {
            AddressableKeyValidator validator = new();
            IAddressableAssetLoader loader = new UnityAddressableAssetLoader();
            IAddressableAssetProvider provider = new AddressableAssetProvider(loader, validator);

            container.Register(validator);
            container.Register(loader);
            container.Register(provider);
        }
    }
}
