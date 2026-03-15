using System;
using System.Collections.Generic;
using UnityEngine;

using TowerBreak.GameData.TowerBreaker;
using TowerBreak.EventBus;
using TowerBreak.DI;

namespace TowerBreak.Combat
{
    public sealed class EnemyController : MonoBehaviour
    {
        private EnemyRow enemyData;
        private int currentHealth;  // 각 몬스터별 개별 체력
        private SpriteRenderer spriteRenderer;
        private bool isFlashing = false;
        private float flashTimer = 0f;
        private const float FLASH_DURATION = 0.2f;
        private Dictionary<SpriteRenderer, Color> originalColors = new Dictionary<SpriteRenderer, Color>();
        private bool isTouchingPlayer = false;
        private bool isStopped = false;
        private EventBus<PlayerActionEvent> playerActionEventBus;

        public int EnemyId { get; private set; }
        public int CurrentHealth => currentHealth;

        [Header("Body Parts")]
        [SerializeField] private SpriteRenderer headRenderer;
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private SpriteRenderer leftLegRenderer;
        [SerializeField] private SpriteRenderer rightLegRenderer;
        [SerializeField] private SpriteRenderer swordRenderer;

        private List<SpriteRenderer> allRenderers = new List<SpriteRenderer>();

        public void Initialize(EnemyRow data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            enemyData = data;
            EnemyId = data.Id;
            currentHealth = data.Health;  // 각 몬스터별 개별 체력 초기화
            
            // 적 종류별 색상 설정
            if (spriteRenderer != null)
            {
                switch (data.Archetype)
                {
                    case EnemyArchetype.BasicMelee:
                        spriteRenderer.color = Color.red;
                        Debug.Log($"[EnemyController] Initialized BasicMelee (Red) - HP: {currentHealth}");
                        break;
                    case EnemyArchetype.ArmoredPusher:
                        spriteRenderer.color = Color.blue;
                        Debug.Log($"[EnemyController] Initialized ArmoredPusher (Blue) - HP: {currentHealth}");
                        break;
                    default:
                        spriteRenderer.color = Color.gray;
                        Debug.Log($"[EnemyController] Initialized Unknown (Gray) - HP: {currentHealth}");
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

            // Collect all body part renderers
            CollectBodyPartRenderers();
        }

        private void CollectBodyPartRenderers()
        {
            allRenderers.Clear();

            // Add main renderer
            if (spriteRenderer != null)
            {
                allRenderers.Add(spriteRenderer);
            }

            // Add body part renderers if assigned
            if (headRenderer != null) allRenderers.Add(headRenderer);
            if (bodyRenderer != null) allRenderers.Add(bodyRenderer);
            if (leftLegRenderer != null) allRenderers.Add(leftLegRenderer);
            if (rightLegRenderer != null) allRenderers.Add(rightLegRenderer);
            if (swordRenderer != null) allRenderers.Add(swordRenderer);

            // Auto-find renderers in children if not assigned
            if (allRenderers.Count <= 1)
            {
                SpriteRenderer[] childRenderers = GetComponentsInChildren<SpriteRenderer>();
                foreach (SpriteRenderer renderer in childRenderers)
                {
                    if (renderer != spriteRenderer && !allRenderers.Contains(renderer))
                    {
                        allRenderers.Add(renderer);
                    }
                }
            }

            Debug.Log($"[EnemyController] Collected {allRenderers.Count} renderers for flash effect");
        }

        private void Start()
        {
            // EventBus 인스턴스를 DI에서 가져오기
            playerActionEventBus = DIContainer.ResolveFromRegistered<EventBus<PlayerActionEvent>>();
            
            if (playerActionEventBus != null)
            {
                // 가드 이벤트 구독
                playerActionEventBus.Subscribe(OnPlayerAction);
            }

            // CombatManager Singleton 확인
            if (CombatManager.Instance == null)
            {
                Debug.LogWarning("[EnemyController] CombatManager not found! Creating one...");
                GameObject combatManagerGO = new GameObject("CombatManager");
                combatManagerGO.AddComponent<CombatManager>();
            }
        }

        private void OnDestroy()
        {
            // 가드 이벤트 구독 취소
            if (playerActionEventBus != null)
            {
                playerActionEventBus.Unsubscribe(OnPlayerAction);
            }
        }

        private void OnPlayerAction(PlayerActionEvent evt)
        {
            // 플레이어가 가드를 사용하고, 몬스터가 플레이어와 닿아있을 때 밀림
            if (isTouchingPlayer)
            {
                PushBack();
            }
        }

        private void PushBack()
        {
            if (enemyData == null) return;
            
            // 오른쪽으로 밀림
            float pushDistance = enemyData.PushBackDistance;
            transform.Translate(Vector3.right * pushDistance);
            
            Debug.Log($"[EnemyController] Pushed back by {pushDistance}");
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // 플레이어와 충돌 시작
            if (collision.gameObject.CompareTag("Player"))
            {
                isTouchingPlayer = true;
                isStopped = false;
                
                // CombatManager에게 몬스터가 밀기 시작했음을 알림
                if (CombatManager.Instance != null)
                {
                    CombatManager.Instance.SetMonsterPushing(true);
                }
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            // 플레이어와 충돌 종료
            if (collision.gameObject.CompareTag("Player"))
            {
                isTouchingPlayer = false;
                
                // CombatManager에게 몬스터가 밀기를 멈췄음을 알림
                if (CombatManager.Instance != null)
                {
                    CombatManager.Instance.SetMonsterPushing(false);
                }
            }
        }

        public void StopPushing()
        {
            isStopped = true;
            Debug.Log("[EnemyController] Stopped by CombatManager");
        }
        
        public void ResumeMoving()
        {
            isStopped = false;
            Debug.Log("[EnemyController] Resumed moving");
        }

        public void TakeDamage(int damage)
        {
            if (enemyData == null) return;
            
            currentHealth -= damage;
            Debug.Log($"[EnemyController] {gameObject.name} took {damage} damage. Remaining HP: {currentHealth}");
            
            // 데미지 팝업 표시
            DamagePopup.Create(transform.position, damage);
            
            if (currentHealth <= 0)
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
            if (isFlashing) return; // Prevent multiple flashes

            isFlashing = true;
            flashTimer = 0f;

            // Save current colors before flashing
            originalColors.Clear();
            foreach (SpriteRenderer renderer in allRenderers)
            {
                if (renderer != null)
                {
                    originalColors[renderer] = renderer.color;
                    renderer.color = Color.red;
                }
            }

            Debug.Log("[EnemyController] Flash effect started - saved original colors");
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

                // Restore original colors for each body part
                foreach (var kvp in originalColors)
                {
                    SpriteRenderer renderer = kvp.Key;
                    Color originalColor = kvp.Value;
                    if (renderer != null)
                    {
                        renderer.color = originalColor;
                    }
                }
                originalColors.Clear();

                Debug.Log("[EnemyController] Flash effect ended - restored original colors");
            }
        }

        private void Die()
        {
            // 이벤트 발행 - 자신의 GameObject 참조 포함
            var eventBus = DI.DIContainer.ResolveFromRegistered<EventBus<EnemyDeathEvent>>();
            if (eventBus != null)
            {
                var deathEvent = new EnemyDeathEvent(EnemyId, 1, gameObject);
                eventBus.Publish(deathEvent);
                Debug.Log($"[EnemyController] Published death event for Enemy {EnemyId}");
            }
            
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

            // CombatManager에서 멈춤 명령을 받았으면 이동하지 않음
            if (isStopped)
            {
                return;
            }

            float moveDistance = enemyData.MoveSpeed * Time.deltaTime;

            if (isTouchingPlayer)
            {
                // 플레이어와 충돌 중이면 플레이어를 밀고 함께 이동
                MoveAndPushPlayer(moveDistance);
            }
            else
            {
                // 평상시 왼쪽으로 이동
                transform.Translate(Vector3.left * moveDistance);
            }
        }

        private void MoveAndPushPlayer(float moveDistance)
        {
            // CombatManager에서 멈춤 명령을 받았으면 이동하지 않음
            if (isStopped)
            {
                return;
            }

            // 플레이어 찾기
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                // 플레이어가 가드 중이면 밀지 않음 (몬스터만 이동)
                var playerCtrl = player.GetComponent<PlayerController>();
                if (playerCtrl != null && playerCtrl.CurrentAction == PlayerActionType.Guard)
                {
                    // 가드 중: 몬스터는 제자리, 플레이어를 밀지 않음
                    return;
                }
                
                // 플레이어를 밀기 전에 몬스터 이동
                transform.Translate(Vector3.left * moveDistance);

                // 플레이어를 몬스터와 함께 왼쪽으로 밀기
                float playerMoveDistance = moveDistance * 0.8f;
                player.transform.Translate(Vector3.left * playerMoveDistance);
            }
            else
            {
                // 플레이어가 없으면 그냥 이동
                transform.Translate(Vector3.left * moveDistance);
            }
        }

        // Editor 테스트용: 초기화 여부 확인
        public bool IsInitialized()
        {
            return enemyData != null;
        }

        // 현재 체력 반환
        public int GetCurrentHealth()
        {
            return currentHealth;
        }

        // Body Part Renderers - public for Editor access
        public SpriteRenderer HeadRenderer => headRenderer;
        public SpriteRenderer BodyRenderer => bodyRenderer;
        public SpriteRenderer LeftLegRenderer => leftLegRenderer;
        public SpriteRenderer RightLegRenderer => rightLegRenderer;
        public SpriteRenderer SwordRenderer => swordRenderer;

        // Set color for all body parts (public for Editor)
        public void SetBodyPartsColor(Color color)
        {
            foreach (SpriteRenderer renderer in allRenderers)
            {
                if (renderer != null)
                {
                    renderer.color = color;
                }
            }
        }

        // Re-collect body parts (call after adding new parts in Editor)
        public void RefreshBodyParts()
        {
            CollectBodyPartRenderers();
        }
    }
}
