using UnityEngine;

using TowerBreak.DI;
using TowerBreak.GameData.TowerBreaker;
using TowerBreak.UIFlow.Lobby;
using TowerBreak.Core.State;
using TowerBreak.Core.Router;
using TowerBreak.Meta.State;

namespace TowerBreak.Core
{
    public sealed class LobbySceneInitializer : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private LobbyView lobbyViewPrefab;
        [SerializeField] private EquipmentView equipmentViewPrefab;
        [SerializeField] private EnhancementView enhancementViewPrefab;
        [SerializeField] private Transform uiParent;

        [Header("Settings")]
        [SerializeField] private int currentFloor = 1;

        private const string CurrentFloorKey = "CurrentBattleFloor";
        private LobbyView lobbyView;
        private EquipmentView equipmentView;
        private EnhancementView enhancementView;
        private LobbyPresenter lobbyPresenter;

        private void Start()
        {
            Debug.Log("[Lobby] Initializing lobby scene...");

            // 로비 진입 시 층 데이터 초기화
            ResetFloorProgress();

            // DI 컨테이너 확인
            DIContainer container = DIGlobalContext.EnsureContainer();

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

            // MVP 설정
            SetupMVP(container);

            // UI 생성
            CreateLobbyUI();

            Debug.Log("[Lobby] Initialization complete");
        }

        private void SetupMVP(DIContainer container)
        {
            try
            {
                // 상태 및 라우터 가져오기
                var sessionState = container.Resolve<PlayerSessionState>();
                var sceneLoader = container.Resolve<ISceneLoader>();

                // 상태 리더 생성
                var stateReader = new LobbyStateReader(sessionState);

                // 라우터 생성 (this 전달)
                var flowRouter = new LobbyFlowRouter(sceneLoader, sessionState, this);

                // 프레젠터 생성
                lobbyPresenter = new LobbyPresenter(flowRouter, stateReader, sessionState.Wallet);

                Debug.Log("[Lobby] MVP setup complete");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Lobby] Failed to setup MVP: {ex.Message}");
            }
        }

        private void CreateLobbyUI()
        {
            if (lobbyViewPrefab == null)
            {
                Debug.LogError("[Lobby] LobbyView prefab is not assigned!");
                return;
            }

            // UI 인스턴스화
            Transform parent = uiParent ?? transform;
            lobbyView = Instantiate(lobbyViewPrefab, parent);
            lobbyView.name = "LobbyUI";

            // 프레젠터 연결
            if (lobbyPresenter != null)
            {
                lobbyView.Initialize(lobbyPresenter);
            }
            else
            {
                Debug.LogWarning("[Lobby] LobbyPresenter is null, UI will not be functional");
            }

            Debug.Log("[Lobby] Lobby UI created");
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

        public void CreateEquipmentUI()
        {
            if (equipmentViewPrefab == null)
            {
                Debug.LogError("[Lobby] EquipmentView prefab is not assigned!");
                return;
            }

            Transform parent = uiParent ?? transform;
            equipmentView = Instantiate(equipmentViewPrefab, parent);
            equipmentView.name = "EquipmentUI";

            // EquipmentPresenter 생성 및 연결
            try
            {
                var container = DIGlobalContext.EnsureContainer();
                var sessionState = container.Resolve<PlayerSessionState>();
                var gameData = Resources.Load<TowerBreakerGameData>("TowerBreakerGameData");
                var sceneLoader = container.Resolve<ISceneLoader>();
                var flowRouter = new LobbyFlowRouter(sceneLoader, sessionState, this);

                var equipmentPresenter = new EquipmentPresenter(
                    flowRouter,
                    sessionState.Inventory,
                    sessionState.Wallet,
                    gameData
                );

                equipmentView.Initialize(equipmentPresenter);
                Debug.Log("[Lobby] Equipment UI created");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Lobby] Failed to create Equipment UI: {ex.Message}");
            }
        }

        public void CloseEquipmentUI()
        {
            if (equipmentView != null)
            {
                Destroy(equipmentView.gameObject);
                equipmentView = null;
                Debug.Log("[Lobby] Equipment UI closed");
            }
        }

        public void CreateEnhancementUI()
        {
            if (enhancementViewPrefab == null)
            {
                Debug.LogError("[Lobby] EnhancementView prefab is not assigned!");
                return;
            }

            Transform parent = uiParent ?? transform;
            enhancementView = Instantiate(enhancementViewPrefab, parent);
            enhancementView.name = "EnhancementUI";

            try
            {
                var container = DIGlobalContext.EnsureContainer();
                var sessionState = container.Resolve<PlayerSessionState>();
                var gameData = Resources.Load<TowerBreakerGameData>("TowerBreakerGameData");
                var sceneLoader = container.Resolve<ISceneLoader>();
                var flowRouter = new LobbyFlowRouter(sceneLoader, sessionState, this);

                Debug.Log($"[Lobby] Creating EnhancementPresenter with Inventory count: {sessionState.Inventory.Equipment.Count}, Gold: {sessionState.Wallet.Gold}");

                var enhancementPresenter = new EnhancementPresenter(
                    flowRouter,
                    sessionState.Inventory,
                    sessionState.Wallet,
                    gameData
                );

                enhancementView.Initialize(enhancementPresenter);
                Debug.Log("[Lobby] Enhancement UI created");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Lobby] Failed to create Enhancement UI: {ex.Message}");
            }
        }

        public void CloseEnhancementUI()
        {
            if (enhancementView != null)
            {
                Destroy(enhancementView.gameObject);
                enhancementView = null;
                Debug.Log("[Lobby] Enhancement UI closed");
            }
            
            // LobbyView 새로고침 (골드 갱신)
            if (lobbyView != null)
            {
                lobbyView.Refresh();
                Debug.Log("[Lobby] LobbyView refreshed after enhancement");
            }
        }

        private void Update()
        {
            // 테스트용: B 키로 Battle 씬 전환 (기존 기능 유지)
            if (UnityEngine.InputSystem.Keyboard.current != null &&
                UnityEngine.InputSystem.Keyboard.current.bKey.wasPressedThisFrame)
            {
                Debug.Log("[Lobby] Starting battle (keyboard shortcut)...");
                if (lobbyPresenter != null)
                {
                    lobbyPresenter.OnOpenChallenge();
                }
                else
                {
                    var sceneLoader = DIGlobalContext.EnsureContainer().Resolve<ISceneLoader>();
                    sceneLoader.LoadScene("Battle");
                }
            }

            // 테스트용: E 키로 Equipment 화면 토글
            if (UnityEngine.InputSystem.Keyboard.current != null &&
                UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
            {
                if (equipmentView == null)
                {
                    CreateEquipmentUI();
                }
                else
                {
                    CloseEquipmentUI();
                }
            }
        }
    }
}
