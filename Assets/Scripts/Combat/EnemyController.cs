using System;
using UnityEngine;

using TowerBreak.GameData.TowerBreaker;

namespace TowerBreak.Combat
{
    public sealed class EnemyController : MonoBehaviour
    {
        private EnemyRow enemyData;
        private SpriteRenderer spriteRenderer;
        private bool isFlashing = false;
        private float flashTimer = 0f;
        private const float FLASH_DURATION = 0.2f;
        private Color originalColor;

        public void Initialize(EnemyRow data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            enemyData = data;
            
            // 적 종류별 색상 설정
            if (spriteRenderer != null)
            {
                switch (data.Archetype)
                {
                    case EnemyArchetype.BasicMelee:
                        spriteRenderer.color = Color.red;
                        Debug.Log($"[EnemyController] Initialized BasicMelee (Red) - HP: {data.Health}");
                        break;
                    case EnemyArchetype.ArmoredPusher:
                        spriteRenderer.color = Color.blue;
                        Debug.Log($"[EnemyController] Initialized ArmoredPusher (Blue) - HP: {data.Health}");
                        break;
                    default:
                        spriteRenderer.color = Color.gray;
                        Debug.Log($"[EnemyController] Initialized Unknown (Gray) - HP: {data.Health}");
                        break;
                }
            }
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
            
            // 스프라이트가 없으면 기본 스프라이트 설정
            if (spriteRenderer.sprite == null)
            {
                // Unity 기본 스프라이트 사용
                spriteRenderer.sprite = Sprite.Create(
                    Texture2D.whiteTexture,
                    new Rect(0, 0, 1, 1),
                    new Vector2(0.5f, 0.5f),
                    100f
                );
                Debug.Log("[EnemyController] Created default white sprite");
            }
        }

        private void Start()
        {
            originalColor = spriteRenderer.color;
        }

        public void TakeDamage(int damage)
        {
            if (enemyData == null) return;
            
            enemyData.Health -= damage;
            
            if (enemyData.Health <= 0)
            {
                Die();
            }
            else
            {
                StartFlash();
            }
        }

        private void StartFlash()
        {
            isFlashing = true;
            flashTimer = 0f;
            spriteRenderer.color = Color.red;
        }

        private void Update()
        {
            MoveLeft();
            UpdateFlash();
            HandleDebugInput();
        }

        private void HandleDebugInput()
        {
#if UNITY_EDITOR
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null && keyboard.kKey.wasPressedThisFrame)
            {
                // For testing: instantly kill the enemy
                if (enemyData != null)
                {
                    TakeDamage(enemyData.Health);
                }
            }
#endif
        }

        private void UpdateFlash()
        {
            if (!isFlashing) return;
            
            flashTimer += Time.deltaTime;
            
            if (flashTimer >= FLASH_DURATION)
            {
                isFlashing = false;
                spriteRenderer.color = originalColor;
            }
        }

        private void Die()
        {
            CreateFragments();
            Destroy(gameObject);
        }

        private void CreateFragments()
        {
            if (spriteRenderer.sprite == null) return;
            
            // Create 4 fragments
            for (int i = 0; i < 4; i++)
            {
                CreateFragment(i);
            }
        }

        private void CreateFragment(int index)
        {
            GameObject fragment = new GameObject($"Fragment_{index}");
            fragment.transform.position = transform.position;
            fragment.transform.SetParent(transform);
            
            // Add SpriteRenderer with portion of original sprite
            SpriteRenderer fragRenderer = fragment.AddComponent<SpriteRenderer>();
            fragRenderer.sprite = spriteRenderer.sprite;
            fragRenderer.color = spriteRenderer.color;
            
            // Add Rigidbody2D for physics
            Rigidbody2D rb = fragment.AddComponent<Rigidbody2D>();
            
            // Add Fragment script
            EnemyFragment fragScript = fragment.AddComponent<EnemyFragment>();
            
            // Apply random force
            Vector2 randomDirection = new Vector2(
                UnityEngine.Random.Range(-1f, 1f),
                UnityEngine.Random.Range(-1f, 1f)
            ).normalized;
            
            float randomForce = UnityEngine.Random.Range(3f, 8f);
            fragScript.Initialize(randomDirection * randomForce);
        }

        private void MoveLeft()
        {
            if (enemyData == null) return;
            
            float moveDistance = enemyData.MoveSpeed * Time.deltaTime;
            transform.Translate(Vector3.left * moveDistance);
        }
    }
}
