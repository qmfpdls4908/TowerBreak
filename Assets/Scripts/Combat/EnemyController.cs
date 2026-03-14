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
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
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
            Destroy(gameObject);
        }

        private void MoveLeft()
        {
            if (enemyData == null) return;
            
            float moveDistance = enemyData.MoveSpeed * Time.deltaTime;
            transform.Translate(Vector3.left * moveDistance);
        }
    }
}
