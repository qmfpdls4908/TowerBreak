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
using TowerBreak.EventBus;

namespace TowerBreak.Combat
{
    public sealed class BattleSceneInitializer : MonoBehaviour
    {
        [SerializeField] private int currentFloor = 1;
        [SerializeField] private Transform enemySpawnParent;
        [SerializeField] private string playerPrefabPath = "Prefabs/Combat/Player";
        [SerializeField] private StageManager stageManager;
        
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
        private bool isFloorClearPopupShown = false;
        private BattleUIInstaller uiInstaller;

        private const float PressureTickInterval = 0.5f;
        private const float WallHitThreshold = 10f;
        private const int WallDamagePerHit = 1;
        private const float GuardPressureReduction = 5f;
        private const string CurrentFloorKey = "CurrentBattleFloor";
        
        private void Awake()
        {
            if (stageManager == null)
            {
                stageManager = FindFirstObjectByType<StageManager>();
            }
            
            if (stageManager == null)
            {
                Debug.LogError("[BattleSceneInitializer] StageManager not found!");
            }
            
            // 저장된 층 로드
            LoadCurrentFloor();
        }
        
        private void OnGUI()
        {
            // 기존 reward popup 처리
            if (showRewardPopup)
            {
                DrawRewardPopup();
            }

            // 층 클리어 팝업
            if (isFloorClearPopupShown)
            {
                DrawFloorClearPopup();
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
            
            // StageManager와 층 동기화
            if (stageManager != null)
            {
                // StageManager가 아직 초기화되지 않았으면 InitializeStage 호출
                if (stageManager.TotalFloors == 0)
                {
                    stageManager.InitializeStage();
                }
                
                // 현재 층이 StageManager와 다르면 업데이트
                if (stageManager.CurrentFloor != currentFloor)
                {
                    // 층 유효성 검사
                    if (currentFloor > 0 && currentFloor <= stageManager.TotalFloors)
                    {
                        stageManager.SetFloor(currentFloor);
                    }
                    else
                    {
                        Debug.LogWarning($"[Battle] Invalid floor {currentFloor}, resetting to floor 1");
                        currentFloor = 1;
                        SaveCurrentFloor();
                        stageManager.SetFloor(1);
                    }
                }
                
                // StageManager 초기화 대기 (비동기) - FloorStage 생성 시간 확보
                await Task.Delay(500);
            }
            
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
            
            // EventBus 인스턴스 생성 및 DI 등록
            var container = DIGlobalContext.EnsureContainer();
            var playerActionEventBus = new EventBus<PlayerActionEvent>();
            var playerDamagedEventBus = new EventBus<PlayerDamagedEvent>();
            container.Register(playerActionEventBus);
            container.Register(playerDamagedEventBus);
            Debug.Log("[Battle] EventBus instances registered to DI container");
            
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
                
                // FloorStage에서 적 스폰
                var currentFloorStage = GetCurrentFloorStage();
                var enemies = await SpawnEnemiesAtFloorStage(spawnService, gameData, currentFloor, currentFloorStage);
                
                spawnedEnemies = new List<GameObject>(enemies);
                Debug.Log($"[Battle] Spawned {spawnedEnemies.Count} enemies at FloorStage positions");
                
                // BattleLoopController 초기화
                InitializeBattle(gameData, floor);
                
                // 플레이어 생성 (무기 정보 전달)
                CreatePlayer(equippedWeapon);
                
                // UI 인스톨러 초기화 (BattleInputView와 PlayerController 연결)
                InitializeBattleUI();
                
                Debug.Log("[Battle] Battle ready!");
                Debug.Log($"[Battle] Controls: SPACE = Attack ({GetCurrentAttackPower()} damage) | G = Guard | Left Shift = Dash | UI Buttons");
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
            
            // FloorStage에서 벽 위치 가져오기
            Vector3 wallPosition = GetPlayerSpawnPosition();
            
            // 프리팹 인스턴스화
            GameObject playerObject = Instantiate(playerPrefab, transform);
            playerObject.name = "Player";
            playerObject.transform.position = wallPosition;
            
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
            
            Debug.Log($"[Battle] Player created from prefab at wall position: {playerObject.transform.position}");
        }
        
        private void CreatePlayerFallback(WeaponRow weapon)
        {
            Debug.Log("[Battle] Creating player from code (fallback)...");
            
            // FloorStage에서 벽 위치 가져오기
            Vector3 wallPosition = GetPlayerSpawnPosition();
            
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.SetParent(transform);
            playerObject.transform.position = wallPosition;
            
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
            
            Debug.Log($"[Battle] Player created from code at wall position: {playerObject.transform.position}");
        }
        
        private Vector3 GetPlayerSpawnPosition()
        {
            var floorStage = GetCurrentFloorStage();
            if (floorStage != null)
            {
                Vector3 wallPos = floorStage.GetWallPosition();
                Debug.Log($"[Battle] Using FloorStage wall position: {wallPos}");
                return wallPos;
            }
            
            Debug.Log("[Battle] FloorStage not found, using default position");
            return new Vector3(-6f, 0f, 0f);
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
            // FloorStage에서 벽 체력 가져오기
            int wallHealth = GetWallHealthForFloor();
            
            var combatState = CombatState.CreateInitial(wallHealth, wallHealth, combatEnemies);
            
            // BattleDebugService 생성
            var battleService = new CombatDebugBattleService(combatState);
            
            // BattleLoopController 생성
            battleLoopController = new BattleLoopController(
                battleService,
                PressureTickInterval,
                WallHitThreshold,
                WallDamagePerHit
            );
            
            Debug.Log($"[Battle] Initialized with {combatEnemies.Count} enemies, Wall HP: {wallHealth}");
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
            
            // UI 버튼 상태 업데이트
            UpdateUIButtonStates();
            
            // 승리 조건 체크 - 모든 적 처치
            // spawnedEnemies.Count == 0: 실제로 스폰된 모든 적이 파괴됨
            // battleLoopController.State.Enemies.Count == 0: 전투 시스템에서 모든 적이 제거됨
            bool allSpawnedEnemiesDefeated = spawnedEnemies.Count == 0;
            bool allLogicEnemiesDefeated = battleLoopController.State.Enemies.Count == 0;
            
            if ((allSpawnedEnemiesDefeated || allLogicEnemiesDefeated) && !victoryHandled)
            {
                Debug.Log($"[Battle] VICTORY! All enemies defeated! (Spawned: {allSpawnedEnemiesDefeated}, Logic: {allLogicEnemiesDefeated})");
                victoryHandled = true;
                HandleFloorClear(); // 층 클리어 처리
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
                playerController.PerformAction(PlayerActionType.Attack);
            }
            // G: 가드
            else if (keyboard.gKey.wasPressedThisFrame)
            {
                playerController.PerformAction(PlayerActionType.Guard);
                HandleGuard();
            }
            // Left Shift: 대시
            else if (keyboard.leftShiftKey.wasPressedThisFrame)
            {
                playerController.PerformAction(PlayerActionType.Dash);
            }
        }
        
        private void InitializeBattleUI()
        {
            // BattleUIInstaller가 씬에 있는지 확인
            uiInstaller = FindFirstObjectByType<BattleUIInstaller>();
            if (uiInstaller == null)
            {
                // Canvas 찾기
                var canvas = FindFirstObjectByType<UnityEngine.Canvas>();
                if (canvas == null)
                {
                    Debug.LogWarning("[Battle] Canvas not found. UI buttons will not work.");
                    return;
                }
                
                // 없으면 Canvas에 생성
                var installerObject = new GameObject("BattleUIInstaller");
                installerObject.transform.SetParent(canvas.transform);
                uiInstaller = installerObject.AddComponent<BattleUIInstaller>();
                Debug.Log("[Battle] BattleUIInstaller created on Canvas");
            }
            else
            {
                Debug.Log("[Battle] BattleUIInstaller found");
            }
            
            // 버튼 이벤트 연결
            if (uiInstaller != null)
            {
                SetupUIButtonListeners();
                Debug.Log("[Battle] UI button listeners set up");
            }
        }
        
        private void SetupUIButtonListeners()
        {
            if (uiInstaller.AttackButton != null)
            {
                uiInstaller.AttackButton.onClick.AddListener(OnAttackButtonClicked);
            }
            if (uiInstaller.GuardButton != null)
            {
                uiInstaller.GuardButton.onClick.AddListener(OnGuardButtonClicked);
            }
            if (uiInstaller.DashButton != null)
            {
                uiInstaller.DashButton.onClick.AddListener(OnDashButtonClicked);
            }
        }
        
        private void OnAttackButtonClicked()
        {
            Debug.Log("[BattleSceneInitializer] Attack button clicked");
            if (playerController == null) return;
            
            playerController.PerformAction(PlayerActionType.Attack);
        }
        
        private void OnGuardButtonClicked()
        {
            Debug.Log("[BattleSceneInitializer] Guard button clicked");
            if (playerController == null) return;
            
            // G 키와 동일한 동작
            playerController.PerformAction(PlayerActionType.Guard);
            HandleGuard();
            
            // 가드 시 몬스터 이동 재개
            ResumeAllMonsters();
        }
        
        private void OnDashButtonClicked()
        {
            Debug.Log("[BattleSceneInitializer] Dash button clicked");
            if (playerController == null) return;
            
            // Left Shift 키와 동일한 동작
            playerController.PerformAction(PlayerActionType.Dash);
            
            // 대시 시 몬스터 이동 재개
            ResumeAllMonsters();
        }
        
        private void ResumeAllMonsters()
        {
            foreach (var enemy in spawnedEnemies)
            {
                var controller = enemy.GetComponent<EnemyController>();
                if (controller != null)
                {
                    controller.StopPushing();
                }
            }
            Debug.Log("[BattleSceneInitializer] All monsters resumed");
        }
        
        private void UpdateUIButtonStates()
        {
            if (uiInstaller == null || playerController == null) return;
            
            // 공격 버튼: 스턴 상태거나 액션 중이면 비활성화
            if (uiInstaller.AttackButton != null)
            {
                uiInstaller.AttackButton.interactable = playerController.CanAttack;
            }
            
            // 가드/대시 버튼: 액션 중이 아니면 활성화
            if (uiInstaller.GuardButton != null)
            {
                uiInstaller.GuardButton.interactable = playerController.CanPerformAction(PlayerActionType.Guard);
            }
            
            if (uiInstaller.DashButton != null)
            {
                uiInstaller.DashButton.interactable = playerController.CanPerformAction(PlayerActionType.Dash);
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
            
            // 처치된 적의 인덱스 찾기
            int defeatedEnemyIndex = -1;
            var currentEnemies = battleLoopController.State.Enemies;
            
            // 이전 상태와 비교하여 어떤 인덱스가 제거되었는지 찾기
            // spawnedEnemies는 combatEnemies와 같은 순서로 생성되었으므로
            // combatEnemies에서 enemyId를 찾아 인덱스를 확인
            for (int i = 0; i < spawnedEnemies.Count; i++)
            {
                var controller = spawnedEnemies[i].GetComponent<EnemyController>();
                if (controller != null && controller.EnemyId == enemyId)
                {
                    defeatedEnemyIndex = i;
                    break;
                }
            }
            
            if (defeatedEnemyIndex >= 0 && defeatedEnemyIndex < spawnedEnemies.Count)
            {
                var enemyToRemove = spawnedEnemies[defeatedEnemyIndex];
                spawnedEnemies.RemoveAt(defeatedEnemyIndex);
                
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
            else
            {
                // fallback: 첫 번째 적 제거 (기존 동작)
                var enemyToRemove = spawnedEnemies.FirstOrDefault();
                if (enemyToRemove != null)
                {
                    spawnedEnemies.Remove(enemyToRemove);
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

        private void HandleFloorClear()
        {
            Debug.Log($"[Battle] Floor {currentFloor} cleared!");

            // FloorStage에 클리어 표시
            var floorStage = GetCurrentFloorStage();
            if (floorStage != null)
            {
                floorStage.MarkAsCompleted();
            }
            
            // 보상 계산
            CalculateAndApplyRewards();

            // 마지막 층(보스) 체크
            if (stageManager != null && currentFloor >= stageManager.TotalFloors)
            {
                ShowGameCompletePopup();
                return;
            }

            // 층 클리어 팝업 표시
            ShowFloorClearPopup();
        }
        
        private void CalculateAndApplyRewards()
        {
            try
            {
                var floor = gameData.Floors[currentFloor - 1];
                var random = new System.Random();
                currentReward = RewardResolver.Resolve(floor, gameData.RewardTables, gameData.RewardEntries, random);
                
                // 보상 적용
                var wallet = DIContainer.ResolveFromRegistered<PlayerWalletState>();
                if (wallet != null)
                {
                    int nextInstanceId = GetNextInstanceId();
                    Debug.Log($"[Battle] Applying reward: Gold={currentReward.GoldAmount}, Weapons={currentReward.GrantedWeaponIds.Count}");
                    for (int i = 0; i < currentReward.GrantedWeaponIds.Count; i++)
                    {
                        Debug.Log($"[Battle] Weapon reward {i + 1}: WeaponId={currentReward.GrantedWeaponIds[i]}");
                    }
                    BattleRewardService.Apply(currentReward, wallet, inventoryState, () => nextInstanceId++);
                    Debug.Log($"[Battle] Reward applied successfully");
                }
                else
                {
                    Debug.LogWarning("[Battle] PlayerWalletState not found, cannot apply rewards");
                }
                
                Debug.Log($"[Battle] Reward calculated: {currentReward.GoldAmount} gold");
            }
            catch (System.InvalidOperationException ex) when (ex.Message.Contains("Weapon drop"))
            {
                // 무기 드랍 실패 시 골드만 보상
                Debug.LogWarning($"[Battle] Weapon drop failed, using fallback gold reward: {ex.Message}");
                var floor = gameData.Floors[currentFloor - 1];
                var rewardTable = gameData.RewardTables.Find(t => t.Id == floor.RewardTableId);
                int goldAmount = rewardTable?.GuaranteedGold ?? 20;
                currentReward = new RewardBundle(goldAmount, new System.Collections.Generic.List<int>());
                
                // 골드 보상만 적용
                var wallet = DIContainer.ResolveFromRegistered<PlayerWalletState>();
                if (wallet != null)
                {
                    wallet.AddGold(goldAmount);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[Battle] Reward calculation failed: {ex.Message}");
                currentReward = new RewardBundle(20, new System.Collections.Generic.List<int>());
            }
        }

        private void ShowFloorClearPopup()
        {
            isFloorClearPopupShown = true;
            Debug.Log("[Battle] Showing floor clear popup - Press 'Continue' to advance");
        }

        private void ShowGameCompletePopup()
        {
            Debug.Log("[Battle] Showing game complete popup");
            isFloorClearPopupShown = true;
            isGameComplete = true;
        }

        private void HandleGameComplete()
        {
            Debug.Log("[Battle] GAME COMPLETE! All floors cleared!");
            
            // 저장 데이터 삭제
            ClearSavedFloor();
            
            // 로비로 이동
            UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
        }

        private void AdvanceToNextFloor()
        {
            Debug.Log($"[Battle] Advancing to floor {currentFloor + 1}...");
            
            // 다음 층 저장
            currentFloor++;
            SaveCurrentFloor();
            
            // 씬 리로드
            UnityEngine.SceneManagement.SceneManager.LoadScene("Battle");
        }

        private void LoadCurrentFloor()
        {
            if (PlayerPrefs.HasKey(CurrentFloorKey))
            {
                currentFloor = PlayerPrefs.GetInt(CurrentFloorKey);
                Debug.Log($"[Battle] Loaded saved floor: {currentFloor}");
            }
            else
            {
                currentFloor = 1;
                Debug.Log("[Battle] No saved floor, starting from floor 1");
            }
        }

        private void SaveCurrentFloor()
        {
            PlayerPrefs.SetInt(CurrentFloorKey, currentFloor);
            PlayerPrefs.Save();
            Debug.Log($"[Battle] Saved current floor: {currentFloor}");
        }

        private void ClearSavedFloor()
        {
            if (PlayerPrefs.HasKey(CurrentFloorKey))
            {
                PlayerPrefs.DeleteKey(CurrentFloorKey);
                PlayerPrefs.Save();
                Debug.Log("[Battle] Cleared saved floor data");
            }
        }

        private void HandleFloorFail()
        {
            Debug.Log($"[Battle] Floor {currentFloor} failed!");
            
            // 저장 데이터 삭제 (처음부터 다시 시작)
            ClearSavedFloor();
            
            // FloorStage에 실패 표시
            var floorStage = GetCurrentFloorStage();
            if (floorStage != null)
            {
                floorStage.MarkAsFailed();
            }
            
            // TODO: 게임 오버 화면 표시
            
            // 로비로 이동
            UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
        }

        private void DrawFloorClearPopup()
        {
            // 배경 (반투명)
            GUI.color = new Color(0, 0, 0, 0.8f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // 팝업 창
            float popupWidth = 400;
            float popupHeight = 250;
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
            titleStyle.normal.textColor = Color.green;
            GUI.Label(new Rect(popupRect.x, popupRect.y + 30, popupWidth, 50), "FLOOR CLEAR!", titleStyle);

            // 층 정보
            GUIStyle contentStyle = new GUIStyle(GUI.skin.label);
            contentStyle.fontSize = 24;
            contentStyle.alignment = TextAnchor.MiddleCenter;
            contentStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(popupRect.x, popupRect.y + 90, popupWidth, 40), $"Floor {currentFloor} Completed", contentStyle);

            // 버튼 스타일
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 22;
            buttonStyle.alignment = TextAnchor.MiddleCenter;

            // 보상 정보 표시
            if (currentReward != null)
            {
                GUIStyle rewardStyle = new GUIStyle(GUI.skin.label);
                rewardStyle.fontSize = 20;
                rewardStyle.alignment = TextAnchor.MiddleCenter;
                rewardStyle.normal.textColor = Color.yellow;

                // 골드 표시
                GUI.Label(new Rect(popupRect.x, popupRect.y + 125, popupWidth, 30), $"Gold: +{currentReward.GoldAmount}", rewardStyle);

                // 무기 드랍 표시
                if (currentReward.GrantedWeaponIds != null && currentReward.GrantedWeaponIds.Count > 0)
                {
                    string weaponNames = GetWeaponNames(currentReward.GrantedWeaponIds);
                    GUI.Label(new Rect(popupRect.x, popupRect.y + 155, popupWidth, 30), $"Weapon: {weaponNames}", rewardStyle);
                }
            }
            
            // 버튼들
            float buttonWidth = 140;
            float buttonHeight = 45;
            float buttonY = popupRect.y + popupHeight - 70;
            
            if (isGameComplete)
            {
                // 게임 완료 - 축하 메시지와 나가기 버튼만
                GUIStyle completeStyle = new GUIStyle(GUI.skin.label);
                completeStyle.fontSize = 28;
                completeStyle.alignment = TextAnchor.MiddleCenter;
                completeStyle.normal.textColor = Color.yellow;
                GUI.Label(new Rect(popupRect.x, popupRect.y + 80, popupWidth, 40), "GAME COMPLETE!", completeStyle);
                
                // 나가기 버튼 (중앙)
                if (GUI.Button(new Rect(popupRect.x + (popupWidth - buttonWidth) / 2, buttonY, buttonWidth, buttonHeight), "나가기", buttonStyle))
                {
                    isFloorClearPopupShown = false;
                    HandleGameComplete();
                }
            }
            else
            {
                // 계속하기 버튼
                if (GUI.Button(new Rect(popupRect.x + 30, buttonY, buttonWidth, buttonHeight), "계속하기", buttonStyle))
                {
                    isFloorClearPopupShown = false;
                    AdvanceToNextFloor();
                }
                
                // 나가기 버튼
                if (GUI.Button(new Rect(popupRect.x + popupWidth - buttonWidth - 30, buttonY, buttonWidth, buttonHeight), "나가기", buttonStyle))
                {
                    isFloorClearPopupShown = false;
                    ClearSavedFloor();
                    UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
                }
            }
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
            if (equippedWeapon == null)
            {
                Debug.Log("[Battle] GetCurrentAttackPower: No equipped weapon, returning default 15");
                return 15;
            }

            // 장착된 무기의 인스턴스 ID로 OwnedEquipment 찾기
            if (inventoryState == null || !inventoryState.EquippedWeaponInstanceId.HasValue)
            {
                Debug.Log($"[Battle] GetCurrentAttackPower: No equipped instance ID, returning BaseAttack {equippedWeapon.BaseAttack}");
                return equippedWeapon.BaseAttack;
            }

            if (!inventoryState.TryGetEquipment(inventoryState.EquippedWeaponInstanceId.Value, out var ownedEquipment))
            {
                Debug.Log($"[Battle] GetCurrentAttackPower: OwnedEquipment not found for ID {inventoryState.EquippedWeaponInstanceId.Value}, returning BaseAttack {equippedWeapon.BaseAttack}");
                return equippedWeapon.BaseAttack;
            }

            // 강화 레벨에 따른 공격력 계산
            int enhancementLevel = ownedEquipment.EnhancementLevel;
            Debug.Log($"[Battle] GetCurrentAttackPower: Weapon={equippedWeapon.Archetype}, Level={enhancementLevel}, BaseAttack={equippedWeapon.BaseAttack}");
            
            if (enhancementLevel <= 0)
            {
                return equippedWeapon.BaseAttack;
            }

            // 레벨당 10씩 증가 (EquipmentEnhancementService와 동일한 계산)
            int totalBonus = 0;
            for (int level = 1; level <= enhancementLevel; level++)
            {
                totalBonus += 10 * level;
            }

            int finalAttack = equippedWeapon.BaseAttack + totalBonus;
            Debug.Log($"[Battle] GetCurrentAttackPower: Final attack power={finalAttack} (Base={equippedWeapon.BaseAttack}, Bonus={totalBonus})");
            return finalAttack;
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

        private string GetWeaponNames(System.Collections.Generic.IReadOnlyList<int> weaponIds)
        {
            if (weaponIds == null || weaponIds.Count == 0)
            {
                return "None";
            }

            var names = new System.Collections.Generic.List<string>();
            foreach (int weaponId in weaponIds)
            {
                var weapon = gameData.Weapons.Find(w => w.Id == weaponId);
                if (weapon != null)
                {
                    names.Add(weapon.Archetype.ToString());
                }
            }

            return names.Count > 0 ? string.Join(", ", names) : "Unknown";
        }
        
        private int GetWallHealthForFloor()
        {
            var floorStage = GetCurrentFloorStage();
            
            if (floorStage != null && floorStage.IsBossFloor)
            {
                return 150;
            }
            
            return 100;
        }
        
        private FloorStage GetCurrentFloorStage()
        {
            if (stageManager == null) return null;
            
            FloorStage[] allFloors = stageManager.GetComponentsInChildren<FloorStage>();
            foreach (var floor in allFloors)
            {
                if (floor.IsCurrentFloor)
                {
                    return floor;
                }
            }
            
            if (allFloors.Length > 0)
            {
                return allFloors[0];
            }
            
            return null;
        }
        
        private async Task<List<GameObject>> SpawnEnemiesAtFloorStage(
            CombatDebugSpawnService spawnService,
            TowerBreakerGameData gameData,
            int floorNumber,
            FloorStage floorStage)
        {
            var enemies = new List<GameObject>();
            
            if (floorStage != null && floorStage.EnemySpawnPoints != null)
            {
                Transform spawnParent = floorStage.EnemySpawnPoints;
                
                var floor = gameData.Floors[floorNumber - 1];
                var floorWaves = gameData.FloorWaves.FindAll(w => w.FloorId == floor.Id);
                
                int enemyIndex = 0;
                foreach (var wave in floorWaves)
                {
                    var enemyRow = gameData.Enemies.Find(e => e.Id == wave.EnemyId);
                    if (enemyRow != null)
                    {
                        for (int i = 0; i < wave.Quantity; i++)
                        {
                            // 단일 스폰 포인트 사용 (index 0)
                            Vector3 spawnPosition = floorStage.GetSpawnPosition(0);
                            
                            GameObject enemy = await SpawnEnemyAtPosition(enemyRow, spawnPosition, spawnParent);
                            if (enemy != null)
                            {
                                enemies.Add(enemy);
                                enemyIndex++;
                            }
                        }
                    }
                }
                
                Debug.Log($"[Battle] Spawned {enemies.Count} enemies at FloorStage {floorNumber}");
            }
            else
            {
                Debug.LogWarning("[Battle] FloorStage not found, using default spawn");
                var fallbackEnemies = await spawnService.SpawnAllEnemiesAsync(
                    gameData, 
                    floorNumber, 
                    enemySpawnParent,
                    index => new Vector3(index * 2f - 3f, 0f, 0f)
                );
                enemies = new List<GameObject>(fallbackEnemies);
            }
            
            return enemies;
        }
        
        private async Task<GameObject> SpawnEnemyAtPosition(EnemyRow enemyRow, Vector3 position, Transform parent)
        {
            GameObject enemyPrefab = await provider.LoadAssetAsync<GameObject>(enemyRow.PrefabKey);
            if (enemyPrefab == null)
            {
                enemyPrefab = CreateFallbackEnemyPrefab();
            }
            
            GameObject enemy = Instantiate(enemyPrefab, parent);
            enemy.transform.position = position;
            enemy.name = $"Enemy_{enemyRow.Id}";
            
            var controller = enemy.GetComponent<EnemyController>();
            if (controller == null)
            {
                controller = enemy.AddComponent<EnemyController>();
            }
            controller.Initialize(enemyRow);
            
            return enemy;
        }
        
        private GameObject CreateFallbackEnemyPrefab()
        {
            GameObject enemy = new GameObject("Enemy");
            
            var sr = enemy.AddComponent<SpriteRenderer>();
            sr.sprite = Sprite.Create(
                Texture2D.whiteTexture,
                new Rect(0, 0, 1, 1),
                new Vector2(0.5f, 0.5f),
                100f
            );
            sr.color = Color.red;
            
            enemy.AddComponent<EnemyController>();
            
            return enemy;
        }
    }
}