using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

using TowerBreak.GameData.TowerBreaker;
using TowerBreak.GameData.Addressables;
using TowerBreak.DI;

namespace TowerBreak.Combat
{
    public sealed class BattleSceneInitializer : MonoBehaviour
    {
        [SerializeField] private int currentFloor = 1;
        [SerializeField] private Transform enemySpawnParent;
        [SerializeField] private string playerPrefabPath = "Prefabs/Combat/Player";
        
        private BattleLoopController battleLoopController;
        private List<GameObject> spawnedEnemies = new();
        private PlayerController playerController;
        private IAddressableAssetProvider provider;
        private PooledCombatInstantiator pooled;
        private bool isAttackInFlight = false;
        private bool wallDefeated = false;
        
        private const float PressureTickInterval = 0.5f;
        private const float WallHitThreshold = 10f;
        private const int WallDamagePerHit = 1;
        private const float GuardPressureReduction = 5f;
        
        private async void Start()
        {
            Debug.Log($"[Battle] Initializing battle scene for floor {currentFloor}...");
            
            // GameData 로드
            var gameData = Resources.Load<TowerBreakerGameData>("TowerBreakerGameData");
            if (gameData == null)
            {
                Debug.LogError("[Battle] TowerBreakerGameData not found!");
                return;
            }
            
            // 현재 층 정보 확인
            if (currentFloor > gameData.Floors.Count)
            {
                Debug.LogError($"[Battle] Floor {currentFloor} not found in GameData!");
                return;
            }
            
            var floor = gameData.Floors[currentFloor - 1];
            var floorWaves = gameData.FloorWaves.FindAll(w => w.FloorId == floor.Id);
            int waveCount = floorWaves.Count > 0 ? floorWaves[floorWaves.Count - 1].WaveIndex + 1 : 0;
            const int DefaultWallHealth = 100;
            Debug.Log($"[Battle] Floor {currentFloor}: {waveCount} waves, Wall HP: {DefaultWallHealth}/{DefaultWallHealth}");
            
            // 카메라 정보 로그
            var mainCamera = Camera.main;
            if (mainCamera != null)
            {
                Debug.Log($"[Battle] Camera: OrthographicSize={mainCamera.orthographicSize}, Pos={mainCamera.transform.position}");
            }
            else
            {
                Debug.LogWarning("[Battle] Main Camera not found!");
            }
            provider = CombatProviderResolver.Resolve();
            pooled = new PooledCombatInstantiator();
            
            if (enemySpawnParent == null)
            {
                enemySpawnParent = transform;
            }
            
            try
            {
                // 적 스폰 서비스 생성
                var spawnService = new CombatDebugSpawnService(provider, pooled);
                
                // 적 스폰 - 화면 중앙에 배치
                var enemies = await spawnService.SpawnAllEnemiesAsync(
                    gameData, 
                    currentFloor, 
                    enemySpawnParent,
                    index => new Vector3(index * 2f - 3f, 0f, 0f)  // 중앙 정렬
                );
                
                spawnedEnemies = new List<GameObject>(enemies);
                Debug.Log($"[Battle] Spawned {spawnedEnemies.Count} enemies");
                
                // 스폰된 적에 시각적 요소 추가
                for (int i = 0; i < spawnedEnemies.Count; i++)
                {
                    var enemy = spawnedEnemies[i];
                    if (enemy != null)
                    {
                        // SpriteRenderer 확인/추가
                        var sr = enemy.GetComponent<SpriteRenderer>();
                        if (sr == null)
                        {
                            sr = enemy.AddComponent<SpriteRenderer>();
                            sr.sprite = Sprite.Create(
                                Texture2D.whiteTexture,
                                new Rect(0, 0, 1, 1),
                                new Vector2(0.5f, 0.5f),
                                100f
                            );
                            sr.color = Color.red;
                            Debug.Log($"[Battle] Added SpriteRenderer to enemy {i}");
                        }
                        
                        // EnemyController 확인/추가
                        var controller = enemy.GetComponent<EnemyController>();
                        if (controller == null)
                        {
                            controller = enemy.AddComponent<EnemyController>();
                            Debug.Log($"[Battle] Added EnemyController to enemy {i}");
                        }
                        
                        // 스케일 조정 (너무 작으면 보이지 않음)
                        enemy.transform.localScale = new Vector3(1f, 1f, 1f);
                    }
                }
                
                // BattleLoopController 초기화
                InitializeBattle(gameData, floor);
                
                // 플레이어 생성
                CreatePlayer();
                
                Debug.Log("[Battle] Battle ready!");
                Debug.Log("[Battle] Controls: SPACE = Attack | G = Guard | Left Shift = Dash");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Battle] Failed to spawn enemies: {ex.Message}");
            }
        }
        
        private void CreatePlayer()
        {
            Debug.Log("[Battle] Creating player from prefab...");
            
            // 프리팹 로드
            GameObject playerPrefab = Resources.Load<GameObject>(playerPrefabPath);
            if (playerPrefab == null)
            {
                Debug.LogError($"[Battle] Player prefab not found at: {playerPrefabPath}");
                // 폴백: 코드로 생성
                CreatePlayerFallback();
                return;
            }
            
            // 프리팹 인스턴스화
            GameObject playerObject = Instantiate(playerPrefab, transform);
            playerObject.name = "Player";
            playerObject.transform.position = new Vector3(-6f, 0f, 0f);
            
            // PlayerController 가져오기
            playerController = playerObject.GetComponent<PlayerController>();
            if (playerController == null)
            {
                Debug.LogWarning("[Battle] PlayerController not found on prefab, adding...");
                playerController = playerObject.AddComponent<PlayerController>();
            }
            
            Debug.Log($"[Battle] Player created from prefab at position: {playerObject.transform.position}");
        }
        
        private void CreatePlayerFallback()
        {
            Debug.Log("[Battle] Creating player from code (fallback)...");
            
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.SetParent(transform);
            playerObject.transform.position = new Vector3(-6f, 0f, 0f);
            
            // SpriteRenderer 추가
            var spriteRenderer = playerObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreatePlayerSprite();
            spriteRenderer.color = Color.white;
            
            // PlayerController 추가
            playerController = playerObject.AddComponent<PlayerController>();
            
            Debug.Log($"[Battle] Player created from code at position: {playerObject.transform.position}");
        }
        
        private Sprite CreatePlayerSprite()
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            
            return Sprite.Create(
                texture,
                new Rect(0, 0, 1, 1),
                new Vector2(0.5f, 0.5f),
                100f
            );
        }
        
        private void InitializeBattle(TowerBreakerGameData gameData, FloorRow floor)
        {
            // WaveSpawnPlan 생성
            var plan = WaveSpawnPlanner.CreatePlan(floor, gameData.FloorWaves, gameData.Enemies);
            
            // CombatEnemyState 목록 생성
            var combatEnemies = new List<CombatEnemyState>();
            
            foreach (var entry in plan.Entries)
            {
                var enemyRow = gameData.Enemies.Find(e => e.Id == entry.EnemyId);
                if (enemyRow != null)
                {
                    for (int i = 0; i < entry.Quantity; i++)
                    {
                        combatEnemies.Add(new CombatEnemyState(enemyRow.Id, enemyRow.Health, enemyRow.Pressure));
                    }
                }
            }
            
            // 스폰된 적에 EnemyRow 데이터 연결
            for (int i = 0; i < spawnedEnemies.Count && i < combatEnemies.Count; i++)
            {
                var controller = spawnedEnemies[i].GetComponent<EnemyController>();
                var enemyRow = gameData.Enemies.Find(e => e.Id == combatEnemies[i].EnemyId);
                if (controller != null && enemyRow != null)
                {
                    controller.Initialize(enemyRow);
                }
            }
            const int DefaultWallHealth = 100;
            var combatState = CombatState.CreateInitial(DefaultWallHealth, DefaultWallHealth, combatEnemies);
            
            // BattleDebugService 생성
            var battleService = new CombatDebugBattleService(combatState);
            
            // BattleLoopController 생성
            battleLoopController = new BattleLoopController(
                battleService,
                PressureTickInterval,
                WallHitThreshold,
                WallDamagePerHit
            );
            
            Debug.Log($"[Battle] Initialized with {combatEnemies.Count} enemies, Wall HP: {DefaultWallHealth}");
        }
        
        private void Update()
        {
            if (battleLoopController == null || wallDefeated) return;
            
            // 적 압박 업데이트
            var result = battleLoopController.Update(Time.deltaTime);
            
            if (result.DidAdvancePressure)
            {
                if (result.PressureResult.DefeatedWall)
                {
                    Debug.Log("[Battle] ☠️ WALL DEFEATED! Game Over.");
                    wallDefeated = true;
                    return;
                }
            }
            
            // 플레이어 입력 처리
            HandlePlayerInput();
            
            // 승리 조건 체크
            if (battleLoopController.State.Enemies.Count == 0 && spawnedEnemies.Count > 0)
            {
                Debug.Log("[Battle] 🎉 VICTORY! All enemies defeated!");
                wallDefeated = true;
            }
        }
        
        private void HandlePlayerInput()
        {
            if (playerController == null) return;
            
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard == null) return;
            
            // Space: 공격
            if (keyboard.spaceKey.wasPressedThisFrame)
            {
                playerController.PerformAttack();
                if (!isAttackInFlight)
                {
                    _ = HandleAttackAsync(15);
                }
            }
            // G: 가드
            else if (keyboard.gKey.wasPressedThisFrame)
            {
                playerController.PerformGuard();
                HandleGuard();
            }
            // Left Shift: 대시
            else if (keyboard.leftShiftKey.wasPressedThisFrame)
            {
                playerController.PerformDash();
            }
        }
        
        private async Task HandleAttackAsync(int damage)
        {
            isAttackInFlight = true;
            try
            {
                var result = battleLoopController.ApplyAttackToFirstEnemy(damage);
                if (result.HasTarget)
                {
                    Debug.Log($"[Battle] ⚔️ Attack! Enemy {result.TargetEnemyId} took {damage} damage. Remaining HP: {GetEnemyHealth(result.TargetEnemyId)}");
                    
                    if (result.TargetDefeated)
                    {
                        await HandleEnemyDefeated(result.TargetEnemyId);
                    }
                }
                else
                {
                    Debug.Log("[Battle] ⚔️ Attack! No enemies remaining!");
                }
            }
            finally
            {
                isAttackInFlight = false;
            }
        }
        
        private int GetEnemyHealth(int enemyId)
        {
            var enemy = battleLoopController.State.Enemies.FirstOrDefault(e => e.EnemyId == enemyId);
            return enemy?.Health ?? 0;
        }
        
        private async Task HandleEnemyDefeated(int enemyId)
        {
            Debug.Log($"[Battle] 💀 Enemy {enemyId} defeated!");
            
            // 처치된 적 GameObject 찾기 및 제거
            var enemyToRemove = spawnedEnemies.FirstOrDefault();
            if (enemyToRemove != null)
            {
                spawnedEnemies.Remove(enemyToRemove);
                
                // 파괴 효과
                var controller = enemyToRemove.GetComponent<EnemyController>();
                if (controller != null)
                {
                    controller.TakeDamage(int.MaxValue);
                }
                else
                {
                    Destroy(enemyToRemove);
                }
            }
            
            Debug.Log($"[Battle] Remaining enemies: {spawnedEnemies.Count}");
        }
        
        private void HandleGuard()
        {
            var result = battleLoopController.ApplyPlayerGuard(GuardPressureReduction);
            Debug.Log($"[Battle] 🛡️ Guard! Pressure reduced by {result.PressureReduced:F2}. Wall HP: {battleLoopController.State.WallHealth}");
        }
        
        private static bool IsAttackInputDown()
        {
            return UnityEngine.InputSystem.Keyboard.current?.spaceKey.wasPressedThisFrame ?? false;
        }
        
        private static bool IsGuardInputDown()
        {
            return UnityEngine.InputSystem.Keyboard.current?.gKey.wasPressedThisFrame ?? false;
        }
    }
}
