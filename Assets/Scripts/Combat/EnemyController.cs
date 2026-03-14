using System;
using UnityEngine;

using TowerBreak.GameData.TowerBreaker;

namespace TowerBreak.Combat
{
    public sealed class EnemyController : MonoBehaviour
    {
        private EnemyRow enemyData;
        private SpriteRenderer spriteRenderer;

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

        private void Update()
        {
            MoveLeft();
        }

        private void MoveLeft()
        {
            if (enemyData == null) return;
            
            float moveDistance = enemyData.MoveSpeed * Time.deltaTime;
            transform.Translate(Vector3.left * moveDistance);
        }
    }
}
