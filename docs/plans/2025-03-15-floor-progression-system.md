# Floor Progression System Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 현재 층 클리어 시 다음 층으로 자동 진행하고, 보스 층 클리어 시 게임 완료 처리

**Architecture:** BattleSceneInitializer가 전투 종료(승리/패배)를 감지하면 StageManager를 통해 다음 층으로 진행하거나 게임 완료 처리를 수행합니다.

**Tech Stack:** Unity, C#, TowerBreak.Combat 어셈블리

---

## Task 1: 층 클리어 이벤트 시스템 구현

**Files:**
- Modify: `Assets/Scripts/Combat/BattleSceneInitializer.cs`

**Step 1: 층 클리어 메서드 추가**

BattleSceneInitializer에 층 클리어 처리 메서드 추가:

```csharp
private bool isFloorClearPopupShown = false;

private void HandleFloorClear()
{
    Debug.Log($"[Battle] Floor {currentFloor} cleared!");
    
    // FloorStage에 클리어 표시
    var floorStage = GetCurrentFloorStage();
    if (floorStage != null)
    {
        floorStage.MarkAsCompleted();
    }
    
    // 마지막 층(보스) 체크
    if (stageManager != null && currentFloor >= stageManager.TotalFloors)
    {
        ShowGameCompletePopup();
        return;
    }
    
    // 층 클리어 팝업 표시
    ShowFloorClearPopup();
}

private void ShowFloorClearPopup()
{
    isFloorClearPopupShown = true;
    Debug.Log("[Battle] Showing floor clear popup - Press 'Continue' to advance");
}

private void ShowGameCompletePopup()
{
    // TODO: 게임 완료 팝업 표시
    Debug.Log("[Battle] Showing game complete popup");
    HandleGameComplete();
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
    buttonStyle.fontSize = 24;
    buttonStyle.alignment = TextAnchor.MiddleCenter;
    
    float buttonWidth = 180;
    float buttonHeight = 50;
    float buttonY = popupRect.y + popupHeight - 90;
    
    // 계속하기 버튼
    if (GUI.Button(new Rect(popupRect.x + (popupWidth - buttonWidth) / 2, buttonY, buttonWidth, buttonHeight), "계속하기", buttonStyle))
    {
        isFloorClearPopupShown = false;
        AdvanceToNextFloor();
    }
}

private void HandleGameComplete()
{
    Debug.Log("[Battle] GAME COMPLETE! All floors cleared!");
    
    // TODO: 게임 완료 화면 표시
    // TODO: 보상 지급
    // TODO: 로비로 이동 또는 엔딩 씬
    
    // 임시: 로비로 이동
    UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
}

private void AdvanceToNextFloor()
{
    Debug.Log($"[Battle] Advancing to floor {currentFloor + 1}...");
    
    // 다음 층 설정
    currentFloor++;
    
    // 씬 리로드하여 새 층 로드
    // 데이터는 DI나 PlayerPrefs로 유지
    UnityEngine.SceneManagement.SceneManager.LoadScene("Battle");
}
```

**Step 2: 승리 조건 체크 수정**

```csharp
// Update 메서드에서 승리 체크 부분 수정
private void Update()
{
    if (battleLoopController == null || wallDefeated) return;
    
    // 적 압박 업데이트
    var result = battleLoopController.Update(Time.deltaTime);
    
    if (result.DidAdvancePressure)
    {
        if (result.PressureResult.DefeatedWall)
        {
            Debug.Log("[Battle] WALL DEFEATED! Game Over.");
            wallDefeated = true;
            HandleFloorFail(); // 패배 처리
            return;
        }
    }
    
    // 플레이어 입력 처리
    HandlePlayerInput();
    
    // 승리 조건 체크 - 모든 적 처치
    if (battleLoopController.State.Enemies.Count == 0 && spawnedEnemies.Count > 0 && !victoryHandled)
    {
        Debug.Log("[Battle] VICTORY! All enemies defeated!");
        victoryHandled = true;
        HandleFloorClear(); // 층 클리어 처리
        return;
    }
}

private void HandleFloorFail()
{
    Debug.Log($"[Battle] Floor {currentFloor} failed!");
    
    // FloorStage에 실패 표시
    var floorStage = GetCurrentFloorStage();
    if (floorStage != null)
    {
        floorStage.MarkAsFailed();
    }
    
    // TODO: 게임 오버 화면 표시
    // TODO: 로비로 이동 또는 재시도 옵션
    
    // 임시: 로비로 이동
    UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
}
```

**Commit:**
```bash
git add Assets/Scripts/Combat/BattleSceneInitializer.cs
git commit -m "feat: add floor clear and progression system"
```

---

## Task 2: 현재 층 데이터 유지 (PlayerPrefs 사용)

**Files:**
- Modify: `Assets/Scripts/Combat/BattleSceneInitializer.cs`

**Step 1: 현재 층 저장/로드**

```csharp
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

private void SaveCurrentFloor()
{
    PlayerPrefs.SetInt(CurrentFloorKey, currentFloor);
    PlayerPrefs.Save();
    Debug.Log($"[Battle] Saved current floor: {currentFloor}");
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

private void ClearSavedFloor()
{
    if (PlayerPrefs.HasKey(CurrentFloorKey))
    {
        PlayerPrefs.DeleteKey(CurrentFloorKey);
        PlayerPrefs.Save();
        Debug.Log("[Battle] Cleared saved floor data");
    }
}
```

**Step 2: AdvanceToNextFloor 수정**

```csharp
private void AdvanceToNextFloor()
{
    Debug.Log($"[Battle] Advancing to floor {currentFloor + 1}...");
    
    // 다음 층 저장
    currentFloor++;
    SaveCurrentFloor();
    
    // 씬 리로드
    UnityEngine.SceneManagement.SceneManager.LoadScene("Battle");
}
```

**Step 3: 게임 완료/실패 시 저장 데이터 삭제**

```csharp
private void HandleGameComplete()
{
    Debug.Log("[Battle] GAME COMPLETE! All floors cleared!");
    
    // 저장 데이터 삭제
    ClearSavedFloor();
    
    // 로비로 이동
    UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
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
    
    // 로비로 이동
    UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
}
```

**Commit:**
```bash
git add Assets/Scripts/Combat/BattleSceneInitializer.cs
git commit -m "feat: persist current floor using PlayerPrefs"
```

---

## Task 3: 로비에서 전투 시작 시 층 초기화

**Files:**
- Modify: `Assets/Scripts/Core/LobbySceneInitializer.cs` (또는 Lobby 진입 시)

**Step 1: 로비 진입 시 층 데이터 초기화 옵션**

```csharp
// LobbySceneInitializer.cs 또는 Lobby 씬 진입 시 실행되는 스크립트

private const string CurrentFloorKey = "CurrentBattleFloor";

private void Start()
{
    // 새로운 게임 시작 시 층 데이터 초기화
    // TODO: "새 게임" vs "계속하기" 구분 필요
    
    // 현재는 항상 1층부터 시작
    ResetToFloorOne();
}

private void ResetToFloorOne()
{
    if (PlayerPrefs.HasKey(CurrentFloorKey))
    {
        PlayerPrefs.DeleteKey(CurrentFloorKey);
        PlayerPrefs.Save();
        Debug.Log("[Lobby] Reset to floor 1 for new game");
    }
}
```

**Commit:**
```bash
git add Assets/Scripts/Core/LobbySceneInitializer.cs
git commit -m "feat: reset floor progress when entering lobby"
```

---

## Task 4: 층 클리어 UI/효과 추가 (선택사항)

**Files:**
- Modify: `Assets/Scripts/Combat/BattleSceneInitializer.cs`

**Step 1: 층 클리어 시 딜레이 추가**

```csharp
private async void HandleFloorClear()
{
    Debug.Log($"[Battle] Floor {currentFloor} cleared!");
    
    // FloorStage에 클리어 표시
    var floorStage = GetCurrentFloorStage();
    if (floorStage != null)
    {
        floorStage.MarkAsCompleted();
    }
    
    // 층 클리어 효과 (1초 대기)
    await Task.Delay(1000);
    
    // 마지막 층(보스) 체크
    if (stageManager != null && currentFloor >= stageManager.TotalFloors)
    {
        HandleGameComplete();
        return;
    }
    
    // 다음 층으로 진행
    AdvanceToNextFloor();
}
```

**Commit:**
```bash
git add Assets/Scripts/Combat/BattleSceneInitializer.cs
git commit -m "feat: add delay before floor transition"
```

---

## Task 5: FloorStage 시각적 클리어 표시 개선

**Files:**
- Modify: `Assets/Scripts/Combat/FloorStage.cs`

**Step 1: 클리어 시 효과 추가**

```csharp
public void MarkAsCompleted()
{
    isCompleted = true;
    
    if (floorRenderer != null)
    {
        // 완료된 층은 녹색으로 표시
        floorRenderer.color = new Color(0.3f, 0.8f, 0.3f, 1f);
        
        // 클리어 효과: 크기 애니메이션
        StartCoroutine(PlayClearEffect());
    }
    
    Debug.Log($"[FloorStage] Floor {floorNumber} completed!");
}

private System.Collections.IEnumerator PlayClearEffect()
{
    Vector3 originalScale = transform.localScale;
    Vector3 targetScale = originalScale * 1.2f;
    
    // 커지기
    float duration = 0.3f;
    float elapsed = 0f;
    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = elapsed / duration;
        transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
        yield return null;
    }
    
    // 원래대로
    elapsed = 0f;
    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = elapsed / duration;
        transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
        yield return null;
    }
    
    transform.localScale = originalScale;
}
```

**Commit:**
```bash
git add Assets/Scripts/Combat/FloorStage.cs
git commit -m "feat: add visual clear effect to FloorStage"
```

---

## Verification Steps

**1. Unity 에디터에서 테스트:**
- Battle 씬에서 1층 시작
- 모든 적 처치 후 2층으로 자동 진행 확인
- 마지막 층 클리어 시 로비로 이동 확인
- 게임 종료 후 재시작 시 1층부터 시작 확인

**2. 확인할 사항:**
- [ ] 적 모두 처치 시 자동으로 다음 층으로 이동
- [ ] 층 전환 시 현재 층 번호 저장됨
- [ ] 보스 층 클리어 시 로비로 이동
- [ ] 게임 실패 시 로비로 이동하고 저장 데이터 삭제
- [ ] 로비에서 전투 시작 시 항상 1층부터 시작

**3. 예상 로그 출력:**
```
[Battle] Floor 1 cleared!
[Battle] Saved current floor: 2
[Battle] Advancing to floor 2...
[Battle] Loaded saved floor: 2
[Battle] Floor 2 cleared!
...
[Battle] GAME COMPLETE! All floors cleared!
[Battle] Cleared saved floor data
```

---

## Summary

이 변경으로 다음과 같이 동작합니다:

1. **층 클리어** → 자동으로 다음 층으로 진행
2. **층 데이터 저장** → PlayerPrefs로 현재 층 유지
3. **보스 클리어** → 게임 완료 후 로비로 이동
4. **게임 실패** → 로비로 이동하고 저장 데이터 삭제
5. **새 게임** → 로비에서 전투 시작 시 1층부터 시작