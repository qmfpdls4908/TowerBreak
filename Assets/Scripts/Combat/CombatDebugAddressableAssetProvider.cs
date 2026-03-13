using System.Collections.Generic;
using System.Threading.Tasks;

using TowerBreak.GameData.Addressables;

using UnityEngine;

namespace TowerBreak.Combat
{
    public sealed class CombatDebugAddressableAssetProvider : IAddressableAssetProvider
    {
        private readonly Dictionary<string, GameObject> prefabByKey = new();

        public Task<T> LoadAssetAsync<T>(string key)
            where T : Object
        {
            if (typeof(T) != typeof(GameObject))
            {
                return Task.FromResult<T>(null);
            }

            if (!prefabByKey.TryGetValue(key, out GameObject prefab))
            {
                prefab = GameObject.CreatePrimitive(PrimitiveType.Quad);
                prefab.name = $"EnemyPrefab_{key.Replace('/', '_')}";
                prefab.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
                prefab.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                prefab.SetActive(false);
                prefabByKey.Add(key, prefab);
            }

            return Task.FromResult(prefab as T);
        }

        public void Release(Object asset)
        {
        }
    }
}
