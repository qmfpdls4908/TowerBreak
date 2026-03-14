# Battle Scene Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Battle 씬에서 적을 스폰하고 플레이어가 공격/가드/대시할 수 있는 기본 전투 시스템 구현

**Architecture:** BattleSceneInitializer에서 WaveSpawnPlanner를 사용하여 적을 생성하고, BattleLoopController로 전투 로직을 관리합니다. 플레이어 입력을 받아 공격/가드를 처리합니다.

**Tech Stack:** Unity 6, Input System, Addressables, Object Pooling

---

## Task 1: BattleSceneInitializer에 적 스폰 연결

**Files:**
- Modify: `Assets/Scripts/Combat/BattleSceneInitializer.cs`
- Check: `Assets/Scripts/Combat/CombatProviderResolver.cs`
- Check: `Assets/Scripts/Combat/PooledCombatInstantiator.cs`

**Step 1: BattleSceneInitializer 수정**

```csharp
using System.Collections.Generic;
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
        
        private BattleLoopController battleLoopController;
        private IReadOnlyList<GameObject> spawnedEnemies;
        private IAddressableAssetProvider provider;
        private PooledCombatInstantiator pooled;
        private bool isAttackInFlight = false;
        
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
            Debug.Log($"[Battle] Floor {floor.FloorNumber}: {floor.WaveCount} waves, Wall HP: {floor.WallHealth}");
            
            // Provider 및 Pool 초기화
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
                
                // 적 스폰
                spawnedEnemies = await spawnService.SpawnAllEnemiesAsync(
                    gameData, 
                    currentFloor, 
                    enemySpawnParent,
                    index => new Vector3(index * 2f, 0f, 0f)
                );
                
                Debug.Log($"[Battle] Spawned {spawnedEnemies.Count} enemies");
                
                // BattleLoopController 초기화
                InitializeBattle(gameData, floor);
                
                Debug.Log("[Battle] Battle ready!");
                Debug.Log("[Battle] Controls: SPACE = Attack, G = Guard");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Battle] Failed to spawn enemies: {ex.Message}");
            }
        }
        
        private void InitializeBattle(TowerBreakerGameData gameData, FloorRow floor)
        {
            // WaveSpawnPlan 생성
            var plan = WaveSpawnPlanner.CreatePlan(floor, gameData.FloorWaves, gameData.Enemies);
            
            // CombatEnemyState 목록 생성
            var combatEnemies = new List<CombatEnemyState>();
            int enemyIndex = 0;
            
            foreach (var entry in plan.Entries)
            {
                var enemyRow = gameData.Enemies.Find(e => e.Id == entry.EnemyId);
                if (enemyRow != null)
                {
                    for (int i = 0; i < entry.Quantity; i++)
                    {
                        combatEnemies.Add(new CombatEnemyState(enemyRow.Id, enemyRow.Health, enemyRow.Pressure));
                        enemyIndex++;
                    }
                }
            }
            
            // CombatState 생성
            var combatState = CombatState.CreateInitial(floor.WallHealth, floor.WallMaxHealth, combatEnemies);
            
            // BattleDebugService 생성
            var battleService = new CombatDebugBattleService(combatState);
            
            // BattleLoopController 생성
            battleLoopController = new BattleLoopController(
                battleService,
                PressureTickInterval,
                WallHitThreshold,
                WallDamagePerHit
            );
            
            Debug.Log($"[Battle] Initialized with {combatEnemies.Count} enemies, Wall HP: {floor.WallHealth}");
        }
        
        private void Update()
        {
            if (battleLoopController == null) return;
            
            // 적 압박 업데이트
            var result = battleLoopController.Update(Time.deltaTime);
            
            if (result.DidAdvancePressure)
            {
                if (result.PressureResult.DefeatedWall)
                {
                    Debug.Log("[Battle] WALL DEFEATED! Game Over.");
                    return;
                }
            }
            
            // 입력 처리
            if (!isAttackInFlight && IsAttackInputDown())
            {
                _ = HandleAttackAsync(15); // 15 damage
            }
            else if (IsGuardInputDown())
            {
                HandleGuard();
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
                    Debug.Log($"[Battle] Hit enemy {result.TargetEnemyId}, Defeated: {result.TargetDefeated}");
                    
                    if (result.TargetDefeated)
                    {
                        // 적 처치 시 처리
                        await HandleEnemyDefeated(result.TargetEnemyId);
                    }
                }
                else
                {
                    Debug.Log("[Battle] No enemies remaining!");
                }
            }
            finally
            {
                isAttackInFlight = false;
            }
        }
        
        private async Task HandleEnemyDefeated(int enemyId)
        {
            // 처치된 적 GameObject 제거
            // TODO: 애니메이션/파티클 효과 추가
            Debug.Log($"[Battle] Enemy {enemyId} defeated!");
        }
        
        private void HandleGuard()
        {
            var result = battleLoopController.ApplyPlayerGuard(GuardPressureReduction);
            Debug.Log($"[Battle] Guard! Pressure reduced: {result.PressureReduced:F2}");
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
```

**Step 2: 필요한 using 추가 확인**

---

## Task 2: 플레이어 입력 시스템 연결

**Files:**
- Modify: `Assets/Scripts/Combat/BattleSceneInitializer.cs`

**Step 1: Input System 설정 확인**

Project Settings → Input System Package 확인

**Step 2: 키 입력 로직 검증**

- Space: 공격
- G: 가드

---

## Task 3: Battle UI 추가

**Files:**
- Create: `Assets/Scripts/Combat/BattleUIManager.cs`

**Step 1: 간단한 UI 매니저 작성**

```csharp
using UnityEngine;
using UnityEngine.UI;

namespace TowerBreak.Combat
{
    public sealed class BattleUIManager : MonoBehaviour
    {
        [SerializeField] private Text statusText;
        [SerializeField] private Text wallHealthText;
        [SerializeField] private Text enemyCountText;
        
        public void UpdateStatus(string message)
        {
            if (statusText != null)
                statusText.text = message;
        }
        
        public void UpdateWallHealth(int current, int max)
        {
            if (wallHealthText != null)
                wallHealthText.text = $"Wall: {current}/{max}";
        }
        
        public void UpdateEnemyCount(int count)
        {
            if (enemyCountText != null)
                enemyCountText.text = $"Enemies: {count}";
        }
    }
}
```

---

## Task 4: 카메라 설정

**Files:**
- Modify: `Assets/Scenes/Battle.unity` (Unity Editor)

**Step 1: Main Camera 설정**

- Projection: Orthographic
- Size: 5
- Position: (0, 0, -10)

**Step 2: Camera Controller (선택)**

```csharp
using UnityEngine;

namespace TowerBreak.Combat
{
    public sealed class BattleCameraController : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);
        [SerializeField] private float smoothSpeed = 5f;
        
        private void LateUpdate()
        {
            if (target == null) return;
            
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;
        }
    }
}
```

---

## Task 5: 테스트 및 검증

**Files:**
- Test: Play Mode

**Step 1: Battle 씬 테스트**

1. Unity에서 Battle 씬 직접 실행
2. Console 로그 확인:
   - "[Battle] Spawned X enemies"
   - "[Battle] Initialized with X enemies"

**Step 2: 입력 테스트**

- Space 키: 공격 로그 확인
- G 키: 가드 로그 확인

**Step 3: 전투 흐름 테스트**

- Bootstrap → Lobby → Battle (B키)
- 전투 진행 확인
- 모든 적 처치 시 승리 조건

---

## Task 6: 문서화

**Files:**
- Update: `docs/plans/2026-03-14-scene-bootstrap.md`

**Step 1: 완료 사항 기록**

- Battle 씬 구체화 완료
- 적 스폰 시스템 연결
- 플레이어 입력 처리
- UI 시스템 추가

**Acceptance Criteria**

- [ ] Battle 씬에서 적이 스폰됨
- [ ] Space 키로 공격 가능
- [ ] G 키로 가드 가능
- [ ] 적 처치 시 GameObject 제거
- [ ] 벽 HP가 0이 되면 게임 오버
- [ ] 모든 적 처치 시 승리
