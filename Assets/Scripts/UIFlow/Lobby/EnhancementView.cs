using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TowerBreak.UIFlow.Lobby
{
    public sealed class EnhancementView : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private Transform equipmentListParent;
        [SerializeField] private GameObject enhancementItemPrefab;
        [SerializeField] private Button closeButton;

        [Header("Feedback Panel")]
        [SerializeField] private GameObject feedbackPanel;
        [SerializeField] private TextMeshProUGUI feedbackText;
        [SerializeField] private float feedbackDuration = 2f;

        private EnhancementPresenter presenter;
        private List<GameObject> equipmentItems = new();
        private bool isInitialized = false;
        private float feedbackTimer = 0f;

        public void Initialize(EnhancementPresenter presenter)
        {
            if (presenter == null)
            {
                throw new ArgumentNullException(nameof(presenter));
            }

            this.presenter = presenter;
            isInitialized = true;

            SetupButtons();
            Refresh();

            if (feedbackPanel != null)
            {
                feedbackPanel.SetActive(false);
            }

            Debug.Log("[EnhancementView] Initialized");
        }

        private void Update()
        {
            if (feedbackTimer > 0)
            {
                feedbackTimer -= Time.deltaTime;
                if (feedbackTimer <= 0)
                {
                    HideFeedback();
                }
            }
        }

        private void SetupButtons()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(OnCloseClicked);
            }
        }

        private void OnCloseClicked()
        {
            Debug.Log("[EnhancementView] Close button clicked");
            presenter?.OnClose();
        }

        public void Refresh()
        {
            if (!isInitialized || presenter == null)
            {
                return;
            }

            UpdateGoldDisplay();
            UpdateEquipmentList();
        }

        public void RefreshGoldOnly()
        {
            if (!isInitialized || presenter == null)
            {
                return;
            }

            UpdateGoldDisplay();
        }

        private void UpdateGoldDisplay()
        {
            if (goldText == null)
            {
                Debug.LogError("[EnhancementView] goldText is NULL!");
                return;
            }
            
            if (presenter == null)
            {
                Debug.LogError("[EnhancementView] presenter is NULL!");
                return;
            }
            
            int currentGold = presenter.CurrentGold;
            goldText.text = $"Gold: {currentGold:N0}";
            Debug.Log($"[EnhancementView] Gold text set to: {goldText.text}, GameObject active: {goldText.gameObject.activeInHierarchy}");
        }

        private void UpdateEquipmentList()
        {
            foreach (var item in equipmentItems)
            {
                if (item != null)
                {
                    Destroy(item);
                }
            }
            equipmentItems.Clear();

            if (equipmentListParent == null || enhancementItemPrefab == null)
            {
                Debug.LogWarning("[EnhancementView] equipmentListParent or enhancementItemPrefab is null");
                return;
            }

            var allEquipment = presenter.GetAllEquipmentForEnhancement();

            if (allEquipment.Count == 0)
            {
                Debug.Log("[EnhancementView] No equipment to display");
                return;
            }

            foreach (var equipment in allEquipment)
            {
                var itemObj = Instantiate(enhancementItemPrefab, equipmentListParent);
                
                var rectTransform = itemObj.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.localScale = Vector3.one;
                    rectTransform.localPosition = Vector3.zero;
                    rectTransform.anchoredPosition = Vector2.zero;
                }
                
                var itemView = itemObj.GetComponent<EnhancementItemView>();

                if (itemView != null)
                {
                    itemView.Setup(equipment, OnEnhanceClicked);
                }
                else
                {
                    Debug.LogError("[EnhancementView] EnhancementItemView component NOT FOUND on prefab!");
                }

                equipmentItems.Add(itemObj);
            }
            
            Debug.Log($"[EnhancementView] Total equipment items: {equipmentItems.Count}");
        }

        private void OnEnhanceClicked(int instanceId)
        {
            Debug.Log($"[EnhancementView] Enhance clicked for instance {instanceId}");
            
            var result = presenter.TryEnhance(instanceId);
            
            if (result.Success)
            {
                ShowFeedback($"Enhancement Success!\nLevel {result.NewLevel}\nATK: {result.NewAttackPower}", true);
                Refresh();
            }
            else
            {
                ShowFeedback($"Enhancement Failed\n{result.ErrorMessage}", false);
                RefreshGoldOnly();
            }
        }

        private void ShowFeedback(string message, bool isSuccess)
        {
            if (feedbackPanel != null && feedbackText != null)
            {
                feedbackText.text = message;
                feedbackPanel.SetActive(true);
                feedbackTimer = feedbackDuration;
                
                Debug.Log($"[EnhancementView] Feedback: {message}");
            }
        }

        private void HideFeedback()
        {
            if (feedbackPanel != null)
            {
                feedbackPanel.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(OnCloseClicked);
            }
        }
    }
}
