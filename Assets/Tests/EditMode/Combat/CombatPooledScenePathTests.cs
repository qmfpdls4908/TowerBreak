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

        [Test]
        public void BuildWallDefeatMessage_ContainsBattleLossSummary()
        {
            string message = CombatSampleSceneBootstrap.BuildWallDefeatMessage(wallHealth: 0);

            Assert.That(message, Does.Contain("Wall defeated"));
            Assert.That(message, Does.Contain("wall_health=0"));
        }

        [Test]
        public void BuildOverlayStatusMessage_WhenDangerActive_ReturnsDangerSummary()
        {
            CombatState state = CombatState.CreateInitial(
                3,
                4,
                new List<CombatEnemyState>
                {
                    new CombatEnemyState(101, 10, 1f)
                });
            state = state.AdvanceEnemyPressure(deltaTime: 1f, wallHitThreshold: 1f, wallDamagePerHit: 1);

            string message = CombatSampleSceneBootstrap.BuildOverlayStatusMessage(state);

            Assert.That(message, Does.Contain("DANGER"));
            Assert.That(message, Does.Contain("Wall 3"));
        }

        [Test]
        public void BuildOverlayStatusMessage_WhenWallDefeated_ReturnsDefeatSummary()
        {
            CombatState state = CombatState.CreateInitial(3, 1, new List<CombatEnemyState>());
            state = state.ApplyWallDamage(1);

            string message = CombatSampleSceneBootstrap.BuildOverlayStatusMessage(state);

            Assert.That(message, Does.Contain("DEFEAT"));
            Assert.That(message, Does.Contain("Wall 0"));
        }

        [Test]
        public void BuildOverlayStatusMessage_WhenStateIsSafe_ReturnsEmpty()
        {
            CombatState state = CombatState.CreateInitial(3, 5, new List<CombatEnemyState>());

            string message = CombatSampleSceneBootstrap.BuildOverlayStatusMessage(state);

            Assert.That(message, Is.Empty);
        }

        [Test]
        public void IsRecoverableAddressablesSpawnFailure_WhenAddressableKeyMessage_ReturnsTrue()
        {
            System.InvalidOperationException exception = new("Addressable key 'enemy/armored_pusher' did not resolve to asset type 'UnityEngine.GameObject'.");

            bool result = CombatSampleSceneBootstrap.IsRecoverableAddressablesSpawnFailure(exception);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsRecoverableAddressablesSpawnFailure_WhenInvalidKeyInnerException_ReturnsTrue()
        {
            System.Exception exception = new("Outer", new System.Exception("No Location found for Key=enemy/armored_pusher"));

            bool result = CombatSampleSceneBootstrap.IsRecoverableAddressablesSpawnFailure(exception);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsRecoverableAddressablesSpawnFailure_WhenUnrelatedException_ReturnsFalse()
        {
            System.InvalidOperationException exception = new("Some other failure");

            bool result = CombatSampleSceneBootstrap.IsRecoverableAddressablesSpawnFailure(exception);

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsAttackInputDown_WhenNoKeyboardDevice_ReturnsFalse()
        {
            bool result = CombatSampleSceneBootstrap.IsAttackInputDown();

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsGuardInputDown_WhenNoKeyboardDevice_ReturnsFalse()
        {
            bool result = CombatSampleSceneBootstrap.IsGuardInputDown();

            Assert.That(result, Is.False);
        }

        [Test]
        public void ShouldTickCombat_WhenAttackIsInFlight_ReturnsTrue()
        {
            bool result = CombatSampleSceneBootstrap.ShouldTickCombat(hasBattleLoopController: true, isAttackInFlight: true);

            Assert.That(result, Is.True);
        }

        [Test]
        public void ShouldTickCombat_WhenControllerMissing_ReturnsFalse()
        {
            bool result = CombatSampleSceneBootstrap.ShouldTickCombat(hasBattleLoopController: false, isAttackInFlight: false);

            Assert.That(result, Is.False);
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
