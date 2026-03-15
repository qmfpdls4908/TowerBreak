using UnityEngine;
using System.Collections.Generic;
using TowerBreak.GameData.TowerBreaker;

namespace TowerBreak.Combat
{
    public sealed class PlayerController : MonoBehaviour, IPlayerActionHandler
    {
        [SerializeField] private float actionDuration = 0.3f;
        [SerializeField] private float dashDuration = 0.15f;
        [SerializeField] private float dashDistance = 2f;
        
        private float actionTimer = 0f;
        private WeaponRow currentWeapon;
        private List<EnemyController> touchingEnemies = new List<EnemyController>();
        
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
        
        public void TakeDamage(int damage)
        {
            Debug.Log($"[Player] Took {damage} damage! Stunned!");
            IsStunned = true;
            
            // TODO: 피격 애니메이션
            // GetComponent<Animator>()?.SetTrigger("Hit");
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
            
            // 충돌 중인 몬스터에게 데미지
            AttackTouchingEnemies();
            
            // TODO: 애니메이션 트리거 호출
            // GetComponent<Animator>()?.SetTrigger("Attack");
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
            
            // 가드 시 스턴 해제
            if (IsStunned)
            {
                RecoverFromStun();
            }
            
            // TODO: 애니메이션 트리거 호출
            // GetComponent<Animator>()?.SetTrigger("Guard");
        }
        
        private void PerformDash()
        {
            CurrentAction = PlayerActionType.Dash;
            actionTimer = dashDuration;
            Vector3 dashPosition = transform.position + new Vector3(dashDistance, 0f, 0f);
            transform.position = dashPosition;
            Debug.Log("[Player] Dash!");
            
            // 대시 시 스턴 해제
            if (IsStunned)
            {
                RecoverFromStun();
            }
            
            // TODO: 애니메이션 트리거 호출
            // GetComponent<Animator>()?.SetTrigger("Dash");
        }
        
        private void ResetAction()
        {
            CurrentAction = PlayerActionType.None;
            
            // TODO: 애니메이션 리셋
            // GetComponent<Animator>()?.ResetTrigger("Attack");
            // GetComponent<Animator>()?.ResetTrigger("Guard");
            // GetComponent<Animator>()?.ResetTrigger("Dash");
        }
        
        public void SetWeapon(WeaponRow weapon)
        {
            if (weapon == null)
                throw new System.ArgumentNullException(nameof(weapon));
            
            currentWeapon = weapon;
            Debug.Log($"[PlayerController] Weapon equipped: {weapon.Archetype}");
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
