using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

using TowerBreak.GameData.Addressables;
using TowerBreak.GameData.TowerBreaker;

using UnityEngine;

namespace TowerBreak.Combat.Tests
{
    public sealed class WaveSpawnPlannerTests
    {
        [Test]
        public void CreatePlan_ForFloor1_ReturnsSpawnEntriesInWaveAndOrderSequence()
        {
            FloorRow floor = new()
            {
                Id = 1,
                RewardTableId = 1001,
                RecommendedPower = 10,
                DisplayName = "Floor 1"
            };

            List<FloorWaveRow> floorWaves = new()
            {
                new FloorWaveRow { FloorId = 1, WaveIndex = 1, EnemyId = 101, SpawnOrder = 2, SpawnTime = 4f, Quantity = 1 },
                new FloorWaveRow { FloorId = 1, WaveIndex = 1, EnemyId = 100, SpawnOrder = 1, SpawnTime = 0f, Quantity = 2 },
                new FloorWaveRow { FloorId = 2, WaveIndex = 1, EnemyId = 999, SpawnOrder = 1, SpawnTime = 0f, Quantity = 1 }
            };

            List<EnemyRow> enemies = new()
            {
                new EnemyRow { Id = 100, Archetype = EnemyArchetype.BasicMelee, PrefabKey = "enemy/basic_a", Health = 10 },
                new EnemyRow { Id = 101, Archetype = EnemyArchetype.ArmoredPusher, PrefabKey = "enemy/armored_a", Health = 30 }
            };

            WaveSpawnPlan plan = WaveSpawnPlanner.CreatePlan(floor, floorWaves, enemies);

            Assert.That(plan.FloorId, Is.EqualTo(1));
            Assert.That(plan.Entries.Count, Is.EqualTo(2));
            Assert.That(plan.Entries.Select(entry => entry.EnemyId), Is.EqualTo(new[] { 100, 101 }));
            Assert.That(plan.Entries[0].PrefabKey, Is.EqualTo("enemy/basic_a"));
            Assert.That(plan.Entries[0].Quantity, Is.EqualTo(2));
            Assert.That(plan.Entries[1].WaveIndex, Is.EqualTo(1));
        }

        [Test]
        public void CreatePlan_WhenEnemyPrefabKeyMissing_ThrowsInvalidOperationException()
        {
            FloorRow floor = new()
            {
                Id = 1,
                RewardTableId = 1001,
                RecommendedPower = 10,
                DisplayName = "Floor 1"
            };

            List<FloorWaveRow> floorWaves = new()
            {
                new FloorWaveRow { FloorId = 1, WaveIndex = 1, EnemyId = 100, SpawnOrder = 1, SpawnTime = 0f, Quantity = 1 }
            };

            List<EnemyRow> enemies = new()
            {
                new EnemyRow { Id = 100, Archetype = EnemyArchetype.BasicMelee, PrefabKey = "", Health = 10 }
            };

            Assert.Throws<System.InvalidOperationException>(() => WaveSpawnPlanner.CreatePlan(floor, floorWaves, enemies));
        }

        [Test]
        public async Task EnemySpawnPresenter_LoadEnemyPrefabAsync_UsesPrefabKeyFromSpawnEntry()
        {
            GameObject expectedPrefab = new("EnemyPrefab");
            RecordingAssetProvider provider = new(expectedPrefab);
            EnemySpawnPresenter presenter = new(provider);
            WaveSpawnEntry entry = new()
            {
                EnemyId = 101,
                PrefabKey = "enemy/basic_melee"
            };

            GameObject prefab = await presenter.LoadEnemyPrefabAsync(entry);

            Assert.That(prefab, Is.SameAs(expectedPrefab));
            Assert.That(provider.LastRequestedKey, Is.EqualTo("enemy/basic_melee"));

            Object.DestroyImmediate(expectedPrefab);
        }

        [Test]
        public async Task EnemySpawnRuntimeSpawner_SpawnAsync_CreatesSceneInstanceFromLoadedPrefab()
        {
            GameObject prefab = new("EnemyPrefab");
            prefab.AddComponent<BoxCollider>();

            RecordingAssetProvider provider = new(prefab);
            EnemySpawnPresenter presenter = new(provider);
            EnemySpawnRuntimeSpawner spawner = new(presenter, new UnityCombatInstantiator());
            GameObject parent = new("SpawnRoot");
            WaveSpawnEntry entry = new()
            {
                EnemyId = 101,
                PrefabKey = "enemy/basic_melee"
            };

            GameObject instance = await spawner.SpawnAsync(entry, parent.transform, new Vector3(2f, 0f, 0f));

            Assert.That(instance, Is.Not.Null);
            Assert.That(instance, Is.Not.SameAs(prefab));
            Assert.That(instance.transform.parent, Is.EqualTo(parent.transform));
            Assert.That(instance.transform.position, Is.EqualTo(new Vector3(2f, 0f, 0f)));
            Assert.That(instance.name, Does.StartWith("EnemyPrefab"));

            Object.DestroyImmediate(instance);
            Object.DestroyImmediate(parent);
            Object.DestroyImmediate(prefab);
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
