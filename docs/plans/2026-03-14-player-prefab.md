# Player Prefab Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 플레이어를 프리팹으로 변경하여 에디터에서도 볼 수 있게 하고, BattleSceneInitializer에서 프리팹을 인스턴스화하여 사용

**Architecture:** Player 프리팹을 Assets/Prefabs/Combat/에 저장하고, BattleSceneInitializer에서 Resources.Load 또는 Addressables로 로드하여 생성합니다.

**Tech Stack:** Unity 6, Prefab System, Resources.Load

---

## Task 1: Player 프리팹 폴더 확인 및 생성

**Files:**
- Check: `Assets/Prefabs/Combat/` 폴더
- Create: `Assets/Prefabs/Combat/Player.prefab`

**Step 1: 폴더 확인**

```bash
# 폴더 존재 확인
ls Assets/Prefabs/Combat/
```

**Step 2: Unity Editor에서 Player 프리팹 생성**

Hierarchy:
```
1. GameObject → Create Empty
2. 이름 변경: Player
3. Inspector 설정:
   ├── SpriteRenderer 추가
   │   ├── Sprite: 원하는 스프라이트 (또는 Square)
   │   ├── Color: 흰색 (FFFFFF)
   │   └── Sorting Layer: Default
   ├── PlayerController 스크립트 추가
   └── Transform:
       ├── Position: (0, 0, 0)
       ├── Scale: (1.5, 1.5, 1)
       └── Rotation: (0, 0, 0)
4. Project 창의 Assets/Prefabs/Combat/로 드래그
5. Hierarchy에서 원본 삭제 (프리팹은 Project에 남음)
```

**Step 3: Resources 폴더에 복사 (선택)**

프리팹을 Resources 폴더에 넣어야 Resources.Load로 로드 가능:
```
Assets/Resources/Prefabs/Combat/Player.prefab
```

또는 BattleSceneInitializer를 Addressables로 로드하도록 수정

---

## Task 2: BattleSceneInitializer 수정

**Files:**
- Modify: `Assets/Scripts/Combat/BattleSceneInitializer.cs`

**Step 1: 프리팹 경로 필드 추가**

```csharp
public sealed class BattleSceneInitializer : MonoBehaviour
{
    [SerializeField] private int currentFloor = 1;
    [SerializeField] private Transform enemySpawnParent;
    [SerializeField] private string playerPrefabPath = "Prefabs/Combat/Player";  // 추가
    
    // ... 기존 필드 ...
}
```

**Step 2: CreatePlayer 메서드 수정**

```csharp
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
```

**Step 3: 불필요한 메서드 제거**

```csharp
// CreatePlayerSprite 메서드 제거 (이제 프리팹에서 스프라이트 설정)
```

---

## Task 3: PlayerController 수정 (스프라이트 자동 설정 제거)

**Files:**
- Modify: `Assets/Scripts/Combat/PlayerController.cs`

**Step 1: Awake 메서드 수정**

```csharp
private void Awake()
{
    // SpriteRenderer 확인
    spriteRenderer = GetComponent<SpriteRenderer>();
    if (spriteRenderer == null)
    {
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        Debug.Log("[PlayerController] Added SpriteRenderer");
    }
    
    // 프리팹에서 스프라이트가 설정되어 있으면 그대로 사용
    // 없으면 기본 흰색 설정
    if (spriteRenderer.sprite == null)
    {
        spriteRenderer.sprite = CreateDefaultSprite();
        Debug.Log("[PlayerController] Created default sprite");
    }
    
    // 기본 색상 설정 (프리팹에서 설정된 색상 유지 또는 흰색)
    if (spriteRenderer.color == Color.clear)
    {
        spriteRenderer.color = normalColor;
    }
    
    // 위치와 스케일 설정 (프리팹 값 무시)
    transform.localScale = new Vector3(1.5f, 1.5f, 1f);
}
```

---

## Task 4: Unity Editor 설정

**Files:**
- Modify: `Assets/Scenes/Battle.unity` (Unity Editor)

**Step 1: Player 프리팹 스프라이트 설정**

1. Project 창 → `Assets/Prefabs/Combat/Player.prefab` 선택
2. Inspector:
   - SpriteRenderer → Sprite 필드에 원하는 스프라이트 드래그
   - 없으면 2D Sprites → UI → Square 또는 원형 스프라이트 사용
   - Color: 흰색 또는 원하는 색상

**Step 2: Resources 폴더 설정 (선택)**

Resources.Load를 사용하려면:
```
Assets/Resources/Prefabs/Combat/Player.prefab
```

폴더 구조:
1. `Assets/Resources/` 폴더 생성 (없으면)
2. `Assets/Resources/Prefabs/` 폴더 생성
3. `Assets/Resources/Prefabs/Combat/` 폴더 생성
4. Player.prefab을 여기로 복사 또는 이동

**또는 Addressables 사용:**
- Player.prefab을 Addressables에 등록
- BattleSceneInitializer에서 Addressables로 로드

**Step 3: BattleSceneInitializer Inspector 설정**

BattleManager 오브젝트 선택:
- `Player Prefab Path` 필드에 "Prefabs/Combat/Player" 입력
- (Resources 폴더 기준)

---

## Task 5: 테스트 및 검증

**Step 1: Battle 씬 테스트**

1. Unity Editor에서 Battle 씬 열기
2. Hierarchy에 Player 오브젝트가 없는지 확인 (실행 전)
3. Play 모드 실행
4. Console 로그 확인:
   ```
   [Battle] Creating player from prefab...
   [Battle] Player created from prefab at position: (-6.0, 0.0, 0.0)
   ```
5. Scene 뷰에서 플레이어 확인 (흰색 사각형 또는 설정한 스프라이트)

**Step 2: 프리팹 미리보기 확인**

Project 창에서 Player.prefab 선택:
- Inspector 하단의 Preview 창에서 스프라이트 확인
- 또는 더블클릭해서 Prefab Mode로 열기

**Step 3: 프리팹 수정 테스트**

1. Project 창에서 Player.prefab 더블클릭
2. SpriteRenderer 색상 변경 (예: 파란색)
3. 저장
4. 다시 Play → 변경된 색상으로 플레이어 생성 확인

---

## Task 6: 개선사항 (선택)

**1. 여러 캐릭터 스프라이트 지원**

```csharp
[SerializeField] private Sprite[] playerSprites;  // 다양한 캐릭터
[SerializeField] private int selectedCharacter = 0;

private void CreatePlayer()
{
    // ...
    if (playerSprites.Length > selectedCharacter)
    {
        playerController.SetSprite(playerSprites[selectedCharacter]);
    }
}
```

**2. 애니메이션 추가**

Player.prefab에 Animator 컴포넌트 추가:
- Idle, Attack, Guard, Dash 애니메이션 클립
- Animator Controller 생성 및 연결

**3. 무기 스프라이트 추가**

Player 하위에 무기 GameObject:
```
Player
├── SpriteRenderer (몸통)
└── Weapon (GameObject)
    └── SpriteRenderer (무기)
```

---

## Acceptance Criteria

- [ ] `Assets/Resources/Prefabs/Combat/Player.prefab` 존재
- [ ] Player 프리팹에 SpriteRenderer + PlayerController 컴포넌트
- [ ] 프리팹에 스프라이트 설정됨 (Square 또는 원하는 스프라이트)
- [ ] Battle 실행 시 "Creating player from prefab" 로그 출력
- [ ] Scene 뷰에서 플레이어가 설정된 스프라이트로 표시됨
- [ ] Space/G/LeftShift 입력 시 플레이어 색상 변화
- [ ] 프리팹 수정 후 다시 실행하면 변경사항 적용됨
