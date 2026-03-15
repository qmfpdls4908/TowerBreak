using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TowerBreak.GameData.TowerBreaker;

namespace TowerBreak.Combat
{
    public sealed class PlayerController : MonoBehaviour, IPlayerActionHandler
    {
        [SerializeField] private Animator animator;
        [SerializeField] private float actionDuration = 0.3f;
        [SerializeField] private float dashDuration = 0.15f;
        [SerializeField] private float dashDistance = 2f;
        
        [Header("Weapon Display")]
        [SerializeField] private SpriteRenderer weaponSpriteRenderer;
        
        [Header("Screen Shake")]
        [SerializeField] private float shakeIntensity = 0.2f;
        [SerializeField] private float shakeDuration = 0.15f;
        
        [Header("Movement Boundary")]
        [SerializeField] private float minXPosition = -8f;  // 뒤로 밀릴 수 있는 최소 X 위치
        
        private float actionTimer = 0f;
        private WeaponRow currentWeapon;
        private List<EnemyController> touchingEnemies = new List<EnemyController>();
        private bool isAttackHeld = false;  // 공격 버튼 누르고 있는지
        
        public PlayerActionType CurrentAction { get; private set; } = PlayerActionType.None;
        public bool IsActionInProgress => CurrentAction != PlayerActionType.None;
        public bool IsTouchingWall { get; private set; } = false;
        public bool IsStunned { get; private set; } = false;  // 피격 상태
        public IReadOnlyList<EnemyController> TouchingEnemies => touchingEnemies;
        public bool IsTouchingEnemy => touchingEnemies.Count > 0;
        
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
            
            // 공격 버튼을 누르고 있으면 계속 공격
            if (isAttackHeld && !IsActionInProgress && !IsStunned)
            {
                PerformAction(PlayerActionType.Attack);
            }
            
            // 플레이어 위치 제한 (뒤로 떨어지지 않도록)
            ClampPosition();
        }
        
        private void ClampPosition()
        {
            if (transform.position.x < minXPosition)
            {
                transform.position = new Vector3(minXPosition, transform.position.y, transform.position.z);
            }
        }
        
        /// <summary>
        /// 공격 버튼 홀드 상태 설정 (BattleSceneInitializer에서 호출)
        /// </summary>
        public void SetAttackHeld(bool held)
        {
            isAttackHeld = held;
        }
        
        public bool CanPerformAction(PlayerActionType actionType)
        {
            if (actionType == PlayerActionType.None)
                return false;
            
            if (IsActionInProgress)
                return false;
            
            // 스턴 상태에서 공격 불가, 가드/대시만 가능
            if (IsStunned && actionType == PlayerActionType.Attack)
                return false;
            
            return true;
        }
        
        public bool CanAttack => !IsActionInProgress && !IsStunned;
        
        public void PerformAction(PlayerActionType actionType)
        {
            Debug.Log($"[PlayerController] PerformAction called: {actionType}, CanPerform: {CanPerformAction(actionType)}");
            
            if (!CanPerformAction(actionType))
            {
                Debug.Log($"[PlayerController] Cannot perform action: {actionType}");
                return;
            }
            
            switch (actionType)
            {
                case PlayerActionType.Attack:
                    Debug.Log("[PlayerController] Performing Attack");
                    PerformAttack();
                    break;
                case PlayerActionType.Guard:
                    PerformGuard();
                    break;
                case PlayerActionType.Dash:
                    PerformAdvance();
                    break;
            }
        }
        
        public void TakeDamage(int damage)
        {
            Debug.Log($"[Player] Took {damage} damage! Stunned!");
            IsStunned = true;
        }
        
        public void RecoverFromStun()
        {
            Debug.Log("[Player] Recovered from stun!");
            IsStunned = false;
        }
        
        private void PerformAttack()
        {
            CurrentAction = PlayerActionType.Attack;
            actionTimer = actionDuration;
            Debug.Log("[Player] Attack!");
            
            // 애니메이션 트리거 실행
            if (animator != null)
            {
                animator.SetTrigger("attack");
                Debug.Log("[Player] attack animation triggered");
            }
            else
            {
                Debug.LogError("[Player] Animator is null! Cannot play attack animation.");
            }
            
            // 화면 흔들림
            if (ScreenShake.Instance != null)
            {
                ScreenShake.Instance.Shake(shakeDuration, shakeIntensity);
                Debug.Log("[Player] Screen shake triggered");
            }
            
            // 충돌 중인 몬스터에게 데미지
            AttackTouchingEnemies();
        }
        
        private void AttackTouchingEnemies()
        {
            if (touchingEnemies.Count == 0)
            {
                Debug.Log("[Player] No enemies touching to attack");
                return;
            }
            
            // 첫 번째로 충돌한 적에게 공격
            EnemyController enemy = touchingEnemies[0];
            if (enemy != null)
            {
                int damage = GetCurrentAttackPower();
                enemy.TakeDamage(damage);
                Debug.Log($"[Player] Attack hit enemy for {damage} damage!");
            }
        }
        
        private int GetCurrentAttackPower()
        {
            if (currentWeapon != null)
            {
                return currentWeapon.BaseAttack;
            }
            return 15; // 기본 데미지
        }
        
        private void PerformGuard()
        {
            CurrentAction = PlayerActionType.Guard;
            actionTimer = actionDuration;
            Debug.Log("[Player] Guard!");
            
            // 방어 애니메이션
            if (animator != null)
            {
                animator.SetTrigger("block");
                Debug.Log("[Player] block animation triggered");
            }
            
            // 가까이 있을 때만 밀어내기 (Lerp 보간)
            if (touchingEnemies.Count > 0)
            {
                PushBackAllEnemiesAndRetreat();
            }
            else
            {
                Debug.Log("[Player] No enemies nearby to push!");
            }
            
            // 가드 시 스턴 해제
            if (IsStunned)
            {
                RecoverFromStun();
            }
        }
        
        [Header("Guard Push")]
        [SerializeField] private float guardPushDuration = 0.2f;
        [SerializeField] private float enemyPushDistance = 2f;
        [SerializeField] private float playerRetreatDistance = 1f;
        
        /// <summary>
        /// 방어: 모든 적을 뒤로(오른쪽) Lerp 이동, 플레이어도 뒤로(왼쪽) Lerp 이동
        /// </summary>
        private void PushBackAllEnemiesAndRetreat()
        {
            // 모든 적을 Lerp로 밀어냄
            EnemyController[] allEnemies = FindObjectsOfType<EnemyController>();
            foreach (var enemy in allEnemies)
            {
                if (enemy == null) continue;
                Vector3 targetPos = enemy.transform.position + Vector3.right * enemyPushDistance;
                StartCoroutine(LerpMove(enemy.transform, targetPos, guardPushDuration));
            }
            Debug.Log($"[Player] Guard pushing {allEnemies.Length} enemies with Lerp!");
            
            // 플레이어도 Lerp로 뒤로 빠짐
            Vector3 playerTarget = transform.position + Vector3.left * playerRetreatDistance;
            StartCoroutine(LerpMove(transform, playerTarget, guardPushDuration));
            Debug.Log($"[Player] Player retreating with Lerp!");
        }
        
        /// <summary>
        /// Lerp 보간으로 부드럽게 이동
        /// </summary>
        private IEnumerator LerpMove(Transform target, Vector3 destination, float duration)
        {
            Vector3 startPos = target.position;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                if (target != null)
                {
                    target.position = Vector3.Lerp(startPos, destination, t);
                }
                yield return null;
            }
            
            if (target != null)
            {
                target.position = destination;
            }
        }
        
        /// <summary>
        /// 신발 버튼: 앞으로 전진 (몬스터를 뚫고 지나갈 수 없음)
        /// 씬의 모든 적을 확인하여 앞에 있는 가장 가까운 적 앞에서 멈춤
        /// </summary>
        private void PerformAdvance()
        {
            CurrentAction = PlayerActionType.Dash;
            actionTimer = dashDuration;
            
            // 이동 애니메이션
            if (animator != null)
            {
                animator.SetTrigger("run");
                Debug.Log("[Player] run animation triggered");
            }
            
            float moveDistance = dashDistance;
            float playerX = transform.position.x;
            
            // 플레이어 콜라이더 크기 (정지 거리 계산용)
            Collider2D playerCollider = GetComponent<Collider2D>();
            float playerHalfWidth = playerCollider != null 
                ? playerCollider.bounds.extents.x 
                : 0.25f;
            
            // 씬의 모든 적 중 플레이어 앞(오른쪽)에 있는 가장 가까운 적 찾기
            EnemyController[] allEnemies = FindObjectsOfType<EnemyController>();
            float closestEnemyX = float.MaxValue;
            float closestEnemyHalfWidth = 0f;
            bool foundEnemy = false;
            
            foreach (var enemy in allEnemies)
            {
                if (enemy == null) continue;
                
                float enemyX = enemy.transform.position.x;
                
                // 플레이어보다 오른쪽에 있는 적만 체크
                if (enemyX > playerX && enemyX < closestEnemyX)
                {
                    closestEnemyX = enemyX;
                    Collider2D enemyCol = enemy.GetComponent<Collider2D>();
                    closestEnemyHalfWidth = enemyCol != null ? enemyCol.bounds.extents.x : 0.25f;
                    foundEnemy = true;
                }
            }
            
            if (foundEnemy)
            {
                // 적의 왼쪽 가장자리 - 플레이어 오른쪽 가장자리 = 이동 가능 거리
                float maxAllowedDistance = (closestEnemyX - closestEnemyHalfWidth) - (playerX + playerHalfWidth) - 0.05f;
                
                if (maxAllowedDistance <= 0)
                {
                    Debug.Log("[Player] Cannot advance - monster blocking the way!");
                    return;
                }
                
                // 이동 거리는 dashDistance와 몬스터까지 거리 중 작은 값
                float actualMove = Mathf.Min(moveDistance, maxAllowedDistance);
                transform.position += new Vector3(actualMove, 0f, 0f);
                Debug.Log($"[Player] Advanced {actualMove:F2} units (blocked by monster at x={closestEnemyX:F2})");
            }
            else
            {
                // 앞에 적이 없으면 풀 거리 이동
                transform.position += new Vector3(moveDistance, 0f, 0f);
                Debug.Log($"[Player] Advanced {moveDistance} units forward!");
            }
            
            // 전진 시 스턴 해제
            if (IsStunned)
            {
                RecoverFromStun();
            }
        }
        
        private void ResetAction()
        {
            CurrentAction = PlayerActionType.None;
        }
        
        public void SetWeapon(WeaponRow weapon)
        {
            if (weapon == null)
                throw new System.ArgumentNullException(nameof(weapon));
            
            currentWeapon = weapon;
            Debug.Log($"[PlayerController] Weapon equipped: {weapon.Archetype}");
            
            // 무기 스프라이트 변경
            if (weaponSpriteRenderer != null && !string.IsNullOrEmpty(weapon.IconKey))
            {
                Sprite weaponSprite = Resources.Load<Sprite>(weapon.IconKey);
                if (weaponSprite != null)
                {
                    weaponSpriteRenderer.sprite = weaponSprite;
                    Debug.Log($"[PlayerController] Weapon sprite set from IconKey: {weapon.IconKey}");
                }
                else
                {
                    Debug.LogWarning($"[PlayerController] Weapon sprite not found for IconKey: {weapon.IconKey}");
                }
            }
        }
        
        public WeaponRow CurrentWeapon => currentWeapon;
        
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Wall"))
            {
                IsTouchingWall = true;
                Debug.Log("[PlayerController] Touching wall");
            }
            
            // 몬스터와 충돌
            EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();
            if (enemy != null && !touchingEnemies.Contains(enemy))
            {
                touchingEnemies.Add(enemy);
                Debug.Log($"[PlayerController] Touching enemy: {enemy.gameObject.name}");
            }
        }
        
        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Wall"))
            {
                IsTouchingWall = false;
                Debug.Log("[PlayerController] Left wall");
            }
            
            // 몬스터와 분리
            EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();
            if (enemy != null && touchingEnemies.Contains(enemy))
            {
                touchingEnemies.Remove(enemy);
                Debug.Log($"[PlayerController] Left enemy: {enemy.gameObject.name}");
            }
        }
    }
}
