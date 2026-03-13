using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;

using UnityEngine;

namespace TowerBreak.Combat.Editor
{
    public static class CombatAddressableGroupSetup
    {
        private const string PrefabFolder = "Assets/Prefabs/Combat/Enemies";
        private const string BasicMeleePrefabPath = PrefabFolder + "/EnemyBasicMelee.prefab";
        private const string BasicMeleeAddress = "enemy/basic_melee";
        private const string ArmoredPusherPrefabPath = PrefabFolder + "/EnemyArmoredPusher.prefab";
        private const string ArmoredPusherAddress = "enemy/armored_pusher";
        private const string GroupName = "Combat";

        [MenuItem("TowerBreak/Setup/Author Combat Enemy Addressables")]
        public static void AuthorCombatEnemyAddressables()
        {
            EnsureFolderExists("Assets/Prefabs", "Combat");
            EnsureFolderExists("Assets/Prefabs/Combat", "Enemies");

            CreateOrUpdateBasicMeleePrefab();
            CreateOrUpdateArmoredPusherPrefab();
            RegisterWithAddressables();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[CombatSetup] Done. Registered: " + BasicMeleeAddress + ", " + ArmoredPusherAddress);
        }

        private static void EnsureFolderExists(string parent, string folderName)
        {
            string fullPath = parent + "/" + folderName;
            if (!AssetDatabase.IsValidFolder(fullPath))
            {
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }

        private static void CreateOrUpdateBasicMeleePrefab()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(BasicMeleePrefabPath) != null)
            {
                Debug.Log("[CombatSetup] Prefab already exists at " + BasicMeleePrefabPath);
                return;
            }

            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = "EnemyBasicMelee";
            go.transform.localScale = new Vector3(1.5f, 1.5f, 1f);

            PrefabUtility.SaveAsPrefabAsset(go, BasicMeleePrefabPath);
            Object.DestroyImmediate(go);

            Debug.Log("[CombatSetup] Created placeholder prefab at " + BasicMeleePrefabPath);
        }

        private static void CreateOrUpdateArmoredPusherPrefab()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(ArmoredPusherPrefabPath) != null)
            {
                Debug.Log("[CombatSetup] Prefab already exists at " + ArmoredPusherPrefabPath);
                return;
            }

            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "EnemyArmoredPusher";
            go.transform.localScale = new Vector3(2f, 2f, 1f);

            PrefabUtility.SaveAsPrefabAsset(go, ArmoredPusherPrefabPath);
            Object.DestroyImmediate(go);

            Debug.Log("[CombatSetup] Created placeholder prefab at " + ArmoredPusherPrefabPath);
        }

        private static void RegisterWithAddressables()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            if (settings == null)
            {
                Debug.LogWarning("[CombatSetup] Could not obtain Addressable settings. Open Window > Asset Management > Addressables > Groups to initialize, then run this menu item again.");
                return;
            }

            AddressableAssetGroup group = settings.FindGroup(GroupName);
            if (group == null)
            {
                group = settings.CreateGroup(GroupName, false, false, true, null, typeof(BundledAssetGroupSchema));
                Debug.Log("[CombatSetup] Created Addressables group: " + GroupName);
            }

            RegisterEntry(settings, group, BasicMeleePrefabPath, BasicMeleeAddress);
            RegisterEntry(settings, group, ArmoredPusherPrefabPath, ArmoredPusherAddress);
        }

        private static void RegisterEntry(AddressableAssetSettings settings, AddressableAssetGroup group, string prefabPath, string address)
        {
            string guid = AssetDatabase.AssetPathToGUID(prefabPath);
            if (string.IsNullOrEmpty(guid))
            {
                Debug.LogWarning("[CombatSetup] Could not resolve GUID for " + prefabPath);
                return;
            }

            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group);
            entry.address = address;

            Debug.Log("[CombatSetup] Registered " + prefabPath + " with address '" + address + "' in group '" + GroupName + "'.");
        }
    }
}
