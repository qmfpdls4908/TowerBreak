using UnityEditor;
using UnityEngine;

using System.Collections.Generic;

using TowerBreak.GameData.TowerBreaker;

namespace TowerBreak.Combat.Editor
{
    public static class EnemyPrefabBuilder
    {
        private const string PrefabOutputPath = "Assets/Prefabs/Combat/Enemies";
        private const string GameDataPath = "Assets/GameData";

        [MenuItem("TowerBreak/Generate/Build Enemy Prefabs from GameData")]
        public static void BuildEnemyPrefabsFromGameData()
        {
            // Find TowerBreakerGameData asset
            string[] guids = AssetDatabase.FindAssets("t:TowerBreakerGameData", new[] { GameDataPath });
            if (guids.Length == 0)
            {
                Debug.LogError("[EnemyPrefabBuilder] TowerBreakerGameData not found in Assets/GameData");
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            TowerBreakerGameData gameData = AssetDatabase.LoadAssetAtPath<TowerBreakerGameData>(path);

            if (gameData?.Enemies == null || gameData.Enemies.Count == 0)
            {
                Debug.LogError("[EnemyPrefabBuilder] No enemies found in TowerBreakerGameData");
                return;
            }

            int createdCount = 0;
            int updatedCount = 0;

            foreach (EnemyRow enemy in gameData.Enemies)
            {
                if (enemy == null) continue;

                string prefabName = GetPrefabNameFromArchetype(enemy.Archetype);
                string prefabPath = $"{PrefabOutputPath}/{prefabName}.prefab";

                // Check if prefab exists
                GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                if (existingPrefab == null)
                {
                    CreateEnemyPrefab(enemy, prefabName, prefabPath);
                    createdCount++;
                }
                else
                {
                    UpdateEnemyPrefab(existingPrefab, enemy);
                    updatedCount++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[EnemyPrefabBuilder] Complete! Created: {createdCount}, Updated: {updatedCount}");
        }

        [MenuItem("TowerBreak/Generate/Create Specific Enemies (101-103)")]
        public static void CreateSpecificEnemies()
        {
            // Find TowerBreakerGameData
            string[] guids = AssetDatabase.FindAssets("t:TowerBreakerGameData", new[] { GameDataPath });
            if (guids.Length == 0)
            {
                Debug.LogError("[EnemyPrefabBuilder] TowerBreakerGameData not found");
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            TowerBreakerGameData gameData = AssetDatabase.LoadAssetAtPath<TowerBreakerGameData>(path);

            if (gameData?.Enemies == null)
            {
                Debug.LogError("[EnemyPrefabBuilder] No enemies data found");
                return;
            }

            // Find enemies with IDs 101, 102, 103
            List<EnemyRow> targetEnemies = new List<EnemyRow>();
            foreach (EnemyRow enemy in gameData.Enemies)
            {
                if (enemy.Id == 101 || enemy.Id == 102 || enemy.Id == 103)
                {
                    targetEnemies.Add(enemy);
                }
            }

            if (targetEnemies.Count == 0)
            {
                Debug.LogWarning("[EnemyPrefabBuilder] Enemies with IDs 101, 102, or 103 not found in GameData");
                Debug.Log("[EnemyPrefabBuilder] Creating placeholder prefabs with default values instead...");

                // Create placeholders
                CreatePlaceholderEnemy(101, EnemyArchetype.Support, "EnemySupport");
                CreatePlaceholderEnemy(102, EnemyArchetype.Bomber, "EnemyBomber");
                CreatePlaceholderEnemy(103, EnemyArchetype.BossDestroyer, "EnemyBossDestroyer");
            }
            else
            {
                foreach (EnemyRow enemy in targetEnemies)
                {
                    string prefabName = GetPrefabNameFromArchetype(enemy.Archetype);
                    string prefabPath = $"{PrefabOutputPath}/{prefabName}.prefab";
                    CreateEnemyPrefab(enemy, prefabName, prefabPath);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[EnemyPrefabBuilder] Created/Updated {targetEnemies.Count} enemy prefabs for IDs 101-103");
        }

        private static void CreatePlaceholderEnemy(int id, EnemyArchetype archetype, string prefabName)
        {
            EnemyRow placeholder = new EnemyRow
            {
                Id = id,
                Archetype = archetype,
                Health = archetype == EnemyArchetype.BossDestroyer ? 500 : 100,
                Pressure = archetype == EnemyArchetype.BossDestroyer ? 50f : 10f,
                MoveSpeed = archetype == EnemyArchetype.Bomber ? 4f : 3f,
                AttackCadence = 1f,
                IsArmored = archetype == EnemyArchetype.BossDestroyer,
                PrefabKey = prefabName.ToLower(),
                PortraitKey = $"portrait_{prefabName.ToLower()}",
                PushBackDistance = 2f,
                IsBoss = archetype == EnemyArchetype.BossDestroyer,
                HitVfxKey = "hit_default",
                HitSfxKey = "sfx_hit",
                DeathVfxKey = "death_default"
            };

            string prefabPath = $"{PrefabOutputPath}/{prefabName}.prefab";
            CreateEnemyPrefab(placeholder, prefabName, prefabPath);
        }

        private static void CreateEnemyPrefab(EnemyRow enemyData, string prefabName, string prefabPath)
        {
            // Create GameObject
            GameObject enemyGO = new GameObject(prefabName);
            
            // Set tag before creating prefab
            enemyGO.tag = "Enemy";

            // Configure based on archetype
            EnemyVisualConfig config = GetVisualConfig(enemyData.Archetype);

            // Add SpriteRenderer
            SpriteRenderer spriteRenderer = enemyGO.AddComponent<SpriteRenderer>();
            spriteRenderer.color = config.Color;
            spriteRenderer.sortingOrder = 1;

            // Try to load sprite from Resources
            string spritePath = $"Sprites/Enemy/{prefabName}";
            Sprite loadedSprite = Resources.Load<Sprite>(spritePath);
            if (loadedSprite != null)
            {
                spriteRenderer.sprite = loadedSprite;
            }
            else
            {
                // Use default sprite - don't create asset, just use runtime sprite
                spriteRenderer.sprite = CreateDefaultSprite(config.Color);
            }

            // Add Rigidbody2D
            Rigidbody2D rb = enemyGO.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // Add Collider2D
            BoxCollider2D collider = enemyGO.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1f, 1f);

            // Add EnemyController
            EnemyController controller = enemyGO.AddComponent<EnemyController>();

            // Set scale
            enemyGO.transform.localScale = config.Scale;

            // Save as prefab
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(enemyGO, prefabPath);
            Object.DestroyImmediate(enemyGO);

            Debug.Log($"[EnemyPrefabBuilder] Created prefab: {prefabName} (ID: {enemyData.Id}, Archetype: {enemyData.Archetype})");
        }

        private static void UpdateEnemyPrefab(GameObject prefab, EnemyRow enemyData)
        {
            // Update existing prefab with new data
            EnemyController controller = prefab.GetComponent<EnemyController>();
            if (controller != null)
            {
                // Note: Runtime initialization happens at runtime
                Debug.Log($"[EnemyPrefabBuilder] Updated prefab: {prefab.name} (ID: {enemyData.Id})");
            }
        }

        private static string GetPrefabNameFromArchetype(EnemyArchetype archetype)
        {
            return archetype switch
            {
                EnemyArchetype.BasicMelee => "EnemyBasicMelee",
                EnemyArchetype.ArmoredPusher => "EnemyArmoredPusher",
                EnemyArchetype.Support => "EnemySupport",
                EnemyArchetype.Bomber => "EnemyBomber",
                EnemyArchetype.BossDestroyer => "EnemyBossDestroyer",
                EnemyArchetype.BossOverlord => "EnemyBossOverlord",
                _ => $"Enemy{archetype}"
            };
        }

        private static EnemyVisualConfig GetVisualConfig(EnemyArchetype archetype)
        {
            return archetype switch
            {
                EnemyArchetype.BasicMelee => new EnemyVisualConfig
                {
                    Color = Color.red,
                    Scale = new Vector3(1.2f, 1.2f, 1f),
                    Description = "Basic melee attacker"
                },
                EnemyArchetype.ArmoredPusher => new EnemyVisualConfig
                {
                    Color = new Color(0.2f, 0.4f, 1f), // Blue
                    Scale = new Vector3(1.8f, 1.8f, 1f),
                    Description = "Armored pusher with high defense"
                },
                EnemyArchetype.Support => new EnemyVisualConfig
                {
                    Color = new Color(0.2f, 0.8f, 0.3f), // Green
                    Scale = new Vector3(1.3f, 1.3f, 1f),
                    Description = "Support unit with healing abilities"
                },
                EnemyArchetype.Bomber => new EnemyVisualConfig
                {
                    Color = new Color(1f, 0.6f, 0.1f), // Orange
                    Scale = new Vector3(1.2f, 1.2f, 1f),
                    Description = "Explosive unit dealing area damage"
                },
                EnemyArchetype.BossDestroyer => new EnemyVisualConfig
                {
                    Color = new Color(0.6f, 0.2f, 0.8f), // Purple
                    Scale = new Vector3(2.5f, 2.5f, 1f),
                    Description = "Boss tier destroyer"
                },
                EnemyArchetype.BossOverlord => new EnemyVisualConfig
                {
                    Color = new Color(0.1f, 0.1f, 0.1f), // Dark
                    Scale = new Vector3(3f, 3f, 1f),
                    Description = "Final boss overlord"
                },
                _ => new EnemyVisualConfig
                {
                    Color = Color.gray,
                    Scale = Vector3.one,
                    Description = "Unknown enemy type"
                }
            };
        }

        private static Sprite CreateDefaultSprite(Color color)
        {
            Texture2D texture = new Texture2D(64, 64);
            Color[] pixels = new Color[64 * 64];

            // Create border effect
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    if (x < 4 || x >= 60 || y < 4 || y >= 60)
                    {
                        pixels[y * 64 + x] = Color.Lerp(color, Color.black, 0.3f);
                    }
                    else
                    {
                        pixels[y * 64 + x] = color;
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return Sprite.Create(
                texture,
                new Rect(0, 0, 64, 64),
                new Vector2(0.5f, 0.5f),
                100f
            );
        }

        private struct EnemyVisualConfig
        {
            public Color Color;
            public Vector3 Scale;
            public string Description;
        }
    }
}
