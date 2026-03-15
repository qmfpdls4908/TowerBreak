using UnityEngine;
using TowerBreak.GameData.TowerBreaker;

namespace TowerBreak.Combat
{
    public sealed class PlayerController : MonoBehaviour, IPlayerActionHandler
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color attackColor = Color.yellow;
        [SerializeField] private Color guardColor = Color.cyan;
        [SerializeField] private Color dashColor = Color.green;
        
        [SerializeField] private Color clawColor = new Color(1f, 0.3f, 0.3f);
        [SerializeField] private Color lanceColor = new Color(0.3f, 0.5f, 1f);
        
        private float actionTimer = 0f;
        private const float ACTION_DURATION = 0.3f;
        private const float DASH_DURATION = 0.15f;
        
        private WeaponRow currentWeapon;
        private GameObject weaponObject;
        private SpriteRenderer weaponSpriteRenderer;
        
        [SerializeField] private Vector3 weaponOffset = new Vector3(0.8f, 0f, 0f);
        [SerializeField] private Vector3 clawScale = new Vector3(0.8f, 0.8f, 1f);
        [SerializeField] private Vector3 lanceScale = new Vector3(1.2f, 1.2f, 1f);
        
        public PlayerActionType CurrentAction { get; private set; } = PlayerActionType.None;
        public bool IsActionInProgress => CurrentAction != PlayerActionType.None;
        public bool IsTouchingWall { get; private set; } = false;
        
        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = CreateDefaultSprite();
            }
            
            spriteRenderer.color = normalColor;
            transform.position = new Vector3(-6f, 0f, 0f);
            transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        }
        
        private void Update()
        {
            if (actionTimer > 0)
            {
                actionTimer -= Time.deltaTime;
                if (actionTimer <= 0)
                {
                    ResetAction();
                }
            }
        }
        
        public bool CanPerformAction(PlayerActionType actionType)
        {
            if (actionType == PlayerActionType.None)
                return false;
            
            return !IsActionInProgress;
        }
        
        public void PerformAction(PlayerActionType actionType)
        {
            if (!CanPerformAction(actionType))
                return;
            
            switch (actionType)
            {
                case PlayerActionType.Attack:
                    PerformAttack();
                    break;
                case PlayerActionType.Guard:
                    PerformGuard();
                    break;
                case PlayerActionType.Dash:
                    PerformDash();
                    break;
            }
        }
        
        private void PerformAttack()
        {
            CurrentAction = PlayerActionType.Attack;
            actionTimer = ACTION_DURATION;
            spriteRenderer.color = attackColor;
            transform.localScale = new Vector3(2f, 1.5f, 1f);
            Debug.Log("[Player] Attack!");
        }
        
        private void PerformGuard()
        {
            CurrentAction = PlayerActionType.Guard;
            actionTimer = ACTION_DURATION;
            spriteRenderer.color = guardColor;
            transform.localScale = new Vector3(1.5f, 1f, 1f);
            Debug.Log("[Player] Guard!");
        }
        
        private void PerformDash()
        {
            CurrentAction = PlayerActionType.Dash;
            actionTimer = DASH_DURATION;
            spriteRenderer.color = dashColor;
            Vector3 dashPosition = transform.position + new Vector3(2f, 0f, 0f);
            transform.position = dashPosition;
            Debug.Log("[Player] Dash!");
        }
        
        private void ResetAction()
        {
            CurrentAction = PlayerActionType.None;
            spriteRenderer.color = normalColor;
            transform.localScale = new Vector3(1.5f, 1.5f, 1f);
            
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
        
        public void SetWeapon(WeaponRow weapon)
        {
            if (weapon == null)
                throw new System.ArgumentNullException(nameof(weapon));
            
            currentWeapon = weapon;
            ApplyWeaponVisual(weapon);
        }
        
        public WeaponRow CurrentWeapon => currentWeapon;
        
        private void ApplyWeaponVisual(WeaponRow weapon)
        {
            if (spriteRenderer == null) return;
            CreateOrUpdateWeaponObject(weapon);
            spriteRenderer.color = normalColor;
        }
        
        private void CreateOrUpdateWeaponObject(WeaponRow weapon)
        {
            if (weaponObject == null)
            {
                weaponObject = new GameObject("Weapon");
                weaponObject.transform.SetParent(transform);
                weaponSpriteRenderer = weaponObject.AddComponent<SpriteRenderer>();
            }
            
            string spritePath = weapon.Archetype == WeaponArchetype.Claw 
                ? "Sprites/Player/claw_player" 
                : "Sprites/Player/lance_player";
            
            Sprite weaponSprite = Resources.Load<Sprite>(spritePath);
            weaponSpriteRenderer.sprite = weaponSprite != null ? weaponSprite : CreateDefaultSprite();
            
            switch (weapon.Archetype)
            {
                case WeaponArchetype.Claw:
                    weaponSpriteRenderer.color = Color.white;
                    weaponObject.transform.localScale = clawScale;
                    weaponObject.transform.localPosition = new Vector3(0.6f, 0.2f, 0f);
                    break;
                case WeaponArchetype.Lance:
                    weaponSpriteRenderer.color = Color.white;
                    weaponObject.transform.localScale = lanceScale;
                    weaponObject.transform.localPosition = new Vector3(0.8f, 0f, 0f);
                    break;
            }
            
            weaponSpriteRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
        }
        
        private void OnDestroy()
        {
            if (weaponObject != null)
            {
                Destroy(weaponObject);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Wall"))
            {
                IsTouchingWall = true;
                Debug.Log("[PlayerController] Touching wall");
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Wall"))
            {
                IsTouchingWall = false;
                Debug.Log("[PlayerController] Left wall");
            }
        }
    }
}
