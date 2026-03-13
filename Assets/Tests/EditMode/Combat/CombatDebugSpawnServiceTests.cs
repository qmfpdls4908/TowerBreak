using System.Collections.Generic;
using System.Threading.Tasks;

using NUnit.Framework;

using TowerBreak.GameData.Addressables;
using TowerBreak.GameData.TowerBreaker;

using UnityEngine;

namespace TowerBreak.Combat.Tests
{
    public sealed class CombatDebugSpawnServiceTests
    {
        [Test]
        public async Task DebugAssetProvider_CreatesHiddenTemplateAndSpawnedInstanceRemainsActive()
        {
            CombatDebugAddressableAssetProvider provider = new();
            GameObject prefab = await provider.LoadAssetAsync<GameObject>("enemy/basic_melee");
            UnityCombatInstantiator instantiator = new();
            GameObject root = new("Root");

            GameObject instance = instantiator.Instantiate(prefab, Vector3.zero, root.transform);

            Assert.That(prefab.activeSelf, Is.False);
            Assert.That((prefab.hideFlags & HideFlags.HideInHierarchy) != 0, Is.True);
            Assert.That(instance.activeSelf, Is.True);

            Object.DestroyImmediate(instance);
            Object.DestroyImmediate(root);
            Object.DestroyImmediate(prefab);
        }

        [Test]
        public async Task PooledCombatInstantiator_ReusesReleasedInstanceForSamePrefab()
        {
            CombatDebugAddressableAssetProvider provider = new();
            GameObject prefab = await provider.LoadAssetAsync<GameObject>("enemy/basic_melee");
            PooledCombatInstantiator instantiator = new();
            GameObject root = new("Root");

            GameObject first = instantiator.Instantiate(prefab, new Vector3(1f, 0f, 0f), root.transform);
            instantiator.Release(prefab, first);
            GameObject second = instantiator.Instantiate(prefab, new Vector3(2f, 0f, 0f), root.transform);

            Assert.That(second, Is.SameAs(first));
            Assert.That(second.activeSelf, Is.True);
            Assert.That(second.transform.position, Is.EqualTo(new Vector3(2f, 0f, 0f)));
            Assert.That(second.transform.parent, Is.EqualTo(root.transform));

            Object.DestroyImmediate(second);
            Object.DestroyImmediate(root);
            Object.DestroyImmediate(prefab);
        }

        [Test]
        public void ResourcesLoad_FindsTowerBreakerGameDataAsset()
        {
            TowerBreakerGameData gameData = Resources.Load<TowerBreakerGameData>("TowerBreakerGameData");

            Assert.That(gameData, Is.Not.Null);
            Assert.That(gameData.Floors, Is.Not.Null);
            Assert.That(gameData.Floors.Count, Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public async Task SpawnFirstEnemyAsync_UsesGameDataForSelectedFloorAndCreatesInstance()
        {
            TowerBreakerGameData gameData = ScriptableObject.CreateInstance<TowerBreakerGameData>();
            gameData.Floors = new List<FloorRow>
            {
                new() { Id = 1, DisplayName = "Floor 1", RewardTableId = 1001, RecommendedPower = 10 }
            };
            gameData.FloorWaves = new List<FloorWaveRow>
            {
                new() { FloorId = 1, WaveIndex = 1, EnemyId = 101, SpawnOrder = 1, SpawnTime = 0f, Quantity = 1 }
            };
            gameData.Enemies = new List<EnemyRow>
            {
                new() { Id = 101, Archetype = EnemyArchetype.BasicMelee, PrefabKey = "enemy/basic_melee", Health = 10 }
            };

            GameObject prefab = new("EnemyPrefab");
            RecordingAssetProvider provider = new(prefab);
            CombatDebugSpawnService service = new(provider, new UnityCombatInstantiator());
            GameObject root = new("Root");

            GameObject instance = await service.SpawnFirstEnemyAsync(gameData, 1, root.transform, new Vector3(1f, 2f, 0f));

            Assert.That(instance, Is.Not.Null);
            Assert.That(provider.LastRequestedKey, Is.EqualTo("enemy/basic_melee"));
            Assert.That(instance.transform.parent, Is.EqualTo(root.transform));
            Assert.That(instance.transform.position, Is.EqualTo(new Vector3(1f, 2f, 0f)));

            Object.DestroyImmediate(instance);
            Object.DestroyImmediate(root);
            Object.DestroyImmediate(prefab);
            Object.DestroyImmediate(gameData);
        }

        private sealed class RecordingAssetProvider : IAddressableAssetProvider
        {
            private readonly GameObject prefab;

            public RecordingAssetProvider(GameObject prefab)
            {
                this.prefab = prefab;
            }

            public string LastRequestedKey { get; private set; }

            public Task<T> LoadAssetAsync<T>(string key)
                where T : Object
            {
                LastRequestedKey = key;
                return Task.FromResult(prefab as T);
            }

            public void Release(Object asset)
            {
            }
        }
    }
}
