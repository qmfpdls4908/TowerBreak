# Addressables Provider References

## Reference Documents

- `docs/plans/2026-03-13-tower-breaker-vertical-slice/00-overview.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/01-foundation-and-bootstrap.md`
- `docs/plans/2026-03-13-unity-excel-gamedata-converter.md`
- `docs/plans/2026-03-13-implementation-workflow.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/archive/data-schema/README.md`

## Existing Code References

- `Packages/manifest.json`
- `Assets/Scripts/DI/DIContainer.cs`
- `Assets/Scripts/DI/DIInstaller.cs`
- `Assets/Scripts/DI/DIGlobalInstaller.cs`
- `Assets/Scripts/DI/DIGlobalContext.cs`
- `Assets/Scripts/GameData/TowerBreaker/FloorRow.cs`
- `Assets/Scripts/GameData/TowerBreaker/EnemyRow.cs`
- `Assets/Scripts/GameData/TowerBreaker/WeaponRow.cs`

## New Work In This Milestone

- `Assets/Scripts/GameData/Addressables/IAddressableAssetProvider.cs`
- `Assets/Scripts/GameData/Addressables/IAddressableAssetLoader.cs`
- `Assets/Scripts/GameData/Addressables/AddressableAssetProvider.cs`
- `Assets/Scripts/GameData/Addressables/AddressableKeyValidator.cs`
- `Assets/Scripts/GameData/Addressables/UnityAddressableAssetLoader.cs`
- `Assets/Scripts/GameData/Addressables/AddressablesGlobalInstaller.cs`
- `Assets/Scripts/DI/DIGlobalInstaller.cs`
- `Assets/Scripts/DI/DIGlobalContext.cs`
- `Assets/Tests/EditMode/GameData/Addressables/AddressableAssetProviderTests.cs`
- `Assets/Tests/EditMode/GameData/Addressables/AddressablesGlobalInstallerTests.cs`
- `Assets/Tests/EditMode/DI/DIGlobalInstallerTests.cs`
