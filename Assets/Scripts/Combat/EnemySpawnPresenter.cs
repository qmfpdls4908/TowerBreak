using System;
using System.Threading.Tasks;

using TowerBreak.GameData.Addressables;

using UnityEngine;

namespace TowerBreak.Combat
{
    public sealed class EnemySpawnPresenter
    {
        private readonly IAddressableAssetProvider assetProvider;

        public EnemySpawnPresenter(IAddressableAssetProvider assetProvider)
        {
            this.assetProvider = assetProvider ?? throw new ArgumentNullException(nameof(assetProvider));
        }

        public Task<GameObject> LoadEnemyPrefabAsync(WaveSpawnEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            return assetProvider.LoadAssetAsync<GameObject>(entry.PrefabKey);
        }
    }
}
