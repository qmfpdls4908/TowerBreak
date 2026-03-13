using System.Threading.Tasks;

using UnityEngine;

namespace TowerBreak.GameData.Addressables
{
    public interface IAddressableAssetLoader
    {
        Task<T> LoadAssetAsync<T>(string key)
            where T : UnityEngine.Object;

        void Release(UnityEngine.Object asset);
    }
}
