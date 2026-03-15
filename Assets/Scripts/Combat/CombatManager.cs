using System;
using UnityEngine;

using TowerBreak.GameData.TowerBreaker;
using TowerBreak.EventBus;
using TowerBreak.DI;

namespace TowerBreak.Combat
{
    public sealed class CombatManager : MonoBehaviour
    {
        public static CombatManager Instance { get; private set; }

        [SerializeField] private int playerMaxHealth = 3;
        [SerializeField] private int playerHealth = 3;

        private EventBus<PlayerDamagedEvent> playerDamagedEventBus;
        private EventBus<PlayerDefeatedEvent> playerDefeatedEventBus;
        private bool isPlayerTouchingWall = false;
        private bool isMonsterPushing = false;

        // Public properties for access
        public int PlayerHealth => playerHealth;
        public int PlayerMaxHealth => playerMaxHealth;
        public bool IsPlayerTouchingWall => isPlayerTouchingWall;
        public bool IsMonsterPushing => isMonsterPushing;

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Debug.Log("[CombatManager] Initialized as Singleton");
        }

        private void Start()
        {
            playerDamagedEventBus = DIContainer.ResolveFromRegistered<EventBus<PlayerDamagedEvent>>();
            playerDefeatedEventBus = DIContainer.ResolveFromRegistered<EventBus<PlayerDefeatedEvent>>();
            Debug.Log("[CombatManager] EventBus connected");
        }

        private void Update()
        {
            CheckPlayerWallCollision();
        }

        private void CheckPlayerWallCollision()
        {
            // Find player dynamically
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player == null) return;

            bool currentlyTouchingWall = player.IsTouchingWall;

            // 플레이어가 벽에 닿았고, 몬스터가 밀고 있을 때
            if (currentlyTouchingWall && isMonsterPushing && !isPlayerTouchingWall)
            {
                // 플레이어 HP 감소
                DamagePlayer(1);

                // 몬스터에게 멈추라고 알림
                StopAllMonsters();
            }

            isPlayerTouchingWall = currentlyTouchingWall;
        }

        public void DamagePlayer(int damage)
        {
            playerHealth -= damage;
            playerHealth = Mathf.Max(0, playerHealth);

            Debug.Log($"[CombatManager] Player damaged! HP: {playerHealth}/{playerMaxHealth}");

            // 플레이어 스턴 처리
            var playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(damage);
            }

            // 이벤트 발행
            if (playerDamagedEventBus != null)
            {
                playerDamagedEventBus.Publish(new PlayerDamagedEvent(damage));
            }

            // 플레이어 HP가 0이 되면 패배 이벤트 발행
            if (playerHealth == 0 && playerDefeatedEventBus != null)
            {
                playerDefeatedEventBus.Publish(new PlayerDefeatedEvent(playerHealth));
                Debug.Log("[CombatManager] Player defeated!");
            }
        }

        public void HealPlayer(int amount)
        {
            playerHealth += amount;
            playerHealth = Mathf.Min(playerHealth, playerMaxHealth);

            Debug.Log($"[CombatManager] Player healed! HP: {playerHealth}/{playerMaxHealth}");
        }

        public void SetMonsterPushing(bool pushing)
        {
            isMonsterPushing = pushing;
            Debug.Log($"[CombatManager] Monster pushing state: {pushing}");
        }

        public void StopAllMonsters()
        {
            // 모든 EnemyController에게 멈춤 알림
            EnemyController[] enemies = FindObjectsOfType<EnemyController>();
            foreach (EnemyController enemy in enemies)
            {
                enemy.StopPushing();
            }

            isMonsterPushing = false;
            Debug.Log($"[CombatManager] Stopped {enemies.Length} monsters");
        }

        public void ResetPlayerHealth()
        {
            playerHealth = playerMaxHealth;
            Debug.Log($"[CombatManager] Player health reset to {playerMaxHealth}");
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
