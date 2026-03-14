# Enemy Visibility Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Battle 씬에서 적이 화면에 시각적으로 보이도록 SpriteRenderer 및 시각적 요소 추가

**Architecture:** EnemyController에 SpriteRenderer를 추가하고, Addressables에서 적 프리팹을 로드하여 화면에 표시합니다. 기본 도형(사각형/원) 스프라이트를 사용하여 빠르게 시각적 피드백을 제공합니다.

**Tech Stack:** Unity 6, SpriteRenderer, Addressables, 2D URP

---

## Task 1: 적 프리팹 확인 및 생성

**Files:**
- Check: `Assets/Prefabs/Combat/` 폴더 존재 여부
- Create: `Assets/Prefabs/Combat/EnemyBase.prefab`

**Step 1: Prefabs 폴더 확인**

```bash
# 폴더 존재 확인
ls Assets/Prefabs/
ls Assets/Prefabs/Combat/
```

**Step 2: EnemyBase 프리팹 구성 (Unity Editor에서)**

Hierarchy 구조:
```
EnemyBase (GameObject)
├── SpriteRenderer
│   ├── Sprite: Square (기본 스프라이트)
│   ├── Color: 빨간색 (FF0000)
│   └── Sorting Layer: Default
├── BoxCollider2D (선택)
└── EnemyController (Script)
```

**Step 3: Addressables 등록**

1. EnemyBase 선택
2. Inspector → Addressable 체크
3. Addressable Name: `EnemyBase` 또는 `Enemy_Melee`

---

## Task 2: GameData에 프리팹 키 추가

**Files:**
- Check: `Assets/Scripts/GameData/TowerBreaker/EnemyRow.cs`
- Check: `Assets/Resources/TowerBreakerGameData.asset`

**Step 1: EnemyRow에 PrefabKey 필드 확인**

```csharp
public class EnemyRow
{
    public int Id;
    public string Name;
    public int Health;
    public float Pressure;
    public string PrefabKey;  // 이 필드 확인
    // ...
}
```

**Step 2: TowerBreakerGameData에서 EnemyRow 확인**

Unity Inspector에서:
- Enemies 배열 확인
- 각 Enemy의 PrefabKey 필드 확인
- 비어있으면 "EnemyBase"로 설정

---

## Task 3: CombatDebugSpawnService에 시각적 검증 추가

**Files:**
- Check: `Assets/Scripts/Combat/CombatDebugSpawnService.cs`

**Step 1: 스폰된 적의 SpriteRenderer 확인**

스폰 로그에 시각적 정보 추가:
```csharp
// SpawnAllEnemiesAsync 내부에서
GameObject instance = await pooled.Instantiate(prefab, spawnPosition, spawnParent);

// SpriteRenderer 확인
var spriteRenderer = instance.GetComponent<SpriteRenderer>();
if (spriteRenderer == null)
{
    Debug.LogWarning($"[CombatDebug] Spawned enemy has no SpriteRenderer!");
    // 기본 SpriteRenderer 추가
    spriteRenderer = instance.AddComponent<SpriteRenderer>();
    spriteRenderer.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite");
    spriteRenderer.color = Color.red;
}
else
{
    Debug.Log($"[CombatDebug] Enemy spawned with sprite: {spriteRenderer.sprite?.name}");
}
```

---

## Task 4: BattleSceneInitializer에 카메라 위치 조정

**Files:**
- Modify: `Assets/Scripts/Combat/BattleSceneInitializer.cs`

**Step 1: 적 스폰 위치를 카메라 시야에 맞춤**

```csharp
// 현재: index => new Vector3(index * 2f, 0f, 0f)
// 변경: 화면 중앙에 스폰되도록 조정
index => new Vector3(index * 2f - 4f, 0f, 0f)  // 왼쪽에서 오른쪽으로 배치
```

**Step 2: 카메라 설정 로그 추가**

```csharp
private void Start()
{
    // ... 기존 코드 ...
    
    // 카메라 정보 로그
    var camera = Camera.main;
    if (camera != null)
    {
        Debug.Log($"[Battle] Camera: {camera.name}, OrthographicSize: {camera.orthographicSize}, Position: {camera.transform.position}");
    }
}
```

---

## Task 5: 적 시각적 표현 개선

**Files:**
- Modify: `Assets/Scripts/Combat/EnemyController.cs`

**Step 1: Awake에서 SpriteRenderer 자동 추가**

```csharp
private void Awake()
{
    spriteRenderer = GetComponent<SpriteRenderer>();
    if (spriteRenderer == null)
    {
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        // 기본 스프라이트 설정
        spriteRenderer.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background");
        spriteRenderer.color = Color.red;
        Debug.Log("[EnemyController] Auto-added SpriteRenderer");
    }
    
    // 콜라이더 추가 (선택)
    if (GetComponent<Collider2D>() == null)
    {
        gameObject.AddComponent<BoxCollider2D>();
    }
}
```

**Step 2: 적 종류별 색상 구분**

```csharp
public void Initialize(EnemyRow data)
{
    if (data == null)
    {
        throw new ArgumentNullException(nameof(data));
    }

    enemyData = data;
    
    // 적 종류별 색상 설정
    if (spriteRenderer != null)
    {
        switch (data.Archetype)
        {
            case EnemyArchetype.BasicMelee:
                spriteRenderer.color = Color.red;
                break;
            case EnemyArchetype.ArmoredPusher:
                spriteRenderer.color = Color.blue;
                break;
            default:
                spriteRenderer.color = Color.gray;
                break;
        }
    }
}
```

---

## Task 6: Unity Editor에서 테스트

**Step 1: Battle 씬 실행**

1. Bootstrap → Title → Lobby → Battle 실행
2. Console 로그 확인:
   - "[Battle] Spawned X enemies"
   - "[CombatDebug] Enemy spawned with sprite: XXX"

**Step 2: Scene 뷰에서 확인**

- Scene 뷰에서 빨간색 사각형/원 확인
- Game 뷰에서도 보이는지 확인

**Step 3: 문제 해결**

적이 보이지 않으면:
- SpriteRenderer의 Sorting Layer 확인
- 카메라 Orthographic Size 조정
- 적의 Z 위치 확인 (0으로 설정)

---

## Task 7: 파괴 시각 효과 확인

**Files:**
- Check: `Assets/Scripts/Combat/EnemyController.cs` - Die() 메서드
- Check: `Assets/Scripts/Combat/EnemyFragment.cs`

**Step 1: Die() 메서드 확인**

```csharp
public void Die()
{
    // 현재: Destroy(gameObject)
    // 또는: CreateFragments() 호출
    
    CreateFragments();
    Destroy(gameObject);
}
```

**Step 2: 파편 효과 테스트**

적 처치 시:
- 4개의 파편으로 분리되는지 확인
- 파편에 물리 효과 적용

---

## Acceptance Criteria

- [ ] Battle 씬에서 빨간색(또는 파란색) 사각형/원이 보임
- [ ] 적이 왼쪽에서 오른쪽으로 배치됨
- [ ] 적 처치 시 파편 효과 작동
- [ ] Console에 "Enemy spawned with sprite" 로그 출력
- [ ] Scene 뷰와 Game 뷰 모두에서 적이 보임
