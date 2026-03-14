using UnityEngine;

namespace TowerBreak.Combat
{
    public sealed class EnemyFragment : MonoBehaviour
    {
        private float lifetime = 2f;
        private float timer = 0f;
        
        public void Initialize(Vector2 force)
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.AddForce(force, ForceMode2D.Impulse);
            }
        }
        
        private void Update()
        {
            timer += Time.deltaTime;
            
            if (timer >= lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}
