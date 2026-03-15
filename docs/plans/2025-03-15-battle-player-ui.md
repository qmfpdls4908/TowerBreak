# Battle씬 플레이어 조작 UI 및 캐릭터 구현 계획

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Battle씬에 플레이어 조작 버튼 UI(공격/가드/대시)를 추가하고, PlayerController를 개선하여 액션 상태 관리를 명확히 한다

**Architecture:** MVP 패턴으로 UI를 구현하고, EventBus로 UI와 전투 로직을 연결한다. BattleSceneInitializer의 입력 처리를 분리하여 테스트 가능한 구조로 만든다.

**Tech Stack:** Unity 6, C#, NUnit (EditMode 테스트), TowerBreak.EventBus

---

## Task 1: PlayerActionType Enum 생성

**Files:**
- Create: `Assets/Scripts/Combat/PlayerActionType.cs`

**Step 1: enum 정의**

```csharp
namespace TowerBreak.Combat
{
    public enum PlayerActionType
    {
        None,
        Attack,
        Guard,
        Dash
    }
}
```

**Step 2: 컴파일 확인**

Run: `dotnet build "TowerBreak.Combat.csproj"`
Expected: 빌드 성공

**Step 3: Commit**

```bash
git add Assets/Scripts/Combat/PlayerActionType.cs
git commit -m "feat: add PlayerActionType enum"
```

---

## Task 2: IPlayerActionHandler 인터페이스 생성

**Files:**
- Create: `Assets/Scripts/Combat/IPlayerActionHandler.cs`

**Step 1: 인터페이스 정의**

```csharp
namespace TowerBreak.Combat
{
    public interface IPlayerActionHandler
    {
        bool CanPerformAction(PlayerActionType actionType);
        void PerformAction(PlayerActionType actionType);
        PlayerActionType CurrentAction { get; }
        bool IsActionInProgress { get; }
    }
}
```

**Step 2: 컴파일 확인**

Run: `dotnet build "TowerBreak.Combat.csproj"`
Expected: 빌드 성공

**Step 3: Commit**

```bash
git add Assets/Scripts/Combat/IPlayerActionHandler.cs
git commit -m "feat: add IPlayerActionHandler interface"
```

---

## Task 3: PlayerController 개선 - IPlayerActionHandler 구현

**Files:**
- Modify: `Assets/Scripts/Combat/PlayerController.cs`

**Step 1: 기존 PlayerController 읽기**

이미 읽음 (line 1-228)

**Step 2: IPlayerActionHandler 구현 추가**

```csharp
using UnityEngine;
using TowerBreak.GameData.TowerBreaker;

namespace TowerBreak.Combat
{
    public sealed class PlayerController : MonoBehaviour, IPlayerActionHandler
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color attackColor = Color.yellow;
        [SerializeField] private Color guardColor = Color.cyan;
        [SerializeField] private Color dashColor = Color.green;
        
        [SerializeField] private Color clawColor = new Color(1f, 0.3f, 0.3f);
        [SerializeField] private Color lanceColor = new Color(0.3f, 0.5f, 1f);
        
        private float actionTimer = 0f;
        private const float ACTION_DURATION = 0.3f;
        private const float DASH_DURATION = 0.15f;
        
        private WeaponRow currentWeapon;
        private GameObject weaponObject;
        private SpriteRenderer weaponSpriteRenderer;
        
        [SerializeField] private Vector3 weaponOffset = new Vector3(0.8f, 0f, 0f);
        [SerializeField] private Vector3 clawScale = new Vector3(0.8f, 0.8f, 1f);
        [SerializeField] private Vector3 lanceScale = new Vector3(1.2f, 1.2f, 1f);
        
        public PlayerActionType CurrentAction { get; private set; } = PlayerActionType.None;
        public bool IsActionInProgress => CurrentAction != PlayerActionType.None;
        
        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = CreateDefaultSprite();
            }
            
            spriteRenderer.color = normalColor;
            transform.position = new Vector3(-6f, 0f, 0f);
            transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        }
        
        private void Update()
        {
            if (actionTimer > 0)
            {
                actionTimer -= Time.deltaTime;
                if (actionTimer <= 0)
                {
                    ResetAction();
                }
            }
        }
        
        public bool CanPerformAction(PlayerActionType actionType)
        {
            if (actionType == PlayerActionType.None)
                return false;
            
            return !IsActionInProgress;
        }
        
        public void PerformAction(PlayerActionType actionType)
        {
            if (!CanPerformAction(actionType))
                return;
            
            switch (actionType)
            {
                case PlayerActionType.Attack:
                    PerformAttack();
                    break;
                case PlayerActionType.Guard:
                    PerformGuard();
                    break;
                case PlayerActionType.Dash:
                    PerformDash();
                    break;
            }
        }
        
        private void PerformAttack()
        {
            CurrentAction = PlayerActionType.Attack;
            actionTimer = ACTION_DURATION;
            spriteRenderer.color = attackColor;
            transform.localScale = new Vector3(2f, 1.5f, 1f);
            Debug.Log("[Player] Attack!");
        }
        
        private void PerformGuard()
        {
            CurrentAction = PlayerActionType.Guard;
            actionTimer = ACTION_DURATION;
            spriteRenderer.color = guardColor;
            transform.localScale = new Vector3(1.5f, 1f, 1f);
            Debug.Log("[Player] Guard!");
        }
        
        private void PerformDash()
        {
            CurrentAction = PlayerActionType.Dash;
            actionTimer = DASH_DURATION;
            spriteRenderer.color = dashColor;
            Vector3 dashPosition = transform.position + new Vector3(2f, 0f, 0f);
            transform.position = dashPosition;
            Debug.Log("[Player] Dash!");
        }
        
        private void ResetAction()
        {
            CurrentAction = PlayerActionType.None;
            spriteRenderer.color = normalColor;
            transform.localScale = new Vector3(1.5f, 1.5f, 1f);
            
            if (weaponObject != null)
            {
                weaponObject.transform.localPosition = weaponOffset;
                weaponObject.transform.localRotation = Quaternion.identity;
            }
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
        
        public void SetWeapon(WeaponRow weapon)
        {
            if (weapon == null)
                throw new System.ArgumentNullException(nameof(weapon));
            
            currentWeapon = weapon;
            ApplyWeaponVisual(weapon);
        }
        
        public WeaponRow CurrentWeapon => currentWeapon;
        
        private void ApplyWeaponVisual(WeaponRow weapon)
        {
            if (spriteRenderer == null) return;
            CreateOrUpdateWeaponObject(weapon);
            spriteRenderer.color = normalColor;
        }
        
        private void CreateOrUpdateWeaponObject(WeaponRow weapon)
        {
            if (weaponObject == null)
            {
                weaponObject = new GameObject("Weapon");
                weaponObject.transform.SetParent(transform);
                weaponSpriteRenderer = weaponObject.AddComponent<SpriteRenderer>();
            }
            
            string spritePath = weapon.Archetype == WeaponArchetype.Claw 
                ? "Sprites/Player/claw_player" 
                : "Sprites/Player/lance_player";
            
            Sprite weaponSprite = Resources.Load<Sprite>(spritePath);
            weaponSpriteRenderer.sprite = weaponSprite != null ? weaponSprite : CreateDefaultSprite();
            
            switch (weapon.Archetype)
            {
                case WeaponArchetype.Claw:
                    weaponSpriteRenderer.color = Color.white;
                    weaponObject.transform.localScale = clawScale;
                    weaponObject.transform.localPosition = new Vector3(0.6f, 0.2f, 0f);
                    break;
                case WeaponArchetype.Lance:
                    weaponSpriteRenderer.color = Color.white;
                    weaponObject.transform.localScale = lanceScale;
                    weaponObject.transform.localPosition = new Vector3(0.8f, 0f, 0f);
                    break;
            }
            
            weaponSpriteRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
        }
        
        private void OnDestroy()
        {
            if (weaponObject != null)
            {
                Destroy(weaponObject);
            }
        }
    }
}
```

**Step 3: 컴파일 확인**

Run: `dotnet build "TowerBreak.Combat.csproj"`
Expected: 빌드 성공

**Step 4: Commit**

```bash
git add Assets/Scripts/Combat/PlayerController.cs
git commit -m "refactor: PlayerController implements IPlayerActionHandler with state management"
```

---

## Task 4: PlayerActionEvent EventBus 이벤트 생성

**Files:**
- Create: `Assets/Scripts/EventBus/PlayerActionEvent.cs`

**Step 1: 이벤트 클래스 정의**

```csharp
using TowerBreak.Combat;

namespace TowerBreak.EventBus
{
    public readonly struct PlayerActionEvent
    {
        public PlayerActionType ActionType { get; }
        
        public PlayerActionEvent(PlayerActionType actionType)
        {
            ActionType = actionType;
        }
    }
}
```

**Step 2: 컴파일 확인**

Run: `dotnet build "TowerBreak.EventBus.asmdef"` 또는 Unity에서 확인
Expected: 빌드 성공

**Step 3: Commit**

```bash
git add Assets/Scripts/EventBus/PlayerActionEvent.cs
git commit -m "feat: add PlayerActionEvent for EventBus"
```

---

## Task 5: IBattleInputView 인터페이스 생성

**Files:**
- Create: `Assets/Scripts/UIFlow/Battle/IBattleInputView.cs`

**Step 1: 인터페이스 정의**

```csharp
using System;

namespace TowerBreak.UIFlow.Battle
{
    public interface IBattleInputView
    {
        event Action OnAttackClicked;
        event Action OnGuardClicked;
        event Action OnDashClicked;
        
        void SetAttackButtonEnabled(bool enabled);
        void SetGuardButtonEnabled(bool enabled);
        void SetDashButtonEnabled(bool enabled);
    }
}
```

**Step 2: 컴파일 확인**

Run: `dotnet build "TowerBreak.UIFlow.csproj"` 또는 Unity에서 확인
Expected: 빌드 성공

**Step 3: Commit**

```bash
git add Assets/Scripts/UIFlow/Battle/IBattleInputView.cs
git commit -m "feat: add IBattleInputView interface for battle controls"
```

---

## Task 6: BattleInputPresenter 생성

**Files:**
- Create: `Assets/Scripts/UIFlow/Battle/BattleInputPresenter.cs`

**Step 1: Presenter 구현**

```csharp
using System;
using TowerBreak.Combat;
using TowerBreak.EventBus;

namespace TowerBreak.UIFlow.Battle
{
    public sealed class BattleInputPresenter : IDisposable
    {
        private readonly IBattleInputView _view;
        private readonly IPlayerActionHandler _playerActionHandler;
        
        public BattleInputPresenter(
            IBattleInputView view,
            IPlayerActionHandler playerActionHandler)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _playerActionHandler = playerActionHandler ?? throw new ArgumentNullException(nameof(playerActionHandler));
            
            _view.OnAttackClicked += HandleAttackClicked;
            _view.OnGuardClicked += HandleGuardClicked;
            _view.OnDashClicked += HandleDashClicked;
            
            UpdateButtonStates();
        }
        
        public void Update()
        {
            UpdateButtonStates();
        }
        
        private void UpdateButtonStates()
        {
            _view.SetAttackButtonEnabled(_playerActionHandler.CanPerformAction(PlayerActionType.Attack));
            _view.SetGuardButtonEnabled(_playerActionHandler.CanPerformAction(PlayerActionType.Guard));
            _view.SetDashButtonEnabled(_playerActionHandler.CanPerformAction(PlayerActionType.Dash));
        }
        
        private void HandleAttackClicked()
        {
            PublishAction(PlayerActionType.Attack);
        }
        
        private void HandleGuardClicked()
        {
            PublishAction(PlayerActionType.Guard);
        }
        
        private void HandleDashClicked()
        {
            PublishAction(PlayerActionType.Dash);
        }
        
        private void PublishAction(PlayerActionType actionType)
        {
            if (_playerActionHandler.CanPerformAction(actionType))
            {
                EventBus<PlayerActionEvent>.Publish(new PlayerActionEvent(actionType));
                _playerActionHandler.PerformAction(actionType);
                UpdateButtonStates();
            }
        }
        
        public void Dispose()
        {
            _view.OnAttackClicked -= HandleAttackClicked;
            _view.OnGuardClicked -= HandleGuardClicked;
            _view.OnDashClicked -= HandleDashClicked;
        }
    }
}
```

**Step 2: 컴파일 확인**

Run: `dotnet build "TowerBreak.UIFlow.csproj"`
Expected: 빌드 성공

**Step 3: Commit**

```bash
git add Assets/Scripts/UIFlow/Battle/BattleInputPresenter.cs
git commit -m "feat: add BattleInputPresenter for handling battle input logic"
```

---

## Task 7: BattleInputView (MonoBehaviour) 생성

**Files:**
- Create: `Assets/Scripts/UIFlow/Battle/BattleInputView.cs`

**Step 1: View 구현**

```csharp
using System;
using UnityEngine;
using UnityEngine.UI;

namespace TowerBreak.UIFlow.Battle
{
    public sealed class BattleInputView : MonoBehaviour, IBattleInputView
    {
        [SerializeField] private Button attackButton;
        [SerializeField] private Button guardButton;
        [SerializeField] private Button dashButton;
        
        public event Action OnAttackClicked;
        public event Action OnGuardClicked;
        public event Action OnDashClicked;
        
        private void Awake()
        {
            if (attackButton != null)
                attackButton.onClick.AddListener(() => OnAttackClicked?.Invoke());
            
            if (guardButton != null)
                guardButton.onClick.AddListener(() => OnGuardClicked?.Invoke());
            
            if (dashButton != null)
                dashButton.onClick.AddListener(() => OnDashClicked?.Invoke());
        }
        
        public void SetAttackButtonEnabled(bool enabled)
        {
            if (attackButton != null)
                attackButton.interactable = enabled;
        }
        
        public void SetGuardButtonEnabled(bool enabled)
        {
            if (guardButton != null)
                guardButton.interactable = enabled;
        }
        
        public void SetDashButtonEnabled(bool enabled)
        {
            if (dashButton != null)
                dashButton.interactable = enabled;
        }
        
        private void OnDestroy()
        {
            if (attackButton != null)
                attackButton.onClick.RemoveAllListeners();
            
            if (guardButton != null)
                guardButton.onClick.RemoveAllListeners();
            
            if (dashButton != null)
                dashButton.onClick.RemoveAllListeners();
        }
    }
}
```

**Step 2: 컴파일 확인**

Run: Unity에서 컴파일 확인 (TowerBreak.UIFlow.asmdef)
Expected: 빌드 성공

**Step 3: Commit**

```bash
git add Assets/Scripts/UIFlow/Battle/BattleInputView.cs
git commit -m "feat: add BattleInputView MonoBehaviour for battle control buttons"
```

---

## Task 8: BattleSceneInitializer에 InputPresenter 연동

**Files:**
- Modify: `Assets/Scripts/Combat/BattleSceneInitializer.cs`

**Step 1: 기존 코드 분석**

Line 454-481: HandlePlayerInput() 메서드가 키보드 입력 처리

**Step 2: 수정 사항**

```csharp
// 필드 추가 (line 35 이후)
private BattleInputPresenter inputPresenter;

// Start() 메서드 수정 (line 264 이후, CreatePlayer 호출 후)
// CreatePlayer 호출 후에 InputPresenter 초기화
```

**Step 3: Start() 메서드에 InputPresenter 초기화 추가**

Line 264-266 사이에 추가:
```csharp
// 플레이어 생성 (무기 정보 전달)
CreatePlayer(equippedWeapon);

// BattleInputView 연동
InitializeInputPresenter();

Debug.Log("[Battle] Battle ready!");
```

**Step 4: InitializeInputPresenter() 메서드 추가**

Line 437 이전에 추가:
```csharp
private void InitializeInputPresenter()
{
    var inputView = FindFirstObjectByType<BattleInputView>();
    if (inputView != null && playerController != null)
    {
        inputPresenter = new BattleInputPresenter(inputView, playerController);
        Debug.Log("[Battle] Input presenter initialized");
    }
    else
    {
        Debug.LogWarning("[Battle] BattleInputView or PlayerController not found, using keyboard input only");
    }
}
```

**Step 5: Update() 메서드 수정**

Line 437 HandlePlayerInput() 호출 부분 수정:
```csharp
// 플레이어 입력 처리
HandlePlayerInput();

// InputPresenter 업데이트 (버튼 상태 갱신)
inputPresenter?.Update();
```

**Step 6: OnDestroy() 메서드 추가**

파일 마지막에 추가:
```csharp
private void OnDestroy()
{
    inputPresenter?.Dispose();
}
```

**Step 7: using 구문 추가**

파일 상단에 추가:
```csharp
using TowerBreak.UIFlow.Battle;
```

**Step 8: 컴파일 확인**

Run: Unity에서 컴파일 확인
Expected: 빌드 성공

**Step 9: Commit**

```bash
git add Assets/Scripts/Combat/BattleSceneInitializer.cs
git commit -m "feat: integrate BattleInputPresenter into BattleSceneInitializer"
```

---

## Task 9: PlayerController 테스트 작성

**Files:**
- Create: `Assets/Tests/EditMode/Combat/PlayerControllerTests.cs`

**Step 1: 테스트 클래스 작성**

```csharp
using NUnit.Framework;
using TowerBreak.Combat;

namespace TowerBreak.Combat.Tests
{
    public sealed class PlayerControllerTests
    {
        private class MockPlayerActionHandler : IPlayerActionHandler
        {
            public PlayerActionType CurrentAction { get; private set; } = PlayerActionType.None;
            public bool IsActionInProgress => CurrentAction != PlayerActionType.None;
            
            public bool CanPerformAction(PlayerActionType actionType)
            {
                return actionType != PlayerActionType.None && !IsActionInProgress;
            }
            
            public void PerformAction(PlayerActionType actionType)
            {
                if (CanPerformAction(actionType))
                {
                    CurrentAction = actionType;
                }
            }
            
            public void ResetAction()
            {
                CurrentAction = PlayerActionType.None;
            }
        }
        
        [Test]
        public void CanPerformAction_IdleState_ReturnsTrue()
        {
            var handler = new MockPlayerActionHandler();
            
            Assert.IsTrue(handler.CanPerformAction(PlayerActionType.Attack));
            Assert.IsTrue(handler.CanPerformAction(PlayerActionType.Guard));
            Assert.IsTrue(handler.CanPerformAction(PlayerActionType.Dash));
        }
        
        [Test]
        public void CanPerformAction_ActionInProgress_ReturnsFalse()
        {
            var handler = new MockPlayerActionHandler();
            handler.PerformAction(PlayerActionType.Attack);
            
            Assert.IsFalse(handler.CanPerformAction(PlayerActionType.Attack));
            Assert.IsFalse(handler.CanPerformAction(PlayerActionType.Guard));
            Assert.IsFalse(handler.CanPerformAction(PlayerActionType.Dash));
        }
        
        [Test]
        public void CanPerformAction_NoneAction_ReturnsFalse()
        {
            var handler = new MockPlayerActionHandler();
            
            Assert.IsFalse(handler.CanPerformAction(PlayerActionType.None));
        }
        
        [Test]
        public void PerformAction_ValidAction_SetsCurrentAction()
        {
            var handler = new MockPlayerActionHandler();
            
            handler.PerformAction(PlayerActionType.Attack);
            
            Assert.AreEqual(PlayerActionType.Attack, handler.CurrentAction);
            Assert.IsTrue(handler.IsActionInProgress);
        }
        
        [Test]
        public void PerformAction_WhileActionInProgress_DoesNotChange()
        {
            var handler = new MockPlayerActionHandler();
            handler.PerformAction(PlayerActionType.Attack);
            
            handler.PerformAction(PlayerActionType.Guard);
            
            Assert.AreEqual(PlayerActionType.Attack, handler.CurrentAction);
        }
        
        [Test]
        public void CurrentAction_InitialState_IsNone()
        {
            var handler = new MockPlayerActionHandler();
            
            Assert.AreEqual(PlayerActionType.None, handler.CurrentAction);
            Assert.IsFalse(handler.IsActionInProgress);
        }
    }
}
```

**Step 2: asmdef 확인**

`TowerBreak.Combat.Tests.asmdef`에 `TowerBreak.Combat` 참조 확인

**Step 3: 테스트 실행**

Unity Test Runner에서 실행
Expected: 모든 테스트 PASS

**Step 4: Commit**

```bash
git add Assets/Tests/EditMode/Combat/PlayerControllerTests.cs
git commit -m "test: add PlayerController action state tests"
```

---

## Task 10: BattleInputPresenter 테스트 작성

**Files:**
- Create: `Assets/Tests/EditMode/UIFlow/BattleInputPresenterTests.cs`

**Step 1: 테스트 클래스 작성**

```csharp
using NUnit.Framework;
using System;
using TowerBreak.Combat;
using TowerBreak.EventBus;
using TowerBreak.UIFlow.Battle;

namespace TowerBreak.UIFlow.Tests
{
    public sealed class BattleInputPresenterTests
    {
        private class MockBattleInputView : IBattleInputView
        {
            public event Action OnAttackClicked;
            public event Action OnGuardClicked;
            public event Action OnDashClicked;
            
            public bool AttackButtonEnabled { get; private set; }
            public bool GuardButtonEnabled { get; private set; }
            public bool DashButtonEnabled { get; private set; }
            
            public void SetAttackButtonEnabled(bool enabled)
            {
                AttackButtonEnabled = enabled;
            }
            
            public void SetGuardButtonEnabled(bool enabled)
            {
                GuardButtonEnabled = enabled;
            }
            
            public void SetDashButtonEnabled(bool enabled)
            {
                DashButtonEnabled = enabled;
            }
            
            public void SimulateAttackClick() => OnAttackClicked?.Invoke();
            public void SimulateGuardClick() => OnGuardClicked?.Invoke();
            public void SimulateDashClick() => OnDashClicked?.Invoke();
        }
        
        private class MockPlayerActionHandler : IPlayerActionHandler
        {
            public PlayerActionType CurrentAction { get; set; } = PlayerActionType.None;
            public bool IsActionInProgress => CurrentAction != PlayerActionType.None;
            
            public bool CanPerformAction(PlayerActionType actionType)
            {
                return actionType != PlayerActionType.None && !IsActionInProgress;
            }
            
            public void PerformAction(PlayerActionType actionType)
            {
                if (CanPerformAction(actionType))
                {
                    CurrentAction = actionType;
                }
            }
        }
        
        private MockBattleInputView mockView;
        private MockPlayerActionHandler mockHandler;
        private BattleInputPresenter presenter;
        private PlayerActionEvent lastEvent;
        private bool eventReceived;
        
        [SetUp]
        public void Setup()
        {
            mockView = new MockBattleInputView();
            mockHandler = new MockPlayerActionHandler();
            presenter = new BattleInputPresenter(mockView, mockHandler);
            
            lastEvent = default;
            eventReceived = false;
            EventBus<PlayerActionEvent>.Subscribe(OnPlayerAction);
        }
        
        [TearDown]
        public void TearDown()
        {
            EventBus<PlayerActionEvent>.Unsubscribe(OnPlayerAction);
            presenter?.Dispose();
            EventBus<PlayerActionEvent>.ClearAllSubscribers();
        }
        
        private void OnPlayerAction(PlayerActionEvent evt)
        {
            lastEvent = evt;
            eventReceived = true;
        }
        
        [Test]
        public void Constructor_InitializesButtonStates()
        {
            Assert.IsTrue(mockView.AttackButtonEnabled);
            Assert.IsTrue(mockView.GuardButtonEnabled);
            Assert.IsTrue(mockView.DashButtonEnabled);
        }
        
        [Test]
        public void OnAttackClicked_PublishesEvent()
        {
            mockView.SimulateAttackClick();
            
            Assert.IsTrue(eventReceived);
            Assert.AreEqual(PlayerActionType.Attack, lastEvent.ActionType);
        }
        
        [Test]
        public void OnGuardClicked_PublishesEvent()
        {
            mockView.SimulateGuardClick();
            
            Assert.IsTrue(eventReceived);
            Assert.AreEqual(PlayerActionType.Guard, lastEvent.ActionType);
        }
        
        [Test]
        public void OnDashClicked_PublishesEvent()
        {
            mockView.SimulateDashClick();
            
            Assert.IsTrue(eventReceived);
            Assert.AreEqual(PlayerActionType.Dash, lastEvent.ActionType);
        }
        
        [Test]
        public void Update_WhenActionInProgress_DisablesButtons()
        {
            mockHandler.CurrentAction = PlayerActionType.Attack;
            
            presenter.Update();
            
            Assert.IsFalse(mockView.AttackButtonEnabled);
            Assert.IsFalse(mockView.GuardButtonEnabled);
            Assert.IsFalse(mockView.DashButtonEnabled);
        }
        
        [Test]
        public void OnAttackClicked_WhenActionInProgress_DoesNotPublish()
        {
            mockHandler.CurrentAction = PlayerActionType.Guard;
            
            mockView.SimulateAttackClick();
            
            Assert.IsFalse(eventReceived);
        }
        
        [Test]
        public void Dispose_UnsubscribesFromEvents()
        {
            presenter.Dispose();
            
            mockView.SimulateAttackClick();
            
            Assert.IsFalse(eventReceived);
        }
        
        [Test]
        public void Constructor_NullView_ThrowsArgumentNullException()
        {
            Assert.Throws<System.ArgumentNullException>(() =>
            {
                new BattleInputPresenter(null, mockHandler);
            });
        }
        
        [Test]
        public void Constructor_NullHandler_ThrowsArgumentNullException()
        {
            Assert.Throws<System.ArgumentNullException>(() =>
            {
                new BattleInputPresenter(mockView, null);
            });
        }
    }
}
```

**Step 2: 테스트 실행**

Unity Test Runner에서 실행
Expected: 모든 테스트 PASS

**Step 3: Commit**

```bash
git add Assets/Tests/EditMode/UIFlow/BattleInputPresenterTests.cs
git commit -m "test: add BattleInputPresenter tests with EventBus verification"
```

---

## Task 11: 빌드 및 최종 검증

**Step 1: 전체 빌드**

Run: `dotnet build "TowerBreak.sln"`
Expected: 모든 프로젝트 빌드 성공

**Step 2: Unity 컴파일 확인**

Unity Editor에서 컴파일 오류 확인
Expected: 오류 없음

**Step 3: Commit**

```bash
git add .
git commit -m "feat: complete battle player UI and character implementation"
```

---

## 테스트 방법

### 1. EditMode 테스트 실행

Unity Test Runner에서 다음 테스트 실행:
- `PlayerControllerTests` (5개 테스트)
- `BattleInputPresenterTests` (8개 테스트)

### 2. Battle씬 통합 테스트

1. Unity에서 Battle씬 열기
2. Canvas에 BattleInputView 컴포넌트 추가
3. 공격/가드/대시 버튼 3개 생성하여 연결
4. Play Mode로 실행
5. 버튼 클릭 시 플레이어 액션 실행 확인

### 3. CLI 테스트 명령어

```bash
# 모든 EditMode 테스트 실행
"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -quit -projectPath "D:\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.Combat.Tests.PlayerControllerTests" -logFile "Logs\player-tests.log" -testResults "Logs\player-tests.xml"

# BattleInputPresenter 테스트
"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -quit -projectPath "D:\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.UIFlow.Tests.BattleInputPresenterTests" -logFile "Logs\input-tests.log" -testResults "Logs\input-tests.xml"
```

---

## 구현 요약

**생성된 파일:**
1. `Assets/Scripts/Combat/PlayerActionType.cs`
2. `Assets/Scripts/Combat/IPlayerActionHandler.cs`
3. `Assets/Scripts/EventBus/PlayerActionEvent.cs`
4. `Assets/Scripts/UIFlow/Battle/IBattleInputView.cs`
5. `Assets/Scripts/UIFlow/Battle/BattleInputPresenter.cs`
6. `Assets/Scripts/UIFlow/Battle/BattleInputView.cs`
7. `Assets/Tests/EditMode/Combat/PlayerControllerTests.cs`
8. `Assets/Tests/EditMode/UIFlow/BattleInputPresenterTests.cs`

**수정된 파일:**
1. `Assets/Scripts/Combat/PlayerController.cs` - IPlayerActionHandler 구현
2. `Assets/Scripts/Combat/BattleSceneInitializer.cs` - InputPresenter 연동

**아키텍처:**
- **MVP 패턴**: View(IBattleInputView) → Presenter(BattleInputPresenter)
- **EventBus**: UI 액션 → 전투 로직 연결
- **인터페이스 분리**: IPlayerActionHandler로 테스트 가능한 구조
