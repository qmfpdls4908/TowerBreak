using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TowerBreak.GameData.Addressables;
using TowerBreak.GameData.TowerBreaker;

using UnityEngine;

namespace TowerBreak.Combat
{
    public sealed class CombatDebugSpawnService
    {
        private readonly EnemySpawnRuntimeSpawner runtimeSpawner;

        public CombatDebugSpawnService(IAddressableAssetProvider assetProvider, ICombatInstantiator instantiator)
        {
            if (assetProvider == null)
            {
                throw new ArgumentNullException(nameof(assetProvider));
            }

            if (instantiator == null)
            {
                throw new ArgumentNullException(nameof(instantiator));
            }

            runtimeSpawner = new EnemySpawnRuntimeSpawner(new EnemySpawnPresenter(assetProvider), instantiator);
        }

        public async Task<GameObject> SpawnFirstEnemyAsync(TowerBreakerGameData gameData, int floorId, Transform parent, Vector3 position)
        {
            if (gameData == null)
            {
                throw new ArgumentNullException(nameof(gameData));
            }

            FloorRow floor = gameData.Floors.First(row => row.Id == floorId);
            WaveSpawnPlan plan = WaveSpawnPlanner.CreatePlan(floor, gameData.FloorWaves, gameData.Enemies);
            WaveSpawnEntry entry = plan.Entries.First();
            return await runtimeSpawner.SpawnAsync(entry, parent, position);
        }

        public async Task<IReadOnlyList<GameObject>> SpawnAllEnemiesAsync(
            TowerBreakerGameData gameData,
            int floorId,
            Transform parent,
            Func<int, Vector3> positionSelector)
        {
            if (gameData == null)
            {
                throw new ArgumentNullException(nameof(gameData));
            }

            if (positionSelector == null)
            {
                throw new ArgumentNullException(nameof(positionSelector));
            }

            FloorRow floor = gameData.Floors.First(row => row.Id == floorId);
            WaveSpawnPlan plan = WaveSpawnPlanner.CreatePlan(floor, gameData.FloorWaves, gameData.Enemies);

            List<GameObject> instances = new();
            int spawnIndex = 0;
            for (int i = 0; i < plan.Entries.Count; i++)
            {
                WaveSpawnEntry entry = plan.Entries[i];
                for (int quantityIndex = 0; quantityIndex < entry.Quantity; quantityIndex++)
                {
                    GameObject instance = await runtimeSpawner.SpawnAsync(entry, parent, positionSelector(spawnIndex));
                    instances.Add(instance);
                    spawnIndex++;
                }
            }

            return instances;
        }
    }
}
