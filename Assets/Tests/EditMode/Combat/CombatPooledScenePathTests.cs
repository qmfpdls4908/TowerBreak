using System.Collections.Generic;
using System.Threading.Tasks;

using NUnit.Framework;

using UnityEngine.TestTools;

using TowerBreak.GameData.TowerBreaker;

using UnityEngine;

namespace TowerBreak.Combat.Tests
{
    public sealed class CombatPooledScenePathTests
    {
        [Test]
        public void BuildSummaryMessage_ContainsReuseResultAndInstanceIds()
        {
            string message = CombatSampleSceneBootstrap.BuildSummaryMessage(10, 10, true);

            Assert.That(message, Does.Contain("pool_reuse=True"));
            Assert.That(message, Does.Contain("first_id=10"));
            Assert.That(message, Does.Contain("second_id=10"));
        }

        [Test]
        public async Task PooledScenePath_FirstSpawn_CreatesNewActiveInstance()
        {
            TowerBreakerGameData gameData = CreateTestGameData();
            CombatDebugAddressableAssetProvider provider = new();
            PooledCombatInstantiator pooled = new();
            CombatDebugSpawnService service = new(provider, pooled);
            GameObject root = new("Root");

            GameObject instance = await service.SpawnFirstEnemyAsync(gameData, 1, root.transform, new Vector3(2f, 0f, 0f));

            Assert.That(instance, Is.Not.Null);
            Assert.That(instance.activeSelf, Is.True);
            Assert.That(instance.transform.parent, Is.EqualTo(root.transform));
            Assert.That(instance.transform.position, Is.EqualTo(new Vector3(2f, 0f, 0f)));

            Object.DestroyImmediate(instance);
            Object.DestroyImmediate(root);
            Object.DestroyImmediate(gameData);
        }

        [Test]
        public async Task PooledScenePath_SpawnReleaseThenRespawn_ReusesSameInstance()
        {
            TowerBreakerGameData gameData = CreateTestGameData();
            CombatDebugAddressableAssetProvider provider = new();
            PooledCombatInstantiator pooled = new();
            CombatDebugSpawnService service = new(provider, pooled);
            GameObject root = new("Root");

            GameObject first = await service.SpawnFirstEnemyAsync(gameData, 1, root.transform, Vector3.zero);

            GameObject prefab = await provider.LoadAssetAsync<GameObject>("enemy/basic_melee");
            pooled.Release(prefab, first);

            GameObject second = await service.SpawnFirstEnemyAsync(gameData, 1, root.transform, new Vector3(1f, 0f, 0f));

            Assert.That(second, Is.SameAs(first));
            Assert.That(second.activeSelf, Is.True);
            Assert.That(second.transform.position, Is.EqualTo(new Vector3(1f, 0f, 0f)));
            Assert.That(second.transform.parent, Is.EqualTo(root.transform));

            Object.DestroyImmediate(second);
            Object.DestroyImmediate(root);
            Object.DestroyImmediate(prefab);
            Object.DestroyImmediate(gameData);
        }

        [Test]
        public async Task PooledScenePath_TwoSpawnsWithoutRelease_AreDistinctInstances()
        {
            TowerBreakerGameData gameData = CreateTestGameData();
            CombatDebugAddressableAssetProvider provider = new();
            PooledCombatInstantiator pooled = new();
            CombatDebugSpawnService service = new(provider, pooled);
            GameObject root = new("Root");

            GameObject first = await service.SpawnFirstEnemyAsync(gameData, 1, root.transform, Vector3.zero);
            GameObject second = await service.SpawnFirstEnemyAsync(gameData, 1, root.transform, new Vector3(1f, 0f, 0f));

            Assert.That(second, Is.Not.SameAs(first));
            Assert.That(first.activeSelf, Is.True);
            Assert.That(second.activeSelf, Is.True);

            Object.DestroyImmediate(first);
            Object.DestroyImmediate(second);
            Object.DestroyImmediate(root);
            Object.DestroyImmediate(gameData);
        }

        [Test]
        public void LogSummary_EmitsWarningForConsoleVisibility()
        {
            LogAssert.Expect(LogType.Warning, "[CombatDebugSummary] first_id=1 second_id=2 pool_reuse=False");

            CombatSampleSceneBootstrap.LogSummary(1, 2, false);
        }

        private static TowerBreakerGameData CreateTestGameData()
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
            return gameData;
        }
    }
}
