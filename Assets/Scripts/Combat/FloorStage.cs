using System;
using UnityEngine;

using TowerBreak.GameData.TowerBreaker;

namespace TowerBreak.Combat
{
    public sealed class FloorStage : MonoBehaviour
    {
        [SerializeField] private int floorNumber;
        [SerializeField] private FloorRow floorData;
        [SerializeField] private SpriteRenderer floorRenderer;
        [SerializeField] private Transform enemySpawnPoints;
        [SerializeField] private Transform wallPosition;
        
        private bool isCurrentFloor;
        private bool isCompleted;
        private bool isBossFloor;
        
        public int FloorNumber => floorNumber;
        public FloorRow FloorData => floorData;
        public bool IsCurrentFloor => isCurrentFloor;
        public bool IsCompleted => isCompleted;
        public bool IsBossFloor => isBossFloor;
        public Transform EnemySpawnPoints => enemySpawnPoints;
        public Transform WallPosition => wallPosition;
        
        public void Initialize(int number, FloorRow data)
        {
            floorNumber = number;
            floorData = data;
            isCurrentFloor = false;
            isCompleted = false;
            isBossFloor = false;
            
            SetupVisuals();
            SetupSpawnPoints();
            SetupWallPosition();
            
            if (data != null)
            {
                Debug.Log($"[FloorStage] Initialized Floor {number}: {data.DisplayName}");
            }
            else
            {
                Debug.Log($"[FloorStage] Initialized Floor {number}: (no data)");
            }
        }
        
        public void SetAsCurrentFloor(bool isCurrent)
        {
            isCurrentFloor = isCurrent;
            
            if (floorRenderer != null)
            {
                // 현재 층은 밝게, 나머지는 어둡게
                // 보스 층은 보라색으로 표시
                if (isBossFloor)
                {
                    floorRenderer.color = isCurrent ? new Color(0.8f, 0.3f, 0.8f, 1f) : new Color(0.4f, 0.2f, 0.4f, 1f);
                }
                else
                {
                    floorRenderer.color = isCurrent ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
                }
            }
            
            Debug.Log($"[FloorStage] Floor {floorNumber} isCurrent: {isCurrent}, isBoss: {isBossFloor}");
        }
        
        public void SetAsBossFloor(bool isBoss)
        {
            isBossFloor = isBoss;
            
            if (floorRenderer != null && isBoss)
            {
                // 보스 층은 보라색으로 표시
                floorRenderer.color = new Color(0.8f, 0.3f, 0.8f, 1f);
            }
            
            Debug.Log($"[FloorStage] Floor {floorNumber} isBoss: {isBoss}");
        }
        
        public void MarkAsCompleted()
        {
            isCompleted = true;
            
            if (floorRenderer != null)
            {
                // 완료된 층은 녹색으로 표시
                floorRenderer.color = new Color(0.3f, 0.8f, 0.3f, 1f);
            }
            
            Debug.Log($"[FloorStage] Floor {floorNumber} completed!");
        }
        
        public void MarkAsFailed()
        {
            isCompleted = false;
            
            if (floorRenderer != null)
            {
                // 실패한 층은 붉은색으로 표시
                floorRenderer.color = new Color(0.8f, 0.3f, 0.3f, 1f);
            }
            
            Debug.Log($"[FloorStage] Floor {floorNumber} failed!");
        }
        
        private void SetupVisuals()
        {
            floorRenderer = GetComponent<SpriteRenderer>();
            if (floorRenderer == null)
            {
                floorRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
            
            // 기본 층 색상 설정
            floorRenderer.color = Color.gray;
        }
        
        private void SetupSpawnPoints()
        {
            // EnemySpawnPoints 트랜스폼 찾기 또는 생성
            enemySpawnPoints = transform.Find("EnemySpawnPoints");
            if (enemySpawnPoints == null)
            {
                GameObject spawnPointsObj = new GameObject("EnemySpawnPoints");
                spawnPointsObj.transform.SetParent(transform);
                spawnPointsObj.transform.localPosition = Vector3.zero;
                enemySpawnPoints = spawnPointsObj.transform;
                
                // 기본 스폰 포인트 생성 (중앙에 여러 위치)
                CreateDefaultSpawnPoints();
            }
        }
        
        private void SetupWallPosition()
        {
            // WallPosition 트랜스폼 찾기 또는 생성
            wallPosition = transform.Find("WallPosition");
            if (wallPosition == null)
            {
                GameObject wallObj = new GameObject("WallPosition");
                wallObj.transform.SetParent(transform);
                wallObj.transform.localPosition = new Vector3(-8f, 0f, 0f); // 왼쪽 벽 위치
                wallPosition = wallObj.transform;
            }
        }
        
        private void CreateDefaultSpawnPoints()
        {
            // 여러 적 스폰 위치 생성
            for (int i = 0; i < 5; i++)
            {
                GameObject point = new GameObject($"SpawnPoint_{i}");
                point.transform.SetParent(enemySpawnPoints);
                
                // 중앙을 기준으로 좌우로 배치
                float xOffset = (i - 2) * 1.5f;
                point.transform.localPosition = new Vector3(xOffset, 0f, 0f);
            }
        }
        
        public Vector3 GetSpawnPosition(int index)
        {
            if (enemySpawnPoints == null || enemySpawnPoints.childCount == 0)
            {
                return transform.position;
            }
            
            index = Mathf.Clamp(index, 0, enemySpawnPoints.childCount - 1);
            return enemySpawnPoints.GetChild(index).position;
        }
        
        public Vector3 GetWallPosition()
        {
            if (wallPosition != null)
            {
                return wallPosition.position;
            }
            
            // 기본값: 층의 왼쪽
            return transform.position + new Vector3(-8f, 0f, 0f);
        }
        
        private void OnDrawGizmos()
        {
            // 층 범위 시각화
            if (isBossFloor)
            {
                Gizmos.color = isCurrentFloor ? new Color(0.8f, 0.3f, 0.8f, 1f) : new Color(0.4f, 0.2f, 0.4f, 1f);
            }
            else
            {
                Gizmos.color = isCurrentFloor ? Color.green : Color.gray;
            }
            
            Vector3 center = transform.position;
            Vector3 size = new Vector3(20f, 2f, 1f);
            Gizmos.DrawWireCube(center, size);
            
            // 보스 층 표시
            if (isBossFloor)
            {
                Gizmos.DrawWireCube(center, size * 1.1f);
            }
            
            // 벽 위치 표시
            if (wallPosition != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(wallPosition.position, 0.3f);
            }
            
            // 스폰 포인트 표시
            if (enemySpawnPoints != null)
            {
                Gizmos.color = isBossFloor ? new Color(1f, 0f, 1f, 1f) : Color.red;
                foreach (Transform point in enemySpawnPoints)
                {
                    Gizmos.DrawSphere(point.position, 0.2f);
                }
            }
        }
    }
}