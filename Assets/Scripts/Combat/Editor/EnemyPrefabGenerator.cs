using UnityEditor;
using UnityEngine;

namespace TowerBreak.Combat.Editor
{
    public static class EnemyPrefabGenerator
    {
        private const string PrefabOutputPath = "Assets/Prefabs/Combat/Enemies";
        private const string SpriteBasePath = "Sprites/Enemy";

        [MenuItem("TowerBreak/Generate/Create Enemy Prefabs (101-103)")]
        public static void CreateEnemyPrefabs()
        {
            // Support (ID: 101, Archetype: 2)
            CreateEnemyPrefab(
                "EnemySupport",
                new Color(0.2f, 0.8f, 0.3f), // Green
                new Vector3(1.3f, 1.3f, 1f),
                "Support monster with healing/buff abilities"
            );

            // Bomber (ID: 102, Archetype: 3)
            CreateEnemyPrefab(
                "EnemyBomber",
                new Color(1f, 0.6f, 0.1f), // Orange
                new Vector3(1.2f, 1.2f, 1f),
                "Explosive monster that deals area damage"
            );

            // BossDestroyer (ID: 103, Archetype: 4)
            CreateEnemyPrefab(
                "EnemyBossDestroyer",
                new Color(0.6f, 0.2f, 0.8f), // Purple
                new Vector3(2.5f, 2.5f, 1f),
                "Boss tier destroyer with massive damage"
            );

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[EnemyPrefabGenerator] Created enemy prefabs for IDs 101-103");
        }

        private static void CreateEnemyPrefab(string name, Color color, Vector3 scale, string description)
        {
            string prefabPath = $"{PrefabOutputPath}/{name}.prefab";

            // Check if prefab already exists
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                Debug.Log($"[EnemyPrefabGenerator] Prefab already exists: {name}");
                return;
            }

            // Create GameObject
            GameObject enemyGO = new GameObject(name);
            
            // Set tag before creating prefab
            enemyGO.tag = "Enemy";

            // Add SpriteRenderer
            SpriteRenderer spriteRenderer = enemyGO.AddComponent<SpriteRenderer>();
            spriteRenderer.color = color;
            spriteRenderer.sortingOrder = 1;

            // Try to load sprite from Resources, fallback to default
            string spritePath = $"{SpriteBasePath}/{name.ToLower()}";
            Sprite loadedSprite = Resources.Load<Sprite>(spritePath);
            if (loadedSprite != null)
            {
                spriteRenderer.sprite = loadedSprite;
                Debug.Log($"[EnemyPrefabGenerator] Loaded sprite: {spritePath}");
            }
            else
            {
                // Create default square sprite
                spriteRenderer.sprite = CreateDefaultSprite();
                Debug.Log($"[EnemyPrefabGenerator] Using default sprite for: {name} (expected at Resources/{spritePath})");
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
            enemyGO.transform.localScale = scale;

            // Save as prefab
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(enemyGO, prefabPath);
            Object.DestroyImmediate(enemyGO);

            Debug.Log($"[EnemyPrefabGenerator] Created prefab: {name} at {prefabPath}");
        }

        private static Sprite CreateDefaultSprite()
        {
            Texture2D texture = new Texture2D(32, 32);
            Color[] pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }
            texture.SetPixels(pixels);
            texture.Apply();

            return Sprite.Create(
                texture,
                new Rect(0, 0, 32, 32),
                new Vector2(0.5f, 0.5f),
                100f
            );
        }
    }
}
