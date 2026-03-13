using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace TowerBreak.GameData.Addressables
{
    public sealed class UnityAddressableAssetLoader : IAddressableAssetLoader
    {
        public Task<T> LoadAssetAsync<T>(string key)
            where T : UnityEngine.Object
        {
            AsyncOperationHandle<T> handle = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<T>(key);
            return handle.Task;
        }

        public void Release(UnityEngine.Object asset)
        {
            if (asset == null)
            {
                return;
            }

            UnityEngine.AddressableAssets.Addressables.Release(asset);
        }
    }
}
