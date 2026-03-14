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
            
            // SceneLoader 직접 생성 (DI가 없는 경우 대비)
            var sceneLoader = new SceneLoader();
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
