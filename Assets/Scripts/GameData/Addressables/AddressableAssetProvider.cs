using System;
using System.Threading.Tasks;

using UnityEngine;

namespace TowerBreak.GameData.Addressables
{
    public sealed class AddressableAssetProvider : IAddressableAssetProvider
    {
        private readonly IAddressableAssetLoader loader;
        private readonly AddressableKeyValidator validator;

        public AddressableAssetProvider(IAddressableAssetLoader loader, AddressableKeyValidator validator)
        {
            this.loader = loader ?? throw new ArgumentNullException(nameof(loader));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<T> LoadAssetAsync<T>(string key)
            where T : UnityEngine.Object
        {
            string validatedKey = validator.ValidateRequired(key, nameof(key));
            T asset = await loader.LoadAssetAsync<T>(validatedKey);
            if (asset == null)
            {
                throw new InvalidOperationException($"Addressable key '{validatedKey}' did not resolve to asset type '{typeof(T).FullName}'.");
            }

            return asset;
        }

        public void Release(UnityEngine.Object asset)
        {
            loader.Release(asset);
        }
    }
}
