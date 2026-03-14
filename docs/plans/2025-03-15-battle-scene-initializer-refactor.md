# BattleSceneInitializer Refactor Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** BattleSceneInitializer를 개선하여 FloorStage의 스폰 포인트와 벽 위치를 사용하도록 수정

**Architecture:** StageManager가 생성한 FloorStage에서 스폰 포인트와 벽 위치 정보를 가져와서 적 스폰과 플레이어 위치를 설정합니다. BattleSceneInitializer는 StageManager와 협력하여 전투를 초기화합니다.

**Tech Stack:** Unity, C#, TowerBreak.Combat 어셈블리

---

## Task 1: StageManager 참조 추가 및 초기화 연동

**Files:**
- Modify: `Assets/Scripts/Combat/BattleSceneInitializer.cs`

**Step 1: StageManager 참조 필드 추가**

BattleSceneInitializer 클래스 상단에 StageManager 참조 추가:

```csharp
public sealed class BattleSceneInitializer : MonoBehaviour
{
    [SerializeField] private int currentFloor = 1;
    [SerializeField] private Transform enemySpawnParent;
    [SerializeField] private string playerPrefabPath = "Prefabs/Combat/Player";
    [SerializeField] private StageManager stageManager; // 추가
    
    // ... existing fields ...
```

**Step 2: Awake에서 StageManager 찾기**

```csharp
private void Awake()
{
    // StageManager 자동 찾기
    if (stageManager == null)
    {
        stageManager = FindFirstObjectByType<StageManager>();
    }
    
    if (stageManager == null)
    {
        Debug.LogError("[BattleSceneInitializer] StageManager not found!");
    }
}
```

**Step 3: Start 메서드 수정 - StageManager와 층 동기화**

```csharp
private async void Start()
{
    Debug.Log($"[Battle] Initializing battle scene for floor {currentFloor}...");
    
    // StageManager와 층 동기화
    if (stageManager != null && stageManager.CurrentFloor != currentFloor)
    {
        stageManager.SetFloor(currentFloor);
    }
    
    // ... rest of initialization ...
}
```

**Commit:**
```bash
git add Assets/Scripts/Combat/BattleSceneInitializer.cs
git commit -m "feat: add StageManager reference to BattleSceneInitializer"
```

---

## Task 2: FloorStage에서 스폰 포인트 가져오기

**Files:**
- Modify: `Assets/Scripts/Combat/BattleSceneInitializer.cs`

**Step 1: 현재 FloorStage 가져오는 메서드 추가**

```csharp
private FloorStage GetCurrentFloorStage()
{
    if (stageManager == null) return null;
    
    // StageManager에서 현재 층의 FloorStage 찾기
    FloorStage[] allFloors = stageManager.GetComponentsInChildren<FloorStage>();
    foreach (var floor in allFloors)
    {
        if (floor.IsCurrentFloor)
        {
            return floor;
        }
    }
    
    // 현재 층을 찾지 못하면 첫 번째 반환
    if (allFloors.Length > 0)
    {
        return allFloors[0];
    }
    
    return null;
}
```

**Step 2: 적 스폰 로직 수정 - FloorStage의 스폰 포인트 사용**

```csharp
// 기존 코드 (주석 처리됨):
// var enemies = await spawnService.SpawnAllEnemiesAsync(
//     gameData, 
//     currentFloor, 
//     enemySpawnParent,
//     index => new Vector3(index * 2f - 3f, 0f, 0f)  // 중앙 정렬
// );

// 새로운 코드:
var currentFloorStage = GetCurrentFloorStage();
var enemies = await SpawnEnemiesAtFloorStage(spawnService, gameData, currentFloor, currentFloorStage);
```

**Step 3: FloorStage 기반 적 스폰 메서드 구현**

```csharp
private async Task<List<GameObject>> SpawnEnemiesAtFloorStage(
    CombatDebugSpawnService spawnService,
    TowerBreakerGameData gameData,
    int floorNumber,
    FloorStage floorStage)
{
    var enemies = new List<GameObject>();
    
    if (floorStage != null && floorStage.EnemySpawnPoints != null)
    {
        // FloorStage의 스폰 포인트 사용
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
                    // 스폰 포인트에서 위치 가져오기
                    Vector3 spawnPosition = floorStage.GetSpawnPosition(enemyIndex % 5); // 5개 포인트 순환
                    
                    // 적 생성
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
        // FloorStage가 없으면 기존 방식 사용 (폴백)
        Debug.LogWarning("[Battle] FloorStage not found, using default spawn");
        enemies = await spawnService.SpawnAllEnemiesAsync(
            gameData, 
            floorNumber, 
            enemySpawnParent,
            index => new Vector3(index * 2f - 3f, 0f, 0f)
        );
    }
    
    return enemies;
}

private async Task<GameObject> SpawnEnemyAtPosition(EnemyRow enemyRow, Vector3 position, Transform parent)
{
    // 기존 provider 사용하여 적 생성
    GameObject enemyPrefab = await provider.LoadAssetAsync<GameObject>(enemyRow.PrefabKey);
    if (enemyPrefab == null)
    {
        // 폴백: 기본 적 생성
        enemyPrefab = CreateFallbackEnemyPrefab();
    }
    
    GameObject enemy = Instantiate(enemyPrefab, parent);
    enemy.transform.position = position;
    enemy.name = $"Enemy_{enemyRow.Id}";
    
    // EnemyController 설정
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
    
    // SpriteRenderer 추가
    var sr = enemy.AddComponent<SpriteRenderer>();
    sr.sprite = Sprite.Create(
        Texture2D.whiteTexture,
        new Rect(0, 0, 1, 1),
        new Vector2(0.5f, 0.5f),
        100f
    );
    sr.color = Color.red;
    
    // EnemyController 추가
    enemy.AddComponent<EnemyController>();
    
    return enemy;
}
```

**Commit:**
```bash
git add Assets/Scripts/Combat/BattleSceneInitializer.cs
git commit -m "feat: spawn enemies at FloorStage spawn points"
```

---

## Task 3: 플레이어 생성 위치를 FloorStage의 벽 위치로 변경

**Files:**
- Modify: `Assets/Scripts/Combat/BattleSceneInitializer.cs`

**Step 1: CreatePlayer 메서드 수정 - FloorStage의 벽 위치 사용**

```csharp
private void CreatePlayer(WeaponRow weapon)
{
    Debug.Log("[Battle] Creating player from prefab...");
    
    // FloorStage에서 벽 위치 가져오기
    Vector3 wallPosition = GetPlayerSpawnPosition();
    
    // 프리팹 로드
    GameObject playerPrefab = Resources.Load<GameObject>(playerPrefabPath);
    if (playerPrefab == null)
    {
        Debug.LogError($"[Battle] Player prefab not found at: {playerPrefabPath}");
        CreatePlayerFallback(weapon, wallPosition);
        return;
    }
    
    // 프리팹 인스턴스화
    GameObject playerObject = Instantiate(playerPrefab, transform);
    playerObject.name = "Player";
    playerObject.transform.position = wallPosition; // FloorStage의 벽 위치로 변경
    
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
    
    Debug.Log($"[Battle] Player created at wall position: {playerObject.transform.position}");
}

private void CreatePlayerFallback(WeaponRow weapon, Vector3 wallPosition)
{
    Debug.Log("[Battle] Creating player from code (fallback)...");
    
    GameObject playerObject = new GameObject("Player");
    playerObject.transform.SetParent(transform);
    playerObject.transform.position = wallPosition; // FloorStage의 벽 위치로 변경
    
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
    
    // FloorStage가 없으면 기본값 사용
    Debug.Log("[Battle] FloorStage not found, using default position");
    return new Vector3(-6f, 0f, 0f);
}
```

**Commit:**
```bash
git add Assets/Scripts/Combat/BattleSceneInitializer.cs
git commit -m "feat: spawn player at FloorStage wall position"
```

---

## Task 4: BattleLoopController 초기화에 FloorStage 연동

**Files:**
- Modify: `Assets/Scripts/Combat/BattleSceneInitializer.cs`

**Step 1: InitializeBattle 메서드 수정 - FloorStage의 벽 체력 사용**

```csharp
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
    
    // FloorStage의 벽 위치와 체력 정보 사용
    var floorStage = GetCurrentFloorStage();
    int wallHealth = GetWallHealth(floorStage);
    
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

private int GetWallHealth(FloorStage floorStage)
{
    // TODO: FloorStage에 벽 체력 정보 추가 필요
    // 현재는 기본값 사용
    const int DefaultWallHealth = 100;
    
    if (floorStage != null && floorStage.IsBossFloor)
    {
        // 보스 층은 더 높은 벽 체력
        return 150;
    }
    
    return DefaultWallHealth;
}
```

**Commit:**
```bash
git add Assets/Scripts/Combat/BattleSceneInitializer.cs
git commit -m "feat: integrate FloorStage with battle initialization"
```

---

## Task 5: FloorStage에 벽 체력 정보 추가 (필요시)

**Files:**
- Modify: `Assets/Scripts/Combat/FloorStage.cs`

**Step 1: 벽 체력 필드 추가**

```csharp
public sealed class FloorStage : MonoBehaviour
{
    [SerializeField] private int floorNumber;
    [SerializeField] private FloorRow floorData;
    [SerializeField] private SpriteRenderer floorRenderer;
    [SerializeField] private Transform enemySpawnPoints;
    [SerializeField] private Transform wallPosition;
    [SerializeField] private int wallHealth = 100; // 벽 체력 추가
    
    // ... existing fields ...
    
    public int WallHealth => wallHealth; // 벽 체력 프로퍼티
    
    public void Initialize(int number, FloorRow data)
    {
        floorNumber = number;
        floorData = data;
        isCurrentFloor = false;
        isCompleted = false;
        isBossFloor = false;
        
        // 보스 층이면 벽 체력 증가
        if (data != null && IsBossFloorByData(data))
        {
            wallHealth = 150;
        }
        
        SetupVisuals();
        SetupSpawnPoints();
        SetupWallPosition();
        
        // ... rest of initialization ...
    }
    
    private bool IsBossFloorByData(FloorRow data)
    {
        // 보스 층 판별 로직 (필요시 구현)
        // 예: RewardTableId가 특정 값인 경우
        return data.RewardTableId >= 2000; // 예시
    }
    
    // ... rest of class ...
}
```

**Commit:**
```bash
git add Assets/Scripts/Combat/FloorStage.cs
git commit -m "feat: add wall health to FloorStage"
```

---

## Task 6: 정리 및 불필요한 코드 제거

**Files:**
- Modify: `Assets/Scripts/Combat/BattleSceneInitializer.cs`

**Step 1: 기존 enemySpawnParent 필드 제거 또는 deprecated 처리**

```csharp
// 기존 필드는 유지하되 선택적으로 사용 가능하도록 변경
[SerializeField] private Transform enemySpawnParent; // 이제 선택적, FloorStage가 없을 때만 사용

private void Awake()
{
    // StageManager 자동 찾기
    if (stageManager == null)
    {
        stageManager = FindFirstObjectByType<StageManager>();
    }
    
    if (stageManager == null)
    {
        Debug.LogWarning("[BattleSceneInitializer] StageManager not found! Will use fallback spawn.");
    }
}
```

**Step 2: 디버그 로그 정리**

필요한 로그만 남기고 불필요한 로그 제거

**Commit:**
```bash
git add Assets/Scripts/Combat/BattleSceneInitializer.cs
git commit -m "refactor: clean up BattleSceneInitializer and remove deprecated spawn logic"
```

---

## Verification Steps

**1. Unity 에디터에서 테스트:**
- Battle 씬 열기
- StageManager와 BattleSceneInitializer 확인
- 플레이 모드로 테스트

**2. 확인할 사항:**
- [ ] 플레이어가 FloorStage의 벽 위치에 생성됨
- [ ] 적들이 FloorStage의 스폰 포인트에 생성됨
- [ ] 층 전환 시 정상적으로 작동함
- [ ] 보스 층에서 보스 프리팹이 사용됨

**3. 예상 로그 출력:**
```
[BattleSceneInitializer] StageManager found: StageManager
[Battle] Player created at wall position: (-8.0, 0.0, 0.0)
[Battle] Spawned 5 enemies at FloorStage 1
```

---

## Summary

이 변경으로 BattleSceneInitializer는 다음과 같이 동작합니다:

1. **StageManager 자동 찾기** - 씬에서 StageManager를 자동으로 찾음
2. **FloorStage 연동** - 현재 층의 FloorStage에서 스폰 정보를 가져옴
3. **벽 위치 기반 플레이어 스폰** - FloorStage.WallPosition에서 플레이어 생성
4. **스폰 포인트 기반 적 스폰** - FloorStage.EnemySpawnPoints에서 적 생성
5. **벽 체력 연동** - FloorStage.WallHealth로 전투 초기화