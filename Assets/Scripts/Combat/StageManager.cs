using System;
using System.Collections.Generic;
using UnityEngine;

using TowerBreak.GameData.TowerBreaker;

namespace TowerBreak.Combat
{
    public sealed class StageManager : MonoBehaviour
    {
        [Header("Stage Configuration")]
        [SerializeField] private int currentFloor = 1;
        [SerializeField] private float floorHeight = 10f;
        [SerializeField] private int visibleFloorCount = 3;
        [SerializeField] private bool stackBackgroundPerFloor = true;
        [SerializeField] private bool stackMidGroundPerFloor = true;
        
        [Header("Normal Floor Prefabs")]
        [SerializeField] private GameObject backgroundPrefab;
        [SerializeField] private GameObject midGroundPrefab;
        [SerializeField] private GameObject floorPrefab;
        
        [Header("Boss Floor Prefabs (Last Floor)")]
        [SerializeField] private GameObject bossBackgroundPrefab;
        [SerializeField] private GameObject bossMidGroundPrefab;
        [SerializeField] private GameObject bossFloorPrefab;
        
        [Header("Spawn Settings")]
        [SerializeField] private Transform stageParent;
        
        private TowerBreakerGameData gameData;
        private List<GameObject> spawnedFloors = new();
        private List<GameObject> spawnedBackgrounds = new();
        private List<GameObject> spawnedMidGrounds = new();
        
        public int CurrentFloor => currentFloor;
        public int TotalFloors => gameData?.Floors?.Count ?? 0;
        public float FloorHeight => floorHeight;
        public bool IsBossFloor => currentFloor == TotalFloors && TotalFloors > 0;
        
        private void Awake()
        {
            if (stageParent == null)
            {
                stageParent = transform;
            }
        }
        
        private void Start()
        {
            InitializeStage();
        }
        
        public void InitializeStage()
        {
            LoadGameData();
            
            if (gameData == null)
            {
                Debug.LogError("[StageManager] Failed to load game data!");
                return;
            }
            
            ValidateFloorIndex();
            ClearExistingStage();
            BuildStage();
            
            string floorType = IsBossFloor ? "BOSS" : "NORMAL";
            Debug.Log($"[StageManager] Stage initialized for floor {currentFloor}/{TotalFloors} ({floorType})");
        }
        
        public void AdvanceFloor()
        {
            if (currentFloor >= TotalFloors)
            {
                Debug.Log("[StageManager] Already at the top floor!");
                return;
            }
            
            currentFloor++;
            InitializeStage();
        }
        
        public void SetFloor(int floorNumber)
        {
            if (floorNumber < 1 || floorNumber > TotalFloors)
            {
                throw new ArgumentOutOfRangeException(nameof(floorNumber), 
                    $"Floor must be between 1 and {TotalFloors}");
            }
            
            currentFloor = floorNumber;
            InitializeStage();
        }
        
        private void LoadGameData()
        {
            gameData = Resources.Load<TowerBreakerGameData>("TowerBreakerGameData");
        }
        
        private void ValidateFloorIndex()
        {
            if (currentFloor < 1)
            {
                currentFloor = 1;
            }
            
            if (TotalFloors > 0 && currentFloor > TotalFloors)
            {
                currentFloor = TotalFloors;
            }
        }
        
        private void ClearExistingStage()
        {
            foreach (var floor in spawnedFloors)
            {
                if (floor != null)
                {
                    Destroy(floor);
                }
            }
            spawnedFloors.Clear();
            
            foreach (var bg in spawnedBackgrounds)
            {
                if (bg != null)
                {
                    Destroy(bg);
                }
            }
            spawnedBackgrounds.Clear();
            
            foreach (var mg in spawnedMidGrounds)
            {
                if (mg != null)
                {
                    Destroy(mg);
                }
            }
            spawnedMidGrounds.Clear();
        }
        
        private void BuildStage()
        {
            // 현재 층부터 위로 visibleFloorCount만큼 쌓기
            int floorsToShow = Mathf.Min(visibleFloorCount, TotalFloors - currentFloor + 1);
            
            // 보스 층 여부에 따라 다른 프리팹 사용
            bool useBossPrefabs = IsBossFloor;
            
            // 배경과 미들그라운드를 층별로 쌓을지 결정
            if (stackBackgroundPerFloor)
            {
                BuildBackgroundsPerFloor(floorsToShow, useBossPrefabs);
            }
            else
            {
                BuildSingleBackground(useBossPrefabs);
            }
            
            if (stackMidGroundPerFloor)
            {
                BuildMidGroundsPerFloor(floorsToShow, useBossPrefabs);
            }
            else
            {
                BuildSingleMidGround(useBossPrefabs);
            }
            
            BuildFloors(floorsToShow, useBossPrefabs);
        }
        
        private void BuildBackgroundsPerFloor(int floorsToShow, bool useBossPrefabs)
        {
            GameObject prefabToUse = GetBackgroundPrefab(useBossPrefabs);
            
            if (prefabToUse == null)
            {
                Debug.LogWarning("[StageManager] Background prefab is not assigned!");
                return;
            }
            
            for (int i = 0; i < floorsToShow; i++)
            {
                int floorNumber = currentFloor + i;
                bool isThisFloorBoss = floorNumber == TotalFloors;
                GameObject actualPrefab = isThisFloorBoss && bossBackgroundPrefab != null 
                    ? bossBackgroundPrefab 
                    : prefabToUse;
                
                GameObject background = Instantiate(actualPrefab, stageParent);
                background.name = $"Background_Floor_{floorNumber}";
                
                // 층별로 위치 설정
                float yPosition = i * floorHeight;
                PositionBackground(background, yPosition);
                
                // 레이어 순서 (가장 뒤)
                background.transform.SetSiblingIndex(i * 3);
                
                spawnedBackgrounds.Add(background);
            }
            
            Debug.Log($"[StageManager] Built {spawnedBackgrounds.Count} backgrounds (per floor)");
        }
        
        private void BuildSingleBackground(bool useBossPrefabs)
        {
            GameObject prefabToUse = GetBackgroundPrefab(useBossPrefabs);
            
            if (prefabToUse == null)
            {
                Debug.LogWarning("[StageManager] Background prefab is not assigned!");
                return;
            }
            
            GameObject background = Instantiate(prefabToUse, stageParent);
            background.name = IsBossFloor ? "Background_Boss" : "Background";
            
            // 배경은 가장 뒤에 위치
            background.transform.SetSiblingIndex(0);
            
            // 배경 위치 설정 (카메라 중심 기준)
            PositionBackground(background, 0f);
            
            spawnedBackgrounds.Add(background);
        }
        
        private void BuildMidGroundsPerFloor(int floorsToShow, bool useBossPrefabs)
        {
            GameObject prefabToUse = GetMidGroundPrefab(useBossPrefabs);
            
            if (prefabToUse == null)
            {
                Debug.LogWarning("[StageManager] MidGround prefab is not assigned!");
                return;
            }
            
            for (int i = 0; i < floorsToShow; i++)
            {
                int floorNumber = currentFloor + i;
                bool isThisFloorBoss = floorNumber == TotalFloors;
                GameObject actualPrefab = isThisFloorBoss && bossMidGroundPrefab != null 
                    ? bossMidGroundPrefab 
                    : prefabToUse;
                
                GameObject midGround = Instantiate(actualPrefab, stageParent);
                midGround.name = $"MidGround_Floor_{floorNumber}";
                
                // 층별로 위치 설정
                float yPosition = i * floorHeight;
                PositionMidGround(midGround, yPosition);
                
                // 레이어 순서 (배경 다음)
                midGround.transform.SetSiblingIndex(i * 3 + 1);
                
                spawnedMidGrounds.Add(midGround);
            }
            
            Debug.Log($"[StageManager] Built {spawnedMidGrounds.Count} midgrounds (per floor)");
        }
        
        private void BuildSingleMidGround(bool useBossPrefabs)
        {
            GameObject prefabToUse = GetMidGroundPrefab(useBossPrefabs);
            
            if (prefabToUse == null)
            {
                Debug.LogWarning("[StageManager] MidGround prefab is not assigned!");
                return;
            }
            
            GameObject midGround = Instantiate(prefabToUse, stageParent);
            midGround.name = IsBossFloor ? "MidGround_Boss" : "MidGround";
            
            // 중간 배경은 배경 다음에 위치
            midGround.transform.SetSiblingIndex(1);
            
            PositionMidGround(midGround, 0f);
            
            spawnedMidGrounds.Add(midGround);
        }
        
        private void BuildFloors(int floorsToShow, bool useBossPrefabs)
        {
            GameObject prefabToUse = GetFloorPrefab(useBossPrefabs);
            
            if (prefabToUse == null)
            {
                Debug.LogError("[StageManager] Floor prefab is not assigned!");
                return;
            }
            
            for (int i = 0; i < floorsToShow; i++)
            {
                int floorNumber = currentFloor + i;
                bool isThisFloorBoss = floorNumber == TotalFloors;
                GameObject actualPrefab = isThisFloorBoss && bossFloorPrefab != null 
                    ? bossFloorPrefab 
                    : prefabToUse;
                
                GameObject floor = CreateFloor(floorNumber, i, actualPrefab, isThisFloorBoss);
                spawnedFloors.Add(floor);
            }
            
            Debug.Log($"[StageManager] Built {spawnedFloors.Count} floors starting from floor {currentFloor}");
        }
        
        private GameObject CreateFloor(int floorNumber, int visualIndex, GameObject prefab, bool isBoss)
        {
            GameObject floor = Instantiate(prefab, stageParent);
            floor.name = isBoss ? $"Floor_{floorNumber}_BOSS" : $"Floor_{floorNumber}";
            
            // 층 위치 계산 (아래에서 위로 쌓이는 구조)
            // visualIndex가 0일 때가 현재 층(가장 아래), 숫자가 커질수록 위로
            float yPosition = visualIndex * floorHeight;
            floor.transform.position = new Vector3(0f, yPosition, 0f);
            
            // Floor 컴포넌트 설정
            FloorStage floorComponent = floor.GetComponent<FloorStage>();
            if (floorComponent == null)
            {
                floorComponent = floor.AddComponent<FloorStage>();
            }
            
            FloorRow floorData = GetFloorData(floorNumber);
            floorComponent.Initialize(floorNumber, floorData);
            
            // 첫 번째 층을 현재 층으로 표시
            if (visualIndex == 0)
            {
                floorComponent.SetAsCurrentFloor(true);
            }
            
            // 보스 층 표시
            if (isBoss)
            {
                floorComponent.SetAsBossFloor(true);
            }
            
            // 레이어 순서 설정 (위에 있는 층이 앞에 보이도록)
            floor.transform.SetSiblingIndex(2 + visualIndex);
            
            return floor;
        }
        
        private GameObject GetBackgroundPrefab(bool useBoss)
        {
            if (useBoss && bossBackgroundPrefab != null)
            {
                return bossBackgroundPrefab;
            }
            return backgroundPrefab;
        }
        
        private GameObject GetMidGroundPrefab(bool useBoss)
        {
            if (useBoss && bossMidGroundPrefab != null)
            {
                return bossMidGroundPrefab;
            }
            return midGroundPrefab;
        }
        
        private GameObject GetFloorPrefab(bool useBoss)
        {
            if (useBoss && bossFloorPrefab != null)
            {
                return bossFloorPrefab;
            }
            return floorPrefab;
        }
        
        private FloorRow GetFloorData(int floorNumber)
        {
            if (gameData?.Floors == null)
            {
                return null;
            }
            
            return gameData.Floors.Find(f => f.Id == floorNumber);
        }
        
        private void PositionBackground(GameObject background, float yOffset)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                background.transform.position = new Vector3(0f, yOffset, 10f);
                return;
            }
            
            // 카메라 중심 위치에 yOffset 적용
            Vector3 cameraPos = mainCamera.transform.position;
            background.transform.position = new Vector3(cameraPos.x, cameraPos.y + yOffset, 10f);
            
            // 배경 스케일 조정 (화면 전체를 채우도록)
            float cameraHeight = mainCamera.orthographicSize * 2f;
            float cameraWidth = cameraHeight * mainCamera.aspect;
            
            SpriteRenderer sr = background.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                float spriteWidth = sr.sprite.bounds.size.x;
                float spriteHeight = sr.sprite.bounds.size.y;
                
                float scaleX = cameraWidth / spriteWidth;
                float scaleY = cameraHeight / spriteHeight;
                
                background.transform.localScale = new Vector3(scaleX, scaleY, 1f);
            }
        }
        
        private void PositionMidGround(GameObject midGround, float yOffset)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                midGround.transform.position = new Vector3(0f, yOffset, 5f);
                return;
            }
            
            // 카메라 중심 위치에 yOffset 적용
            Vector3 cameraPos = mainCamera.transform.position;
            midGround.transform.position = new Vector3(cameraPos.x, cameraPos.y + yOffset, 5f);
        }
        
        // 편집기에서 설정값 변경 시 유효성 검사
        private void OnValidate()
        {
            if (floorHeight < 0f)
            {
                floorHeight = 0f;
            }
            
            if (visibleFloorCount < 1)
            {
                visibleFloorCount = 1;
            }
            
            if (currentFloor < 1)
            {
                currentFloor = 1;
            }
        }
    }
}