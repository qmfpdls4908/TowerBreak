using System.Collections.Generic;
using System.Threading.Tasks;

using NUnit.Framework;

using TowerBreak.GameData.TowerBreaker;

using UnityEngine;

namespace TowerBreak.Combat.Tests
{
    public sealed class CombatMultiWaveSpawnTests
    {
        [Test]
        public async Task SpawnAllEnemiesAsync_ForFloor1WithTwoEntries_ReturnsTwoInstances()
        {
            TowerBreakerGameData gameData = CreateTwoEnemyGameData();
            CombatDebugAddressableAssetProvider provider = new();
            PooledCombatInstantiator pooled = new();
            CombatDebugSpawnService service = new(provider, pooled);
            GameObject root = new("Root");

            IReadOnlyList<GameObject> instances = await service.SpawnAllEnemiesAsync(
                gameData, 1, root.transform, i => new Vector3(i * 2f, 0f, 0f));

            Assert.That(instances.Count, Is.EqualTo(3));
            Assert.That(instances[0], Is.Not.Null);
            Assert.That(instances[1], Is.Not.Null);
            Assert.That(instances[2], Is.Not.Null);
            Assert.That(instances[0], Is.Not.SameAs(instances[1]));
            Assert.That(instances[1], Is.Not.SameAs(instances[2]));

            Object.DestroyImmediate(instances[0]);
            Object.DestroyImmediate(instances[1]);
            Object.DestroyImmediate(instances[2]);
            Object.DestroyImmediate(root);
            Object.DestroyImmediate(gameData);
        }

        [Test]
        public async Task SpawnAllEnemiesAsync_AllInstancesAreActiveAndAtSpecifiedPositions()
        {
            TowerBreakerGameData gameData = CreateTwoEnemyGameData();
            CombatDebugAddressableAssetProvider provider = new();
            PooledCombatInstantiator pooled = new();
            CombatDebugSpawnService service = new(provider, pooled);
            GameObject root = new("Root");

            IReadOnlyList<GameObject> instances = await service.SpawnAllEnemiesAsync(
                gameData, 1, root.transform, i => new Vector3(i * 2f, 0f, 0f));

            for (int i = 0; i < instances.Count; i++)
            {
                Assert.That(instances[i].activeSelf, Is.True);
                Assert.That(instances[i].transform.parent, Is.EqualTo(root.transform));
                Assert.That(instances[i].transform.position, Is.EqualTo(new Vector3(i * 2f, 0f, 0f)));
            }

            Assert.That(instances.Count, Is.EqualTo(3));

            foreach (GameObject go in instances)
            {
                Object.DestroyImmediate(go);
            }
            Object.DestroyImmediate(root);
            Object.DestroyImmediate(gameData);
        }

        [Test]
        public async Task SpawnAllEnemiesAsync_ReleaseFirstEntry_ThenRespawnAll_ReusesThatInstance()
        {
            TowerBreakerGameData gameData = CreateTwoEnemyGameData();
            CombatDebugAddressableAssetProvider provider = new();
            PooledCombatInstantiator pooled = new();
            CombatDebugSpawnService service = new(provider, pooled);
            GameObject root = new("Root");

            IReadOnlyList<GameObject> firstBatch = await service.SpawnAllEnemiesAsync(
                gameData, 1, root.transform, i => new Vector3(i * 2f, 0f, 0f));

            GameObject prefab = await provider.LoadAssetAsync<GameObject>("enemy/basic_melee");
            pooled.Release(prefab, firstBatch[0]);

            IReadOnlyList<GameObject> secondBatch = await service.SpawnAllEnemiesAsync(
                gameData, 1, root.transform, i => new Vector3(i * 3f, 0f, 0f));

            Assert.That(secondBatch[0], Is.SameAs(firstBatch[0]));
            Assert.That(secondBatch[1], Is.Not.SameAs(firstBatch[1]));
            Assert.That(secondBatch[2], Is.Not.SameAs(firstBatch[2]));
            Assert.That(secondBatch[0].activeSelf, Is.True);

            Object.DestroyImmediate(firstBatch[1]);
            Object.DestroyImmediate(firstBatch[2]);
            Object.DestroyImmediate(secondBatch[0]);
            Object.DestroyImmediate(secondBatch[1]);
            Object.DestroyImmediate(secondBatch[2]);
            Object.DestroyImmediate(root);
            Object.DestroyImmediate(gameData);
        }

        [Test]
        public async Task SpawnAllEnemiesAsync_ExpandsQuantityIntoRepeatedSpawnEntries()
        {
            TowerBreakerGameData gameData = CreateTwoEnemyGameData();
            CombatDebugAddressableAssetProvider provider = new();
            PooledCombatInstantiator pooled = new();
            CombatDebugSpawnService service = new(provider, pooled);
            GameObject root = new("Root");

            IReadOnlyList<GameObject> instances = await service.SpawnAllEnemiesAsync(
                gameData, 1, root.transform, i => new Vector3(i, 0f, 0f));

            Assert.That(instances.Count, Is.EqualTo(3));
            Assert.That(instances[0].name, Does.StartWith("EnemyPrefab_enemy_basic_melee"));
            Assert.That(instances[1].name, Does.StartWith("EnemyPrefab_enemy_basic_melee"));
            Assert.That(instances[2].name, Does.StartWith("EnemyPrefab_enemy_armored_pusher"));

            foreach (GameObject go in instances)
            {
                Object.DestroyImmediate(go);
            }
            Object.DestroyImmediate(root);
            Object.DestroyImmediate(gameData);
        }

        [Test]
        public async Task SpawnAllEnemiesAsync_SamePrefabTwoEntries_WithoutRelease_AreDistinctInstances()
        {
            TowerBreakerGameData gameData = CreateSamePrefabTwoEntryGameData();
            CombatDebugAddressableAssetProvider provider = new();
            PooledCombatInstantiator pooled = new();
            CombatDebugSpawnService service = new(provider, pooled);
            GameObject root = new("Root");

            IReadOnlyList<GameObject> instances = await service.SpawnAllEnemiesAsync(
                gameData, 1, root.transform, i => new Vector3(i * 2f, 0f, 0f));

            Assert.That(instances.Count, Is.EqualTo(2));
            Assert.That(instances[0], Is.Not.SameAs(instances[1]));
            Assert.That(instances[0].activeSelf, Is.True);
            Assert.That(instances[1].activeSelf, Is.True);

            Object.DestroyImmediate(instances[0]);
            Object.DestroyImmediate(instances[1]);
            Object.DestroyImmediate(root);
            Object.DestroyImmediate(gameData);
        }

        private static TowerBreakerGameData CreateTwoEnemyGameData()
        {
            TowerBreakerGameData gameData = ScriptableObject.CreateInstance<TowerBreakerGameData>();
            gameData.Floors = new List<FloorRow>
            {
                new() { Id = 1, DisplayName = "Floor 1", RewardTableId = 1001, RecommendedPower = 10 }
            };
            gameData.FloorWaves = new List<FloorWaveRow>
            {
                new() { FloorId = 1, WaveIndex = 1, EnemyId = 101, SpawnOrder = 1, SpawnTime = 0f, Quantity = 2 },
                new() { FloorId = 1, WaveIndex = 1, EnemyId = 102, SpawnOrder = 2, SpawnTime = 5f, Quantity = 1 }
            };
            gameData.Enemies = new List<EnemyRow>
            {
                new() { Id = 101, Archetype = EnemyArchetype.BasicMelee, PrefabKey = "enemy/basic_melee", Health = 10 },
                new() { Id = 102, Archetype = EnemyArchetype.ArmoredPusher, PrefabKey = "enemy/armored_pusher", Health = 20 }
            };
            return gameData;
        }

        private static TowerBreakerGameData CreateSamePrefabTwoEntryGameData()
        {
            TowerBreakerGameData gameData = ScriptableObject.CreateInstance<TowerBreakerGameData>();
            gameData.Floors = new List<FloorRow>
            {
                new() { Id = 1, DisplayName = "Floor 1", RewardTableId = 1001, RecommendedPower = 10 }
            };
            gameData.FloorWaves = new List<FloorWaveRow>
            {
                new() { FloorId = 1, WaveIndex = 1, EnemyId = 101, SpawnOrder = 1, SpawnTime = 0f, Quantity = 1 },
                new() { FloorId = 1, WaveIndex = 1, EnemyId = 103, SpawnOrder = 2, SpawnTime = 0f, Quantity = 1 }
            };
            gameData.Enemies = new List<EnemyRow>
            {
                new() { Id = 101, Archetype = EnemyArchetype.BasicMelee, PrefabKey = "enemy/basic_melee", Health = 10 },
                new() { Id = 103, Archetype = EnemyArchetype.BasicMelee, PrefabKey = "enemy/basic_melee", Health = 10 }
            };
            return gameData;
        }
    }
}
