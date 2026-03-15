using UnityEngine;
using UnityEngine.UI;

namespace TowerBreak.Combat
{
    public sealed class BattleUIInstaller : MonoBehaviour
    {
        [SerializeField] private Button attackButton;
        [SerializeField] private Button guardButton;
        [SerializeField] private Button dashButton;
        
        private void Awake()
        {
            // 버튼 찾기 (직접 할당되지 않은 경우)
            FindButtonsIfNotAssigned();
            
            Debug.Log($"[BattleUIInstaller] Buttons found - Attack: {attackButton != null}, Guard: {guardButton != null}, Dash: {dashButton != null}");
        }
        
        public Button AttackButton => attackButton;
        public Button GuardButton => guardButton;
        public Button DashButton => dashButton;
        
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
            
            // 직접적인 자식에서 찾기
            var button = canvas.transform.Find(buttonName)?.GetComponent<Button>();
            if (button != null) return button;
            
            // 모든 자식에서 검색
            foreach (Transform child in canvas.transform.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == buttonName)
                {
                    button = child.GetComponent<Button>();
                    if (button != null) return button;
                }
            }
            
            return null;
        }
    }
}
