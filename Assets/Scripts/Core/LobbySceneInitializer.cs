using UnityEngine;

using TowerBreak.DI;
using TowerBreak.GameData.TowerBreaker;

namespace TowerBreak.Core
{
    public sealed class LobbySceneInitializer : MonoBehaviour
    {
        [SerializeField] private int currentFloor = 1;
        
        private const string CurrentFloorKey = "CurrentBattleFloor";
        
        private void Start()
        {
            // 로비 진입 시 층 데이터 초기화
            ResetFloorProgress();
            
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
                var floorWaves = gameData.FloorWaves.FindAll(w => w.FloorId == floor.Id);
                int waveCount = floorWaves.Count > 0 ? floorWaves[floorWaves.Count - 1].WaveIndex + 1 : 0;
                Debug.Log($"[Lobby] Current Floor: {currentFloor}, Waves: {waveCount}");
            }
            
            // TODO: Lobby UI 초기화
            // TODO: 플레이어 세션 상태 로드
            
            Debug.Log("[Lobby] Initialization complete. Press 'B' to start battle!");
        }
        
        private void ResetFloorProgress()
        {
            if (PlayerPrefs.HasKey(CurrentFloorKey))
            {
                PlayerPrefs.DeleteKey(CurrentFloorKey);
                PlayerPrefs.Save();
                Debug.Log("[Lobby] Reset floor progress for new game");
            }
        }
        
        private void Update()
        {
            // 테스트용: B 키로 Battle 씬 전환
            if (UnityEngine.InputSystem.Keyboard.current != null && 
                UnityEngine.InputSystem.Keyboard.current.bKey.wasPressedThisFrame)
            {
                Debug.Log("[Lobby] Starting battle...");
                var sceneLoader = DIGlobalContext.EnsureContainer().Resolve<ISceneLoader>();
                sceneLoader.LoadScene("Battle");
            }
        }
    }
}
