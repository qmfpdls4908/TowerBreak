using UnityEngine;

namespace TowerBreak.Combat
{
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color attackColor = Color.yellow;
        [SerializeField] private Color guardColor = Color.cyan;
        [SerializeField] private Color dashColor = Color.green;
        
        private bool isAttacking = false;
        private bool isGuarding = false;
        private bool isDashing = false;
        private float actionTimer = 0f;
        private const float ACTION_DURATION = 0.3f;
        
        private void Awake()
        {
            // SpriteRenderer 확인/추가
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = CreateDefaultSprite();
                Debug.Log("[PlayerController] Added SpriteRenderer");
            }
            
            // 기본 색상 설정
            spriteRenderer.color = normalColor;
            
            // 플레이어 위치 설정 (왼쪽 벽 근처)
            transform.position = new Vector3(-6f, 0f, 0f);
            transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        }
        
        private void Update()
        {
            // 액션 타이머 업데이트
            if (actionTimer > 0)
            {
                actionTimer -= Time.deltaTime;
                if (actionTimer <= 0)
                {
                    ResetAction();
                }
            }
        }
        
        public void PerformAttack()
        {
            if (isAttacking || isGuarding || isDashing) return;
            
            isAttacking = true;
            actionTimer = ACTION_DURATION;
            spriteRenderer.color = attackColor;
            
            // 공격 애니메이션 (scale 효과)
            transform.localScale = new Vector3(2f, 1.5f, 1f);
            
            Debug.Log("[Player] Attack!");
        }
        
        public void PerformGuard()
        {
            if (isAttacking || isGuarding || isDashing) return;
            
            isGuarding = true;
            actionTimer = ACTION_DURATION;
            spriteRenderer.color = guardColor;
            
            // 가드 자세 (y scale 줄임)
            transform.localScale = new Vector3(1.5f, 1f, 1f);
            
            Debug.Log("[Player] Guard!");
        }
        
        public void PerformDash()
        {
            if (isAttacking || isGuarding || isDashing) return;
            
            isDashing = true;
            actionTimer = ACTION_DURATION * 0.5f;
            spriteRenderer.color = dashColor;
            
            // 대시 이동
            Vector3 dashPosition = transform.position + new Vector3(2f, 0f, 0f);
            transform.position = dashPosition;
            
            Debug.Log("[Player] Dash!");
        }
        
        private void ResetAction()
        {
            isAttacking = false;
            isGuarding = false;
            isDashing = false;
            
            spriteRenderer.color = normalColor;
            transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        }
        
        private Sprite CreateDefaultSprite()
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            
            return Sprite.Create(
                texture,
                new Rect(0, 0, 1, 1),
                new Vector2(0.5f, 0.5f),
                100f
            );
        }
        
        public bool IsAttacking => isAttacking;
        public bool IsGuarding => isGuarding;
        public bool IsDashing => isDashing;
    }
}
