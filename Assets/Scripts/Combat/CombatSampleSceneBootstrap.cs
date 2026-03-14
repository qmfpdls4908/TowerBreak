using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;

using TowerBreak.GameData.Addressables;
using TowerBreak.GameData.TowerBreaker;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace TowerBreak.Combat
{
    public sealed class CombatSampleSceneBootstrap : MonoBehaviour
    {
        private BattleLoopController battleLoopController;
        private IAddressableAssetProvider provider;
        private PooledCombatInstantiator pooled;
        private readonly List<GameObject> activeInstances = new();
        private readonly List<string> activePrefabKeys = new();
        private bool isAttackInFlight;
        private bool wallDefeatLogged;

        private const float PressureTickInterval = 0.5f;
        private const float WallHitThreshold = 10f;
        private const int WallDamagePerHit = 1;
        private const float GuardPressureReduction = 5f;
        private GUIStyle overlayStyle;

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

            try
            {
                CombatDebugSpawnService service = new(provider, pooled);
                await DemonstrateMultiSpawnAsync(service, provider, pooled, gameData);
            }
            catch (Exception exception) when (!(provider is CombatDebugAddressableAssetProvider) && IsRecoverableAddressablesSpawnFailure(exception))
            {
                Debug.LogWarning(
                    "[CombatDebug] Addressables enemy spawn failed. Falling back to debug combat prefabs for SampleScene. " +
                    "Run 'TowerBreak/Setup/Author Combat Enemy Addressables' to restore the real Addressables path.\n" +
                    exception.Message);

                ClearSpawnedChildren();
                provider = new CombatDebugAddressableAssetProvider();
                pooled = new PooledCombatInstantiator();

                CombatDebugSpawnService fallbackService = new(provider, pooled);
                await DemonstrateMultiSpawnAsync(fallbackService, provider, pooled, gameData);
            }
        }

        private void Update()
        {
            if (!ShouldTickCombat(battleLoopController != null, isAttackInFlight))
            {
                return;
            }

            if (battleLoopController.State.IsWallDefeated)
            {
                if (!wallDefeatLogged)
                {
                    wallDefeatLogged = true;
                    Debug.LogWarning(BuildWallDefeatMessage(battleLoopController.State.WallHealth));
                }

                return;
            }

            if (!isAttackInFlight && IsAttackInputDown())
            {
                _ = HandleAttackAsync(attackDamage: 15);
            }
            else if (IsGuardInputDown())
            {
                HandleGuard(GuardPressureReduction);
            }

            AdvanceEnemyPressure();
        }

        private void OnGUI()
        {
            if (battleLoopController == null)
            {
                return;
            }

            string message = BattleHudPresenter.BuildStatusMessage(battleLoopController.State);
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            GUIStyle style = GetOverlayStyle();
            GUI.Label(new Rect(16f, 16f, 420f, 32f), message, style);
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

            // Release all instances back to the pool (quantity expansion mirrors SpawnAllEnemiesAsync)
            FloorRow floor = gameData.Floors[0];
            WaveSpawnPlan plan = WaveSpawnPlanner.CreatePlan(floor, gameData.FloorWaves, gameData.Enemies);
            int releaseIndex = 0;
            for (int i = 0; i < plan.Entries.Count; i++)
            {
                WaveSpawnEntry entry = plan.Entries[i];
                GameObject prefab = await provider.LoadAssetAsync<GameObject>(entry.PrefabKey);
                for (int q = 0; q < entry.Quantity; q++)
                {
                    pooled.Release(prefab, batch1[releaseIndex]);
                    releaseIndex++;
                }
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

            CombatDebugBattleService battleService = new(CombatState.CreateInitial(3, 5, combatEnemies));
            battleLoopController = new BattleLoopController(battleService, PressureTickInterval, WallHitThreshold, WallDamagePerHit);
            wallDefeatLogged = false;
        }

        private void AdvanceEnemyPressure()
        {
            BattleLoopUpdateResult result = battleLoopController.Update(Time.deltaTime);
            if (!result.DidAdvancePressure)
            {
                return;
            }

            if (result.PressureResult.DefeatedWall)
            {
                Debug.LogWarning(BuildWallDefeatMessage(battleLoopController.State.WallHealth));
                wallDefeatLogged = true;
                return;
            }

            if (!result.PressureResult.DidDamageWall && !result.PressureResult.EnteredDanger)
            {
                return;
            }

            Debug.Log(
                $"[CombatDebug] Enemy pressure wall_damage={result.PressureResult.WallDamageApplied} wall_health={battleLoopController.State.WallHealth} " +
                $"danger={battleLoopController.State.IsDangerActive} pending_pressure={battleLoopController.State.PendingEnemyPressure:F2}");
        }

        private void ClearSpawnedChildren()
        {
            List<GameObject> children = new();
            for (int i = 0; i < transform.childCount; i++)
            {
                children.Add(transform.GetChild(i).gameObject);
            }

            for (int i = 0; i < children.Count; i++)
            {
                Destroy(children[i]);
            }
        }

        private async Task HandleAttackAsync(int attackDamage)
        {
            isAttackInFlight = true;
            try
            {
                CombatAttackResult result = battleLoopController.ApplyAttackToFirstEnemy(attackDamage);
                if (!result.HasTarget)
                {
                    Debug.Log("[CombatDebug] Attack ignored: no active enemies remain.");
                    return;
                }

                Debug.Log($"[CombatDebug] Attack hit enemy_id={result.TargetEnemyId} defeated={result.TargetDefeated} remaining={battleLoopController.State.Enemies.Count}");

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

        private void HandleGuard(float pressureReduction)
        {
            CombatGuardResult result = battleLoopController.ApplyPlayerGuard(pressureReduction);
            Debug.Log(
                $"[CombatDebug] Guard applied pressure_reduced={result.PressureReduced:F2} " +
                $"pending_pressure={battleLoopController.State.PendingEnemyPressure:F2}");
        }

        public static bool IsAttackInputDown()
        {
            return Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        }

        public static bool IsGuardInputDown()
        {
            return Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame;
        }

        public static bool ShouldTickCombat(bool hasBattleLoopController, bool isAttackInFlight)
        {
            return hasBattleLoopController;
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

        public static string BuildWallDefeatMessage(int wallHealth)
        {
            return $"[CombatDebug] Wall defeated. Battle lost. wall_health={wallHealth}";
        }

        public static string BuildOverlayStatusMessage(CombatState state)
        {
            return BattleHudPresenter.BuildStatusMessage(state);
        }

        public static void LogMultiSummary(int waveCount, int reuseCount)
        {
            Debug.LogWarning(BuildMultiSummaryMessage(waveCount, reuseCount));
        }

        public static bool IsRecoverableAddressablesSpawnFailure(Exception exception)
        {
            Exception current = exception;
            while (current != null)
            {
                if (current.Message != null)
                {
                    if (current.Message.Contains("Addressable key '") || current.Message.Contains("No Location found for Key="))
                    {
                        return true;
                    }
                }

                current = current.InnerException;
            }

            return false;
        }

        private GUIStyle GetOverlayStyle()
        {
            if (overlayStyle != null)
            {
                return overlayStyle;
            }

            overlayStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                normal =
                {
                    textColor = Color.white
                }
            };

            return overlayStyle;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureBootstrapExists()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.name != "SampleScene")
            {
                return;
            }

            if (UnityEngine.Object.FindFirstObjectByType<CombatSampleSceneBootstrap>() != null)
            {
                return;
            }

            GameObject root = new("CombatSampleSceneBootstrap");
            root.AddComponent<CombatSampleSceneBootstrap>();
        }
    }
}
