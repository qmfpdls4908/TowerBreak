using UnityEngine;
using UnityEngine.UI;

namespace TowerBreak.Combat
{
    public sealed class BattleUIInstaller : MonoBehaviour
    {
        [SerializeField] private Button attackButton;
        [SerializeField] private Button guardButton;
        [SerializeField] private Button dashButton;
        
        private PlayerController playerController;
        
        private void Start()
        {
            // BattleSceneInitializer에서 Initialize()를 호출할 때까지 대기
            Debug.Log("[BattleUIInstaller] Start called - waiting for Initialize()");
        }
        
        public void Initialize(PlayerController controller)
        {
            if (controller == null)
            {
                Debug.LogError("[BattleUIInstaller] PlayerController is null!");
                return;
            }
            
            playerController = controller;
            Debug.Log($"[BattleUIInstaller] PlayerController assigned: {playerController.gameObject.name}");
            
            // 버튼 찾기 (직접 할당되지 않은 경우)
            FindButtonsIfNotAssigned();
            
            // 버튼 검증 로그
            Debug.Log($"[BattleUIInstaller] AttackButton: {(attackButton != null ? attackButton.name : "NULL")}");
            Debug.Log($"[BattleUIInstaller] GuardButton: {(guardButton != null ? guardButton.name : "NULL")}");
            Debug.Log($"[BattleUIInstaller] DashButton: {(dashButton != null ? dashButton.name : "NULL")}");
            
            // 버튼 이벤트 연결
            SetupButtonListeners();
            
            Debug.Log("[BattleUIInstaller] Battle UI initialized successfully");
        }
        
        private void Update()
        {
            // 버튼 활성화 상태 업데이트
            UpdateButtonStates();
        }
        
        private void FindButtonsIfNotAssigned()
        {
            if (attackButton == null)
            {
                attackButton = FindButtonByName("AttackButton");
            }
            if (guardButton == null)
            {
                guardButton = FindButtonByName("GuardButton");
            }
            if (dashButton == null)
            {
                dashButton = FindButtonByName("DashButton");
            }
        }
        
        private Button FindButtonByName(string buttonName)
        {
            var canvas = FindFirstObjectByType<UnityEngine.Canvas>();
            if (canvas == null) return null;
            
            var button = canvas.transform.Find(buttonName)?.GetComponent<Button>();
            if (button == null)
            {
                // 자식 오브젝트에서 검색
                foreach (Transform child in canvas.transform)
                {
                    if (child.name == buttonName)
                    {
                        button = child.GetComponent<Button>();
                        if (button != null) break;
                    }
                }
            }
            return button;
        }
        
        private void SetupButtonListeners()
        {
            if (attackButton != null)
            {
                attackButton.onClick.AddListener(OnAttackClicked);
            }
            if (guardButton != null)
            {
                guardButton.onClick.AddListener(OnGuardClicked);
            }
            if (dashButton != null)
            {
                dashButton.onClick.AddListener(OnDashClicked);
            }
        }
        
        private void UpdateButtonStates()
        {
            if (playerController == null) return;
            
            bool canAttack = playerController.CanPerformAction(PlayerActionType.Attack);
            bool canGuard = playerController.CanPerformAction(PlayerActionType.Guard);
            bool canDash = playerController.CanPerformAction(PlayerActionType.Dash);
            
            if (attackButton != null)
                attackButton.interactable = canAttack;
            
            if (guardButton != null)
                guardButton.interactable = canGuard;
            
            if (dashButton != null)
                dashButton.interactable = canDash;
        }
        
        private void OnAttackClicked()
        {
            Debug.Log("[BattleUIInstaller] Attack button clicked!");
            if (playerController != null)
            {
                Debug.Log("[BattleUIInstaller] Calling PerformAction(Attack)");
                playerController.PerformAction(PlayerActionType.Attack);
            }
            else
            {
                Debug.LogError("[BattleUIInstaller] PlayerController is null!");
            }
        }
        
        private void OnGuardClicked()
        {
            if (playerController != null)
            {
                playerController.PerformAction(PlayerActionType.Guard);
            }
        }
        
        private void OnDashClicked()
        {
            if (playerController != null)
            {
                playerController.PerformAction(PlayerActionType.Dash);
            }
        }
        
        private void OnDestroy()
        {
            if (attackButton != null)
                attackButton.onClick.RemoveListener(OnAttackClicked);
            
            if (guardButton != null)
                guardButton.onClick.RemoveListener(OnGuardClicked);
            
            if (dashButton != null)
                dashButton.onClick.RemoveListener(OnDashClicked);
        }
    }
}
