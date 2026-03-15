using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TowerBreak.UIFlow.Lobby
{
    public sealed class LobbyView : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI floorText;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI weaponText;
        [SerializeField] private Image weaponIcon;
        [SerializeField] private Button challengeButton;
        [SerializeField] private Button equipmentButton;
        [SerializeField] private Button rerollButton;
        [SerializeField] private GameObject weaponPanel;

        private LobbyPresenter presenter;
        private bool isInitialized = false;

        public int DisplayedFloorId { get; private set; }
        public int DisplayedGold { get; private set; }
        public bool IsWeaponEquipped { get; private set; }

        public void Initialize(LobbyPresenter presenter)
        {
            if (presenter == null)
            {
                throw new ArgumentNullException(nameof(presenter));
            }

            this.presenter = presenter;
            isInitialized = true;

            SetupButtons();
            SubscribeToGoldChanges();
            Refresh();

            Debug.Log("[LobbyView] Initialized");
        }

        private void SubscribeToGoldChanges()
        {
            if (presenter?.WalletState != null)
            {
                presenter.WalletState.OnGoldChanged += OnGoldChanged;
                Debug.Log("[LobbyView] Subscribed to gold changes");
            }
        }

        private void OnGoldChanged(int newGold)
        {
            Debug.Log($"[LobbyView] Gold changed to {newGold}, updating UI");
            DisplayedGold = newGold;
            UpdateUI();
        }

        private void OnDestroy()
        {
            if (challengeButton != null)
            {
                challengeButton.onClick.RemoveListener(OnChallengeClicked);
            }

            if (equipmentButton != null)
            {
                equipmentButton.onClick.RemoveListener(OnEquipmentClicked);
            }

            if (rerollButton != null)
            {
                rerollButton.onClick.RemoveListener(OnRerollClicked);
            }

            if (presenter?.WalletState != null)
            {
                presenter.WalletState.OnGoldChanged -= OnGoldChanged;
            }
        }

        private void SetupButtons()
        {
            if (challengeButton != null)
            {
                challengeButton.onClick.RemoveAllListeners();
                challengeButton.onClick.AddListener(OnChallengeClicked);
            }

            if (equipmentButton != null)
            {
                equipmentButton.onClick.RemoveAllListeners();
                equipmentButton.onClick.AddListener(OnEquipmentClicked);
            }

            if (rerollButton != null)
            {
                rerollButton.onClick.RemoveAllListeners();
                rerollButton.onClick.AddListener(OnRerollClicked);
            }
        }

        private void OnChallengeClicked()
        {
            Debug.Log("[LobbyView] Challenge button clicked");
            presenter?.OnOpenChallenge();
        }

        private void OnEquipmentClicked()
        {
            Debug.Log("[LobbyView] Equipment button clicked");
            presenter?.OnOpenEquipment();
        }

        private void OnRerollClicked()
        {
            Debug.Log("[LobbyView] Reroll button clicked");
            presenter?.OnOpenReroll();
        }

        public void Refresh()
        {
            if (!isInitialized || presenter == null)
            {
                return;
            }

            DisplayedFloorId = presenter.CurrentFloorId;
            DisplayedGold = presenter.Gold;
            IsWeaponEquipped = presenter.HasEquippedWeapon;

            UpdateUI();
        }

        private void UpdateUI()
        {
            if (floorText != null)
            {
                floorText.text = $"Floor {DisplayedFloorId}";
            }

            if (goldText != null)
            {
                goldText.text = $"{DisplayedGold:N0} G";
            }

            if (weaponText != null)
            {
                if (IsWeaponEquipped)
                {
                    weaponText.text = "Weapon Equipped";
                }
                else
                {
                    weaponText.text = "No Weapon";
                }
            }

            if (weaponPanel != null)
            {
                weaponPanel.SetActive(IsWeaponEquipped);
            }

            if (weaponIcon != null)
            {
                weaponIcon.gameObject.SetActive(IsWeaponEquipped);
            }
        }
    }
}
