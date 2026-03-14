using UnityEngine;
using UnityEngine.UI;

namespace TowerBreak.Combat
{
    public sealed class BattleUIManager : MonoBehaviour
    {
        [SerializeField] private Text statusText;
        [SerializeField] private Text wallHealthText;
        [SerializeField] private Text enemyCountText;
        [SerializeField] private Text controlsText;
        
        private void Start()
        {
            // 기본 컨트롤 텍스트 설정
            if (controlsText != null)
            {
                controlsText.text = "SPACE: Attack | G: Guard";
            }
            
            // 다른 텍스트 초기화
            UpdateStatus("Battle Started!");
            UpdateWallHealth(0, 0);
            UpdateEnemyCount(0);
        }
        
        public void UpdateStatus(string message)
        {
            if (statusText != null)
                statusText.text = message;
        }
        
        public void UpdateWallHealth(int current, int max)
        {
            if (wallHealthText != null)
                wallHealthText.text = $"Wall: {current}/{max}";
        }
        
        public void UpdateEnemyCount(int count)
        {
            if (enemyCountText != null)
                enemyCountText.text = $"Enemies: {count}";
        }
        
        public void ShowVictory()
        {
            UpdateStatus("🎉 VICTORY!");
        }
        
        public void ShowDefeat()
        {
            UpdateStatus("☠️ DEFEAT!");
        }
    }
}
