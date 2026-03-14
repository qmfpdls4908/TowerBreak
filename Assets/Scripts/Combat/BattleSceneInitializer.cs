using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

using TowerBreak.GameData.TowerBreaker;
using TowerBreak.GameData.Addressables;
using TowerBreak.DI;
using TowerBreak.Meta.State;
using TowerBreak.Meta.Rewards;
using TowerBreak.Meta.Progression;

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
        private bool victoryHandled = false;
        private TowerBreakerGameData gameData;
        private PlayerInventoryState inventoryState;
        private WeaponRow equippedWeapon;
        private RewardBundle currentReward;
        private int nextFloorId;
        private bool showRewardPopup = false;
        private string rewardMessage = "";
        private bool isGameComplete = false;
        
        private const float PressureTickInterval = 0.5f;
        private const float WallHitThreshold = 10f;
        private const int WallDamagePerHit = 1;
        private const float GuardPressureReduction = 5f;
        
        private void OnGUI()
        {
            if (showRewardPopup)
            {
                DrawRewardPopup();
            }
        }
        
        private void DrawRewardPopup()
        {
            // 배경 (반투명)
            GUI.color = new Color(0, 0, 0, 0.9f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
            
            // 팝업 창
            float popupWidth = 400;
            float popupHeight = 300;
            Rect popupRect = new Rect(
                (Screen.width - popupWidth) / 2,
                (Screen.height - popupHeight) / 2,
                popupWidth,
                popupHeight
            );
            
            GUI.Box(popupRect, "");
            
            // 제목
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 32;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.normal.textColor = Color.yellow;
            GUI.Label(new Rect(popupRect.x, popupRect.y + 20, popupWidth, 50), "VICTORY!", titleStyle);
            
            // 보상 내용
            GUIStyle contentStyle = new GUIStyle(GUI.skin.label);
            contentStyle.fontSize = 20;
            contentStyle.alignment = TextAnchor.MiddleCenter;
            contentStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(popupRect.x, popupRect.y + 80, popupWidth, 150), rewardMessage, contentStyle);
            
            // 버튼 스타일
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 24;
            buttonStyle.alignment = TextAnchor.MiddleCenter;
            
            float buttonWidth = 150;
            float buttonHeight = 50;
            float buttonY = popupRect.y + popupHeight - 80;
            
            if (!isGameComplete)
            {
                // 계속하기 버튼
                if (GUI.Button(new Rect(popupRect.x + 30, buttonY, buttonWidth, buttonHeight), "계속하기", buttonStyle))
                {
                    ContinueToNextFloor();
                }
                
                // 나가기 버튼
                if (GUI.Button(new Rect(popupRect.x + popupWidth - buttonWidth - 30, buttonY, buttonWidth, buttonHeight), "나가기", buttonStyle))
                {
                    ExitToLobby();
                }
            }
            else
            {
                // 게임 완료 - 나가기 버튼만
                if (GUI.Button(new Rect(popupRect.x + (popupWidth - buttonWidth) / 2, buttonY, buttonWidth, buttonHeight), "나가기", buttonStyle))
                {
                    ExitToLobby();
                }
            }
        }
        
        private void ContinueToNextFloor()
        {
            showRewardPopup = false;
            currentFloor = nextFloorId;
            UnityEngine.SceneManagement.SceneManager.LoadScene("Battle");
        }
        
        private void ExitToLobby()
        {
            showRewardPopup = false;
            UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
        }
        
        private async void Start()
        {
            Debug.Log($"[Battle] Initializing battle scene for floor {currentFloor}...");
            
            // GameData 로드
            gameData = Resources.Load<TowerBreakerGameData>("TowerBreakerGameData");
            if (gameData == null)
            {
                Debug.LogError("[Battle] TowerBreakerGameData not found!");
                return;
            }
            
            // PlayerInventoryState 로드
            inventoryState = DIContainer.ResolveFromRegistered<PlayerInventoryState>();
            if (inventoryState == null)
            {
                Debug.LogWarning("[Battle] PlayerInventoryState not found in DI container. Using default weapon.");
            }
            
            // 장착된 무기 정보 로드
            equippedWeapon = GetEquippedWeapon();
            if (equippedWeapon != null)
            {
                Debug.Log($"[Battle] Equipped weapon: {equippedWeapon.Archetype} (BaseAttack: {equippedWeapon.BaseAttack})");
            }
            else
            {
                Debug.Log("[Battle] No weapon equipped, using default attack power");
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
                
                // 플레이어 생성 (무기 정보 전달)
                CreatePlayer(equippedWeapon);
                
                Debug.Log("[Battle] Battle ready!");
                Debug.Log($"[Battle] Controls: SPACE = Attack ({GetCurrentAttackPower()} damage) | G = Guard | Left Shift = Dash");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Battle] Failed to spawn enemies: {ex.Message}");
            }
        }
        
        private void CreatePlayer(WeaponRow weapon)
        {
            Debug.Log("[Battle] Creating player from prefab...");
            
            // 프리팹 로드
            GameObject playerPrefab = Resources.Load<GameObject>(playerPrefabPath);
            if (playerPrefab == null)
            {
                Debug.LogError($"[Battle] Player prefab not found at: {playerPrefabPath}");
                // 폴백: 코드로 생성
                CreatePlayerFallback(weapon);
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
            
            // 무기 정보 설정
            if (weapon != null)
            {
                playerController.SetWeapon(weapon);
            }
            
            Debug.Log($"[Battle] Player created from prefab at position: {playerObject.transform.position}");
        }
        
        private void CreatePlayerFallback(WeaponRow weapon)
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
            
            // 무기 정보 설정
            if (weapon != null)
            {
                playerController.SetWeapon(weapon);
            }
            
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
                HandleVictory();
                return;
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
                    int damage = GetCurrentAttackPower();
                    _ = HandleAttackAsync(damage);
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
        
        private void HandleVictory()
        {
            if (victoryHandled) return;
            victoryHandled = true;
            wallDefeated = true;
            
            // 현재 층 정보 가져오기
            var floor = gameData.Floors[currentFloor - 1];
            
            // 보상 계산
            RewardBundle rewardBundle;
            try
            {
                var random = new System.Random();
                rewardBundle = RewardResolver.Resolve(floor, gameData.RewardTables, gameData.RewardEntries, random);
            }
            catch (System.InvalidOperationException ex)
            {
                Debug.LogWarning($"[Battle] Reward resolution failed: {ex.Message}");
                Debug.Log("[Battle] Using fallback rewards: 20 gold");
                rewardBundle = new RewardBundle(20, new System.Collections.Generic.List<int>());
            }
            
            currentReward = rewardBundle;
            
            // 보상 적용
            var wallet = DIContainer.ResolveFromRegistered<PlayerWalletState>();
            int nextInstanceId = GetNextInstanceId();
            BattleRewardService.Apply(rewardBundle, wallet, inventoryState, () => nextInstanceId++);
            
            // 다음 층 정보 저장
            var progressionResult = FloorProgressionService.Advance(currentFloor, BattleOutcome.Clear, gameData.Floors);
            nextFloorId = progressionResult.NextFloorId ?? 0;
            isGameComplete = progressionResult.IsRunComplete;
            
            // 보상 메시지 생성
            rewardMessage = $"Gold: +{rewardBundle.GoldAmount}\n";
            rewardMessage += $"Total Gold: {wallet.Gold}\n";
            
            if (rewardBundle.GrantedWeaponIds.Count > 0)
            {
                rewardMessage += $"\nWeapons: {rewardBundle.GrantedWeaponIds.Count}\n";
            }
            
            if (isGameComplete)
            {
                rewardMessage += "\nGame Complete!";
            }
            else
            {
                rewardMessage += $"\nNext Floor: {nextFloorId}";
            }
            
            // 팝업 표시
            showRewardPopup = true;
            Debug.Log("[Battle] Reward popup displayed");
        }
        
        private WeaponRow GetEquippedWeapon()
        {
            if (inventoryState == null || inventoryState.EquippedWeaponInstanceId == null)
            {
                return null;
            }
            
            if (!inventoryState.TryGetEquipment(inventoryState.EquippedWeaponInstanceId.Value, out var ownedEquipment))
            {
                Debug.LogWarning($"[Battle] Equipped weapon instance {inventoryState.EquippedWeaponInstanceId.Value} not found in inventory");
                return null;
            }
            
            var weapon = gameData.Weapons.Find(w => w.Id == ownedEquipment.WeaponId);
            if (weapon == null)
            {
                Debug.LogWarning($"[Battle] Weapon ID {ownedEquipment.WeaponId} not found in GameData");
                return null;
            }
            
            return weapon;
        }
        
        private int GetCurrentAttackPower()
        {
            if (equippedWeapon != null)
            {
                return equippedWeapon.BaseAttack;
            }
            
            // 기본 데미지 (무기 미장착 시)
            return 15;
        }
        
        private int GetNextInstanceId()
        {
            int maxId = 0;
            foreach (var equipment in inventoryState.Equipment)
            {
                if (equipment.InstanceId > maxId)
                {
                    maxId = equipment.InstanceId;
                }
            }
            return maxId + 1;
        }
    }
}