using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TowerBreak.GameData.Addressables;
using TowerBreak.GameData.TowerBreaker;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace TowerBreak.Combat
{
    public sealed class CombatSampleSceneBootstrap : MonoBehaviour
    {
        private CombatDebugBattleService battleService;
        private IAddressableAssetProvider provider;
        private PooledCombatInstantiator pooled;
        private readonly List<GameObject> activeInstances = new();
        private readonly List<string> activePrefabKeys = new();
        private bool isAttackInFlight;

        private async void Start()
        {
            TowerBreakerGameData gameData = Resources.Load<TowerBreakerGameData>("TowerBreakerGameData");
            if (gameData == null)
            {
                Debug.LogWarning("TowerBreakerGameData resource was not found.");
                return;
            }

            provider = CombatProviderResolver.Resolve();
            pooled = new PooledCombatInstantiator();
            CombatDebugSpawnService service = new(provider, pooled);

            await DemonstrateMultiSpawnAsync(service, provider, pooled, gameData);
        }

        private void Update()
        {
            if (battleService == null || isAttackInFlight)
            {
                return;
            }

            if (!Input.GetKeyDown(KeyCode.Space))
            {
                return;
            }

            _ = HandleAttackAsync(attackDamage: 15);
        }

        private async Task DemonstrateMultiSpawnAsync(
            CombatDebugSpawnService service,
            IAddressableAssetProvider provider,
            PooledCombatInstantiator pooled,
            TowerBreakerGameData gameData)
        {
            // First batch: spawn all wave entries for floor 1
            IReadOnlyList<GameObject> batch1 = await service.SpawnAllEnemiesAsync(
                gameData, 1, transform, i => new Vector3(i * 2f, 0f, 0f));

            for (int i = 0; i < batch1.Count; i++)
            {
                Debug.Log($"[CombatDebug] Wave spawn [{i}]: {batch1[i].name} (id={batch1[i].GetInstanceID()})");
            }

            // Release all instances back to the pool
            FloorRow floor = gameData.Floors[0];
            WaveSpawnPlan plan = WaveSpawnPlanner.CreatePlan(floor, gameData.FloorWaves, gameData.Enemies);
            for (int i = 0; i < plan.Entries.Count; i++)
            {
                GameObject prefab = await provider.LoadAssetAsync<GameObject>(plan.Entries[i].PrefabKey);
                pooled.Release(prefab, batch1[i]);
            }
            Debug.Log($"[CombatDebug] Released {batch1.Count} instances back to pool.");

            // Second batch: should reuse pooled instances
            IReadOnlyList<GameObject> batch2 = await service.SpawnAllEnemiesAsync(
                gameData, 1, transform, i => new Vector3(i * 2f + 0.5f, 0f, 0f));

            int reuseCount = 0;
            for (int i = 0; i < batch2.Count; i++)
            {
                bool reused = ReferenceEquals(batch1[i], batch2[i]);
                if (reused) reuseCount++;
                Debug.Log($"[CombatDebug] Second spawn [{i}]: {batch2[i].name} (id={batch2[i].GetInstanceID()}) pool_reuse={reused}");
            }

            LogMultiSummary(batch2.Count, reuseCount);

            InitializeBattleState(plan, batch2, gameData.Enemies);
            Debug.Log("[CombatDebug] Press Space to attack the first active enemy.");
        }

        private void InitializeBattleState(WaveSpawnPlan plan, IReadOnlyList<GameObject> instances, IReadOnlyList<EnemyRow> enemies)
        {
            activeInstances.Clear();
            activePrefabKeys.Clear();

            Dictionary<int, EnemyRow> enemyById = enemies.ToDictionary(enemy => enemy.Id);
            List<CombatEnemyState> combatEnemies = new();

            int instanceIndex = 0;
            for (int i = 0; i < plan.Entries.Count; i++)
            {
                WaveSpawnEntry entry = plan.Entries[i];
                EnemyRow enemy = enemyById[entry.EnemyId];
                for (int quantityIndex = 0; quantityIndex < entry.Quantity; quantityIndex++)
                {
                    combatEnemies.Add(new CombatEnemyState(enemy.Id, enemy.Health, enemy.Pressure));
                    activeInstances.Add(instances[instanceIndex]);
                    activePrefabKeys.Add(entry.PrefabKey);
                    instanceIndex++;
                }
            }

            battleService = new CombatDebugBattleService(CombatState.CreateInitial(3, 5, combatEnemies));
        }

        private async Task HandleAttackAsync(int attackDamage)
        {
            isAttackInFlight = true;
            try
            {
                CombatAttackResult result = battleService.ApplyAttackToFirstEnemy(attackDamage);
                if (!result.HasTarget)
                {
                    Debug.Log("[CombatDebug] Attack ignored: no active enemies remain.");
                    return;
                }

                Debug.Log($"[CombatDebug] Attack hit enemy_id={result.TargetEnemyId} defeated={result.TargetDefeated} remaining={battleService.State.Enemies.Count}");

                if (!result.TargetDefeated)
                {
                    return;
                }

                GameObject defeatedInstance = activeInstances[0];
                string prefabKey = activePrefabKeys[0];
                activeInstances.RemoveAt(0);
                activePrefabKeys.RemoveAt(0);

                GameObject prefab = await provider.LoadAssetAsync<GameObject>(prefabKey);
                pooled.Release(prefab, defeatedInstance);
                Debug.Log($"[CombatDebug] Enemy defeated and released to pool: enemy_id={result.TargetEnemyId}");
            }
            finally
            {
                isAttackInFlight = false;
            }
        }

        public static string BuildSummaryMessage(int firstId, int secondId, bool reused)
        {
            return $"[CombatDebugSummary] first_id={firstId} second_id={secondId} pool_reuse={reused}";
        }

        public static void LogSummary(int firstId, int secondId, bool reused)
        {
            Debug.LogWarning(BuildSummaryMessage(firstId, secondId, reused));
        }

        public static string BuildMultiSummaryMessage(int waveCount, int reuseCount)
        {
            return $"[CombatDebugSummary] wave_count={waveCount} pool_reuse_count={reuseCount}";
        }

        public static void LogMultiSummary(int waveCount, int reuseCount)
        {
            Debug.LogWarning(BuildMultiSummaryMessage(waveCount, reuseCount));
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureBootstrapExists()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.name != "SampleScene")
            {
                return;
            }

            if (Object.FindFirstObjectByType<CombatSampleSceneBootstrap>() != null)
            {
                return;
            }

            GameObject root = new("CombatSampleSceneBootstrap");
            root.AddComponent<CombatSampleSceneBootstrap>();
        }
    }
}
