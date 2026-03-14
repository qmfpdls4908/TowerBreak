using UnityEngine;

using TowerBreak.GameData.TowerBreaker;

namespace TowerBreak.Combat
{
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color attackColor = Color.yellow;
        [SerializeField] private Color guardColor = Color.cyan;
        [SerializeField] private Color dashColor = Color.green;
        
        // 무기 색상
        [SerializeField] private Color clawColor = new Color(1f, 0.3f, 0.3f);
        [SerializeField] private Color lanceColor = new Color(0.3f, 0.5f, 1f);
        
        private bool isAttacking = false;
        private bool isGuarding = false;
        private bool isDashing = false;
        private float actionTimer = 0f;
        private const float ACTION_DURATION = 0.3f;
        
        private WeaponRow currentWeapon;
        private GameObject weaponObject;
        private SpriteRenderer weaponSpriteRenderer;
        
        [SerializeField] private Vector3 weaponOffset = new Vector3(0.8f, 0f, 0f);
        [SerializeField] private Vector3 clawScale = new Vector3(0.8f, 0.8f, 1f);
        [SerializeField] private Vector3 lanceScale = new Vector3(1.2f, 1.2f, 1f);
        
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
            
            // 무기 위치 리셋
            if (weaponObject != null)
            {
                weaponObject.transform.localPosition = weaponOffset;
                weaponObject.transform.localRotation = Quaternion.identity;
            }
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
        
        public void SetWeapon(WeaponRow weapon)
        {
            if (weapon == null)
            {
                throw new System.ArgumentNullException(nameof(weapon));
            }
            
            currentWeapon = weapon;
            Debug.Log($"[PlayerController] Weapon equipped: {weapon.Archetype} (Attack: {weapon.BaseAttack}, Speed: {weapon.AttackSpeed})");
            
            // 무기 아케이타입별 색상 변경
            ApplyWeaponVisual(weapon);
        }
        
        public WeaponRow CurrentWeapon => currentWeapon;
        
        private void ApplyWeaponVisual(WeaponRow weapon)
        {
            if (spriteRenderer == null) return;
            
            // 무기 오브젝트 생성 또는 업데이트
            CreateOrUpdateWeaponObject(weapon);
            
            // 플레이어 색상은 기본으로
            spriteRenderer.color = normalColor;
        }
        
        private void CreateOrUpdateWeaponObject(WeaponRow weapon)
        {
            // 무기 오브젝트가 없으면 생성
            if (weaponObject == null)
            {
                weaponObject = new GameObject("Weapon");
                weaponObject.transform.SetParent(transform);
                weaponSpriteRenderer = weaponObject.AddComponent<SpriteRenderer>();
            }
            
            // 무기별 스프라이트 로드 및 설정
            string spritePath = weapon.Archetype == WeaponArchetype.Claw 
                ? "Sprites/Player/claw_player" 
                : "Sprites/Player/lance_player";
            
            Sprite weaponSprite = Resources.Load<Sprite>(spritePath);
            if (weaponSprite != null)
            {
                weaponSpriteRenderer.sprite = weaponSprite;
                Debug.Log($"[PlayerController] Loaded weapon sprite from: {spritePath}");
            }
            else
            {
                // 폴백: 기본 스프라이트
                weaponSpriteRenderer.sprite = CreateDefaultSprite();
                Debug.LogWarning($"[PlayerController] Weapon sprite not found at: {spritePath}, using default");
            }
            
            // 무기별 설정
            switch (weapon.Archetype)
            {
                case WeaponArchetype.Claw:
                    weaponSpriteRenderer.color = Color.white; // 원본 색상 유지
                    weaponObject.transform.localScale = clawScale;
                    weaponObject.transform.localPosition = new Vector3(0.6f, 0.2f, 0f);
                    Debug.Log("[PlayerController] Configured Claw weapon");
                    break;
                case WeaponArchetype.Lance:
                    weaponSpriteRenderer.color = Color.white; // 원본 색상 유지
                    weaponObject.transform.localScale = lanceScale;
                    weaponObject.transform.localPosition = new Vector3(0.8f, 0f, 0f);
                    Debug.Log("[PlayerController] Configured Lance weapon");
                    break;
            }
            
            // 플레이어 앞에 렌더링되도록 sorting order 조정
            weaponSpriteRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
        }
        
        private void OnDestroy()
        {
            // 무기 오브젝트 정리
            if (weaponObject != null)
            {
                Destroy(weaponObject);
            }
        }
    }
}
