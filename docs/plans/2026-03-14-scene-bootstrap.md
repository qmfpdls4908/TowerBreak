# Tower Breaker Scene Bootstrap Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Unity에서 게임이 실제로 실행되도록 Bootstrap 씬을 구성하고 Build Settings를 설정하여 Title → Lobby → Battle 흐름이 작동하게 함

**Architecture:** Bootstrap 씬에서 DI 초기화와 SceneLoader를 설정하여 앱의 진입점을 만듭니다. Build Settings에 필요한 씬을 등록하고 GameData 리소스가 올바르게 로드되도록 검증합니다.

**Tech Stack:** Unity 6, C#, Addressables, DI Container, Scene Management

---

## Task 1: 현재 상태 검증

**Files:**
- Check: `Assets/Scenes/Bootstrap.unity`
- Check: `ProjectSettings/EditorBuildSettings.asset`
- Check: `Assets/Scripts/Core/SceneLoader.cs`
- Check: `Assets/Scripts/DI/DIGlobalInstaller.cs`

**Step 1: Bootstrap 씬 내용 확인**

명령: Unity 씬 파일에서 GameObject 목록 확인
```bash
# Bootstrap.unity에서 GameObject 확인
grep -n "m_Name:" Assets/Scenes/Bootstrap.unity | head -20
```

기대 결과: GameObject가 없거나 최소한의 오브젝트만 있음

**Step 2: Build Settings 확인**

파일 읽기: `ProjectSettings/EditorBuildSettings.asset`

기대 결과: Scenes 목록이 비어있거나 SampleScene만 있음

**Step 3: Core 시스템 파일 확인**

파일 읽기:
- `Assets/Scripts/Core/SceneLoader.cs`
- `Assets/Scripts/DI/DIGlobalInstaller.cs`
- `Assets/Scripts/Core/Router/TitleFlowRouter.cs`

---

## Task 2: Build Settings에 씬 등록

**Files:**
- Modify: `ProjectSettings/EditorBuildSettings.asset`

**Step 1: Build Settings 백업**

**Step 2: 씬 목록 추가**

EditorBuildSettings.asset 수정:
```yaml
m_Scenes:
- enabled: 1
  path: Assets/Scenes/Bootstrap.unity
  guid: [새로 생성된 GUID]
- enabled: 1
  path: Assets/Scenes/Lobby.unity
  guid: [새로 생성된 GUID]
- enabled: 1
  path: Assets/Scenes/Battle.unity
  guid: [새로 생성된 GUID]
- enabled: 0
  path: Assets/Scenes/SampleScene.unity
  guid: 8c9cfa26abfee488c85f1582747f6a02
```

**Step 3: Unity에서 확인**

File → Build Settings → Scenes In Build 확인

---

## Task 3: Bootstrap 씬에 DI Installer 추가

**Files:**
- Create: `Assets/Scripts/Core/BootstrapInstaller.cs`
- Modify: `Assets/Scenes/Bootstrap.unity` (Unity Editor에서)

**Step 1: BootstrapInstaller 스크립트 작성**

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

using TowerBreak.DI;
using TowerBreak.GameData.Addressables;

namespace TowerBreak.Core
{
    public sealed class BootstrapInstaller : MonoBehaviour
    {
        [SerializeField] private bool autoLoadTitleScene = true;
        
        private async void Start()
        {
            // DI 컨테이너 초기화
            DIGlobalContext.Reset();
            var container = DIGlobalContext.EnsureContainer();
            
            // Addressables Provider 등록
            var addressableProvider = new AddressableAssetProvider();
            container.Register<IAddressableAssetProvider>(addressableProvider);
            
            // Scene Loader 등록
            var sceneLoader = new SceneLoader();
            container.Register<ISceneLoader>(sceneLoader);
            
            Debug.Log("[Bootstrap] DI Container initialized");
            
            // 초기화 완료 후 Title Scene 로드
            if (autoLoadTitleScene)
            {
                await sceneLoader.LoadSceneAsync("Lobby");
            }
        }
    }
}
```

**Step 2: Bootstrap 씬에 GameObject 추가**

Unity Editor에서:
1. Bootstrap.unity 열기
2. GameObject → Create Empty
3. 이름을 "BootstrapInstaller"로 변경
4. BootstrapInstaller.cs 스크립트 추가

**Step 3: 검증**

Play Mode에서:
- BootstrapInstaller 오브젝트가 존재하는지 확인
- DI 컨테이너가 초기화되는지 로그 확인

---

## Task 4: Lobby 씬 기본 구성

**Files:**
- Modify: `Assets/Scenes/Lobby.unity` (Unity Editor에서)
- Create: `Assets/Scripts/Core/Router/LobbySceneInitializer.cs`

**Step 1: LobbySceneInitializer 작성**

```csharp
using UnityEngine;

using TowerBreak.DI;
using TowerBreak.GameData.TowerBreaker;

namespace TowerBreak.Core
{
    public sealed class LobbySceneInitializer : MonoBehaviour
    {
        private void Start()
        {
            // GameData 로드
            var gameData = Resources.Load<TowerBreakerGameData>("TowerBreakerGameData");
            if (gameData == null)
            {
                Debug.LogError("[Lobby] TowerBreakerGameData not found in Resources");
                return;
            }
            
            Debug.Log($"[Lobby] Loaded GameData with {gameData.Floors.Count} floors");
            
            // TODO: Lobby UI 초기화
            // TODO: 플레이어 세션 상태 로드
        }
    }
}
```

**Step 2: Lobby 씬에 오브젝트 추가**

Unity Editor에서:
1. Lobby.unity 열기
2. GameObject → Create Empty
3. 이름을 "LobbyManager"로 변경
4. LobbySceneInitializer.cs 추가

---

## Task 5: Battle 씬 기본 구성

**Files:**
- Modify: `Assets/Scenes/Battle.unity` (Unity Editor에서)

**Step 1: Battle 씬에 기본 요소 추가**

Unity Editor에서:
1. Battle.unity 열기
2. Camera 설정 (Main Camera)
3. Directional Light 추가
4. BattleLoopController를 가진 GameObject 추가

**Step 2: Battle 씬 스크립트 작성**

```csharp
using UnityEngine;

using TowerBreak.GameData.TowerBreaker;

namespace TowerBreak.Combat
{
    public sealed class BattleSceneInitializer : MonoBehaviour
    {
        [SerializeField] private int currentFloor = 1;
        
        private void Start()
        {
            var gameData = Resources.Load<TowerBreakerGameData>("TowerBreakerGameData");
            if (gameData == null)
            {
                Debug.LogError("[Battle] TowerBreakerGameData not found");
                return;
            }
            
            Debug.Log($"[Battle] Starting battle for floor {currentFloor}");
            
            // TODO: 웨이브 스폰 시작
            // TODO: BattleLoopController 초기화
        }
    }
}
```

---

## Task 6: GameData 리소스 검증

**Files:**
- Check: `Assets/Resources/TowerBreakerGameData.asset`
- Check: `Assets/Data/Design/TowerBreaker-MVP.xlsx`

**Step 1: GameData 에셋 확인**

파일이 존재하는지 확인하고 내용 검사

**Step 2: Excel 파일에서 데이터 임포트**

Unity Editor에서:
1. Tools → GameData → Import from Excel 메뉴 확인
2. Excel 파일 경로 확인
3. Import 실행

**Step 3: GameData 검증**

TowerBreakerGameData.asset이 올바르게 생성되었는지 확인:
- Floors 배열이 비어있지 않음
- Enemies 배열이 비어있지 않음
- Weapons 배열이 비어있지 않음

---

## Task 7: 통합 테스트

**Files:**
- Test: Play Mode에서 전체 흐름 검증

**Step 1: Bootstrap → Lobby 흐름 테스트**

1. Bootstrap 씬에서 Play
2. Lobby 씬으로 자동 전환되는지 확인
3. GameData가 로드되는지 확인

**Step 2: Lobby → Battle 흐름 테스트 (수동)**

1. Lobby 씬에서 Battle로 전환 테스트
2. Battle 씬에서 적 스폰 테스트

**Step 3: 로그 확인**

Console 창에서 에러 로그 확인:
- NullReferenceException 없음
- GameData 로드 성공
- Addressables 로드 성공

---

## Task 8: 문서화 및 마일스톤 정리

**Files:**
- Create: `docs/plans/2026-03-14-scene-bootstrap/milestones/bootstrap-setup/README.md`

**Step 1: 마일스톤 문서 작성**

완료된 작업 기록:
- Build Settings에 씬 등록
- Bootstrap 씬에 DI Installer 추가
- Lobby/Battle 씬 기본 구성
- GameData 검증

**Step 2: 아카이브**

마일스톤 완료 후 archive/ 폴더로 이동

---

## Acceptance Criteria

- [ ] Build Settings에 Bootstrap, Lobby, Battle 씬이 등록됨
- [ ] Bootstrap 씬에 DI Installer가 있고 DI 컨테이너가 초기화됨
- [ ] Play Mode에서 Bootstrap → Lobby로 자동 전환됨
- [ ] GameData가 Resources에서 정상 로드됨
- [ ] Console에 에러 로그가 없음
