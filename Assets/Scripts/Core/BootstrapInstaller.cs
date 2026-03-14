using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

using TowerBreak.DI;
using TowerBreak.GameData.Addressables;
using TowerBreak.Meta.State;

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
            
            // Player State 등록
            var walletState = new PlayerWalletState();
            var inventoryState = new PlayerInventoryState();
            container.Register<PlayerWalletState>(walletState);
            container.Register<PlayerInventoryState>(inventoryState);
            
            // 테스트용: 기본 무기 장착 (Claw, ID: 1)
            SetupDefaultEquipment(inventoryState);
            
            Debug.Log("[Bootstrap] DI Container initialized successfully");
            
            // 초기화 완료 후 Title Scene 로드
            if (autoLoadTitleScene)
            {
                Debug.Log("[Bootstrap] Loading Title scene...");
                sceneLoader.LoadScene("Title");
            }
        }
        
        private void SetupDefaultEquipment(PlayerInventoryState inventory)
        {
            // 테스트용 기본 무기 추가 및 장착
            // Claw (WeaponId: 201), Lance (WeaponId: 202) 중 Claw 장착
            var defaultWeapon = new OwnedEquipment(instanceId: 1, weaponId: 201);
            inventory.AddEquipment(defaultWeapon);
            inventory.EquipWeapon(defaultWeapon.InstanceId);
            
            Debug.Log("[Bootstrap] Default equipment setup: Claw equipped");
        }
    }
}
