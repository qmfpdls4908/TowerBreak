# Player Character Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Battle 씬에 플레이어 캐릭터를 추가하여 시각적 표현과 기본 동작(이동, 공격, 가드) 구현

**Architecture:** PlayerController를 생성하여 화면 하단에 배치하고, 키 입력에 따라 공격/가드/대시 애니메이션을 실행합니다. 플레이어는 왼쪽 벽을 지키는 위치에 고정됩니다.

**Tech Stack:** Unity 6, SpriteRenderer, Input System, 2D URP

---

## Task 1: PlayerController 스크립트 작성

**Files:**
- Create: `Assets/Scripts/Combat/PlayerController.cs`

**Step 1: 스크립트 작성**

```csharp
using UnityEngine;

namespace TowerBreak.Combat
{
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color attackColor = Color.yellow;
        [SerializeField] private Color guardColor = Color.cyan;
        [SerializeField] private Color dashColor = Color.green;
        
        private bool isAttacking = false;
        private bool isGuarding = false;
        private bool isDashing = false;
        private float actionTimer = 0f;
        private const float ACTION_DURATION = 0.3f;
        
        private void Awake()
        {
            // SpriteRenderer 확인/추가
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = CreateDefaultSprite();
                Debug.Log("[PlayerController] Added SpriteRenderer");
            }
            
            // 기본 색상 설정
            spriteRenderer.color = normalColor;
            
            // 플레이어 위치 설정 (왼쪽 벽 근처)
            transform.position = new Vector3(-6f, 0f, 0f);
            transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        }
        
        private void Update()
        {
            // 액션 타이머 업데이트
            if (actionTimer > 0)
            {
                actionTimer -= Time.deltaTime;
                if (actionTimer <= 0)
                {
                    ResetAction();
                }
            }
        }
        
        public void PerformAttack()
        {
            if (isAttacking || isGuarding || isDashing) return;
            
            isAttacking = true;
            actionTimer = ACTION_DURATION;
            spriteRenderer.color = attackColor;
            
            // 공격 애니메이션 (scale 효과)
            transform.localScale = new Vector3(2f, 1.5f, 1f);
            
            Debug.Log("[Player] Attack!");
        }
        
        public void PerformGuard()
        {
            if (isAttacking || isGuarding || isDashing) return;
            
            isGuarding = true;
            actionTimer = ACTION_DURATION;
            spriteRenderer.color = guardColor;
            
            // 가드 자세 (y scale 줄임)
            transform.localScale = new Vector3(1.5f, 1f, 1f);
            
            Debug.Log("[Player] Guard!");
        }
        
        public void PerformDash()
        {
            if (isAttacking || isGuarding || isDashing) return;
            
            isDashing = true;
            actionTimer = ACTION_DURATION * 0.5f;
            spriteRenderer.color = dashColor;
            
            // 대시 이동
            Vector3 dashPosition = transform.position + new Vector3(2f, 0f, 0f);
            transform.position = dashPosition;
            
            Debug.Log("[Player] Dash!");
        }
        
        private void ResetAction()
        {
            isAttacking = false;
            isGuarding = false;
            isDashing = false;
            
            spriteRenderer.color = normalColor;
            transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        }
        
        private Sprite CreateDefaultSprite()
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
        
        public bool IsAttacking => isAttacking;
        public bool IsGuarding => isGuarding;
        public bool IsDashing => isDashing;
    }
}
```

---

## Task 2: BattleSceneInitializer에 플레이어 생성

**Files:**
- Modify: `Assets/Scripts/Combat/BattleSceneInitializer.cs`

**Step 1: 플레이어 생성 추가**

```csharp
public sealed class BattleSceneInitializer : MonoBehaviour
{
    [SerializeField] private int currentFloor = 1;
    [SerializeField] private Transform enemySpawnParent;
    [SerializeField] private Transform playerSpawnPosition;  // 추가
    
    private BattleLoopController battleLoopController;
    private List<GameObject> spawnedEnemies = new();
    private PlayerController playerController;  // 추가
    private IAddressableAssetProvider provider;
    private PooledCombatInstantiator pooled;
    // ...
    
    private async void Start()
    {
        // ... 기존 코드 ...
        
        // 플레이어 생성
        CreatePlayer();
        
        // ... 기존 코드 ...
    }
    
    private void CreatePlayer()
    {
        Debug.Log("[Battle] Creating player...");
        
        GameObject playerObject = new GameObject("Player");
        playerObject.transform.SetParent(transform);
        
        // 위치 설정
        if (playerSpawnPosition != null)
        {
            playerObject.transform.position = playerSpawnPosition.position;
        }
        else
        {
            playerObject.transform.position = new Vector3(-6f, 0f, 0f);  // 왼쪽 벽 근처
        }
        
        // SpriteRenderer 추가
        var spriteRenderer = playerObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CreatePlayerSprite();
        spriteRenderer.color = Color.white;
        
        // PlayerController 추가
        playerController = playerObject.AddComponent<PlayerController>();
        
        Debug.Log("[Battle] Player created at position: " + playerObject.transform.position);
    }
    
    private Sprite CreatePlayerSprite()
    {
        // 흰색 사각형 스프라이트 생성
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
}
```

---

## Task 3: 입력 연결 (공격/가드/대시)

**Files:**
- Modify: `Assets/Scripts/Combat/BattleSceneInitializer.cs`

**Step 1: Update 메서드 수정**

```csharp
private void Update()
{
    if (battleLoopController == null || wallDefeated) return;
    
    // ... 기존 코드 ...
    
    // 플레이어 입력 처리
    HandlePlayerInput();
    
    // ... 기존 코드 ...
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
        // 전투 로직 호출
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
```

---

## Task 4: Unity Editor 설정

**Files:**
- Modify: `Assets/Scenes/Battle.unity` (Unity Editor)

**Step 1: Battle 씬에 PlayerSpawnPosition 추가**

Hierarchy:
```
BattleScene
├── Main Camera
├── Directional Light
├── Canvas (UI)
├── BattleManager (BattleSceneInitializer)
│   └── PlayerSpawnPosition (Empty GameObject)  // 추가
│       └── Position: (-6, 0, 0)
└── Enemies (Parent)
```

**Step 2: BattleSceneInitializer Inspector 설정**

- `Player Spawn Position` 필드에 PlayerSpawnPosition 연결

---

## Task 5: 테스트 및 검증

**Step 1: Battle 씬 테스트**

1. Bootstrap → Title → Lobby → Battle 실행
2. 확인할 것:
   - 왼쪽에 흰색 사각형(플레이어) 보임
   - Space: 노란색으로 변하며 공격
   - G: 파란색으로 변하며 가드
   - Left Shift: 초록색으로 변하며 대시

**Step 2: 로그 확인**

```
[Battle] Creating player...
[Battle] Player created at position: (-6.0, 0.0, 0.0)
[Player] Attack!
[Player] Guard!
[Player] Dash!
```

---

## Task 6: 개선사항 (선택)

**1. 플레이어와 적 간격 조정**
- 현재: 플레이어 (-6, 0), 적 (중앙)
- 적 위치를 오른쪽으로 조정하거나 플레이어 위치 조정

**2. 애니메이션 추가**
- DOTween 또는 Unity Animation 사용
- 부드러운 이동/색상 변화

**3. 무기 시각화**
- 공격 시 무기 스프라이트 표시
- 무기별 다른 색상/모양

---

## Acceptance Criteria

- [ ] Battle 씬에 플레이어(흰색 사각형)가 왼쪽에 표시됨
- [ ] Space 키로 공격 시 노란색으로 변함
- [ ] G 키로 가드 시 파란색으로 변함
- [ ] Left Shift로 대시 시 초록색으로 변함 + 앞으로 이동
- [ ] 액션 종료 후 원래 색상으로 돌아옴
- [ ] Console에 "[Player] Attack/Guard/Dash!" 로그 출력
