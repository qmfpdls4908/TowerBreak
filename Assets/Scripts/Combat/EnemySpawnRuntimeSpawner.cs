using System;
using System.Threading.Tasks;

using UnityEngine;

namespace TowerBreak.Combat
{
    public sealed class EnemySpawnRuntimeSpawner
    {
        private readonly EnemySpawnPresenter presenter;
        private readonly ICombatInstantiator instantiator;

        public EnemySpawnRuntimeSpawner(EnemySpawnPresenter presenter, ICombatInstantiator instantiator)
        {
            this.presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
            this.instantiator = instantiator ?? throw new ArgumentNullException(nameof(instantiator));
        }

        public async Task<GameObject> SpawnAsync(WaveSpawnEntry entry, Transform parent, Vector3 position)
        {
            GameObject prefab = await presenter.LoadEnemyPrefabAsync(entry);
            return instantiator.Instantiate(prefab, position, parent);
        }
    }
}
