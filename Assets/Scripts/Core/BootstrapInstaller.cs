using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

using TowerBreak.DI;
using TowerBreak.GameData.Addressables;

namespace TowerBreak.Core
{
    public sealed class BootstrapInstaller : MonoBehaviour
    {
        [SerializeField] private bool autoLoadTitleScene = true;
        
        private void Start()
        {
            Debug.Log("[Bootstrap] Starting initialization...");
            
            // DI 컨테이너 초기화
            DIGlobalContext.Reset();
            var container = DIGlobalContext.EnsureContainer();
            
            // Addressables Provider 등록
            var loader = new UnityAddressableAssetLoader();
            var validator = new AddressableKeyValidator();
            var addressableProvider = new AddressableAssetProvider(loader, validator);
            container.Register<IAddressableAssetProvider>(addressableProvider);
            
            // Scene Loader 등록
            var sceneLoader = new SceneLoader();
            container.Register<ISceneLoader>(sceneLoader);
            
            Debug.Log("[Bootstrap] DI Container initialized successfully");
            
            // 초기화 완료 후 Title Scene 로드
            if (autoLoadTitleScene)
            {
                Debug.Log("[Bootstrap] Loading Title scene...");
                sceneLoader.LoadScene("Title");
            }
        }
    }
}
