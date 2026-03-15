using UnityEngine;
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
        
        public PlayerActionType CurrentAction { get; private set; } = PlayerActionType.None;
        public bool IsActionInProgress => CurrentAction != PlayerActionType.None;
        public bool IsTouchingWall { get; private set; } = false;
        
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
            actionTimer = actionDuration;
            Debug.Log("[Player] Attack!");
            
            // TODO: 애니메이션 트리거 호출
            // GetComponent<Animator>()?.SetTrigger("Attack");
        }
        
        private void PerformGuard()
        {
            CurrentAction = PlayerActionType.Guard;
            actionTimer = actionDuration;
            Debug.Log("[Player] Guard!");
            
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
    }
}
