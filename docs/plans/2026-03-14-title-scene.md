# Title Scene Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Title 씬을 추가하여 게임 시작 화면을 구현하고 Bootstrap → Title → Lobby 흐름을 완성

**Architecture:** Title 씬에서 게임 로고와 "게임 시작" 버튼을 표시하고, 버튼 클릭 시 Lobby로 전환합니다.

**Tech Stack:** Unity 6, uGUI, DI Container, Scene Management

---

## Task 1: TitleSceneInitializer 스크립트 작성

**Files:**
- Create: `Assets/Scripts/Core/TitleSceneInitializer.cs`

**Step 1: 스크립트 작성**

```csharp
using UnityEngine;
using UnityEngine.UI;

using TowerBreak.DI;

namespace TowerBreak.Core
{
    public sealed class TitleSceneInitializer : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        
        private void Start()
        {
            Debug.Log("[Title] Title scene initialized");
            
            // 버튼 이벤트 연결
            if (startButton != null)
            {
                startButton.onClick.AddListener(OnStartButtonClicked);
                Debug.Log("[Title] Start button connected");
            }
            else
            {
                Debug.LogWarning("[Title] Start button not assigned!");
            }
        }
        
        private void OnStartButtonClicked()
        {
            Debug.Log("[Title] Start button clicked! Loading Lobby...");
            
            // Lobby 씬 로드
            var sceneLoader = DIGlobalContext.EnsureContainer().Resolve<ISceneLoader>();
            sceneLoader.LoadScene("Lobby");
        }
        
        private void OnDestroy()
        {
            if (startButton != null)
            {
                startButton.onClick.RemoveListener(OnStartButtonClicked);
            }
        }
    }
}
```

---

## Task 2: BootstrapInstaller 수정 (Title로 로드하도록 변경)

**Files:**
- Modify: `Assets/Scripts/Core/BootstrapInstaller.cs`

**Step 1: Lobby → Title로 변경**

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

using TowerBreak.DI;
using TowerBreak.GameData.Addressables;

namespace TowerBreak.Core
{
    public sealed class BootstrapInstaller : MonoBehaviour
    {
        [SerializeField] private bool autoLoadTitleScene = true;
        
        private void Start()
        {
            Debug.Log("[Bootstrap] Starting initialization...");
            
            // DI 컨테이너 초기화
            DIGlobalContext.Reset();
            var container = DIGlobalContext.EnsureContainer();
            
            // Addressables Provider 등록
            var addressableProvider = new AddressableAssetProvider();
            container.Register<IAddressableAssetProvider>(addressableProvider);
            
            // Scene Loader 등록
            var sceneLoader = new SceneLoader();
            container.Register<ISceneLoader>(sceneLoader);
            
            Debug.Log("[Bootstrap] DI Container initialized successfully");
            
            // 초기화 완료 후 Title Scene 로드
            if (autoLoadTitleScene)
            {
                Debug.Log("[Bootstrap] Loading Title scene...");
                sceneLoader.LoadScene("Title");
            }
        }
    }
}
```

**변경사항:**
- `async void Start()` → `void Start()`
- `LoadSceneAsync("Lobby")` → `LoadScene("Title")`

---

## Task 3: Build Settings에 Title 씬 등록

**Files:**
- Modify: `ProjectSettings/EditorBuildSettings.asset`

**Step 1: Title 씬 추가**

Title 씬을 Bootstrap 다음에, Lobby 이전에 추가:

```yaml
m_Scenes:
- enabled: 1
  path: Assets/Scenes/Bootstrap.unity
  guid: 77c9fbf196fc4c0088419edf66b65452
- enabled: 1
  path: Assets/Scenes/Title.unity
  guid: [새로운 GUID]
- enabled: 1
  path: Assets/Scenes/Lobby.unity
  guid: aa5400019c00475e95edc06d5c553d59
- enabled: 1
  path: Assets/Scenes/Battle.unity
  guid: 75bf15453f5a48a38edd5ab79743694f
- enabled: 0
  path: Assets/Scenes/SampleScene.unity
  guid: 8c9cfa26abfee488c85f1582747f6a02
```

---

## Task 4: Unity Editor에서 Title 씬 구성

**Files:**
- Modify: `Assets/Scenes/Title.unity` (Unity Editor)

**Step 1: Title 씬 생성 및 설정**

1. **File → New Scene** 또는 Bootstrap 씬 복제
2. **Save As:** `Assets/Scenes/Title.unity`
3. **Hierarchy 설정:**

```
TitleScene
├── Main Camera
├── Directional Light
├── Canvas
│   ├── Background (Image - 선택)
│   ├── TitleText (Text)
│   ├── StartButton (Button)
│   │   └── ButtonText (Text)
│   └── VersionText (Text - 선택)
└── TitleSceneInitializer (GameObject + Script)
```

**Step 2: UI 요소 설정**

- **TitleText:** "TOWER BREAKER", 글꼴 크기 48, 가운데 정렬
- **StartButton:** "게임 시작", 글꼴 크기 24
- **VersionText:** "v0.1.0 MVP" (선택)

**Step 3: TitleSceneInitializer 연결**

- TitleSceneInitializer GameObject에 스크립트 추가
- StartButton 필드에 StartButton 연결

---

## Task 5: LobbySceneInitializer 수정 (B키 제거)

**Files:**
- Modify: `Assets/Scripts/Core/LobbySceneInitializer.cs`

**Step 1: 테스트용 B키 제거**

```csharp
using UnityEngine;

using TowerBreak.DI;
using TowerBreak.GameData.TowerBreaker;

namespace TowerBreak.Core
{
    public sealed class LobbySceneInitializer : MonoBehaviour
    {
        [SerializeField] private int currentFloor = 1;
        
        private void Start()
        {
            Debug.Log("[Lobby] Initializing lobby scene...");
            
            // GameData 로드
            var gameData = Resources.Load<TowerBreakerGameData>("TowerBreakerGameData");
            if (gameData == null)
            {
                Debug.LogError("[Lobby] TowerBreakerGameData not found in Resources!");
                return;
            }
            
            Debug.Log($"[Lobby] Loaded GameData: {gameData.Floors.Count} floors, {gameData.Enemies.Count} enemies, {gameData.Weapons.Count} weapons");
            
            // 현재 층 정보 표시
            if (currentFloor <= gameData.Floors.Count)
            {
                var floor = gameData.Floors[currentFloor - 1];
                Debug.Log($"[Lobby] Current Floor: {floor.FloorNumber}, Waves: {floor.WaveCount}");
            }
            
            Debug.Log("[Lobby] Initialization complete.");
        }
    }
}
```

**변경사항:** Update() 메서드 제거 (B키 테스트 코드 삭제)

---

## Task 6: 테스트 및 검증

**Step 1: 전체 흐름 테스트**

1. Unity에서 Play
2. Bootstrap → Title 로드 확인
3. "게임 시작" 버튼 클릭
4. Title → Lobby 로드 확인
5. Lobby → Battle 전환 (Battle 씬 연결되어 있다면)

**Step 2: 로그 확인**

```
[Bootstrap] Starting initialization...
[Bootstrap] DI Container initialized successfully
[Bootstrap] Loading Title scene...
[Title] Title scene initialized
[Title] Start button connected

[Title] Start button clicked! Loading Lobby...
[Lobby] Initializing lobby scene...
[Lobby] Loaded GameData: 3 floors, 2 enemies, 2 weapons
```

---

## Acceptance Criteria

- [ ] Title 씬이 존재하고 Build Settings에 등록됨
- [ ] Bootstrap → Title → Lobby 순서로 로드됨
- [ ] Title 화면에 "TOWER BREAKER" 텍스트 표시
- [ ] "게임 시작" 버튼 클릭 시 Lobby로 전환
- [ ] Lobby에서 B키 입력 제거됨
- [ ] Console에 에러 없음
