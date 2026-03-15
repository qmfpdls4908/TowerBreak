using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TowerBreak.UIFlow.Lobby
{
    public sealed class EquipmentView : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Transform equipmentListParent;
        [SerializeField] private GameObject equipmentItemPrefab;
        [SerializeField] private Button closeButton;

        [Header("Current Equipment")]
        [SerializeField] private GameObject currentEquipmentPanel;
        [SerializeField] private TextMeshProUGUI currentWeaponText;
        [SerializeField] private Button unequipButton;

        private EquipmentPresenter presenter;
        private List<GameObject> equipmentItems = new();
        private bool isInitialized = false;

        public void Initialize(EquipmentPresenter presenter)
        {
            if (presenter == null)
            {
                throw new ArgumentNullException(nameof(presenter));
            }

            this.presenter = presenter;
            isInitialized = true;

            SetupButtons();
            Refresh();

            Debug.Log("[EquipmentView] Initialized");
        }

        private void SetupButtons()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(OnCloseClicked);
            }

            if (unequipButton != null)
            {
                unequipButton.onClick.RemoveAllListeners();
                unequipButton.onClick.AddListener(OnUnequipClicked);
            }
        }

        private void OnCloseClicked()
        {
            Debug.Log("[EquipmentView] Close button clicked");
            presenter?.OnClose();
        }

        private void OnUnequipClicked()
        {
            Debug.Log("[EquipmentView] Unequip button clicked");
            presenter?.OnUnequipWeapon();
            Refresh();
        }

        public void Refresh()
        {
            if (!isInitialized || presenter == null)
            {
                return;
            }

            UpdateCurrentEquipment();
            UpdateEquipmentList();
        }

        private void UpdateCurrentEquipment()
        {
            if (currentEquipmentPanel == null)
            {
                return;
            }

            var equippedWeapon = presenter.GetEquippedWeapon();

            if (equippedWeapon.HasValue)
            {
                currentEquipmentPanel.SetActive(true);
                if (currentWeaponText != null)
                {
                    currentWeaponText.text = $"Equipped: {equippedWeapon.Value.WeaponName}";
                }
                if (unequipButton != null)
                {
                    unequipButton.gameObject.SetActive(true);
                }
            }
            else
            {
                currentEquipmentPanel.SetActive(false);
                if (unequipButton != null)
                {
                    unequipButton.gameObject.SetActive(false);
                }
            }
        }

        private void UpdateEquipmentList()
        {
            // 기존 아이템 제거
            foreach (var item in equipmentItems)
            {
                if (item != null)
                {
                    Destroy(item);
                }
            }
            equipmentItems.Clear();

            if (equipmentListParent == null || equipmentItemPrefab == null)
            {
                Debug.LogWarning("[EquipmentView] equipmentListParent or equipmentItemPrefab is null");
                return;
            }

            var allEquipment = presenter.GetAllEquipment();

            if (allEquipment.Count == 0)
            {
                Debug.Log("[EquipmentView] No equipment to display");
                return;
            }

            foreach (var equipment in allEquipment)
            {
                // 프리팹 인스턴스화 및 부모 설정
                var itemObj = Instantiate(equipmentItemPrefab, equipmentListParent);
                
                // RectTransform 설정
                var rectTransform = itemObj.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.localScale = Vector3.one;
                    rectTransform.localPosition = Vector3.zero;
                    rectTransform.anchoredPosition = Vector2.zero;
                }
                
                var itemView = itemObj.GetComponent<EquipmentItemView>();
                Debug.Log($"[EquipmentView] GetComponent<EquipmentItemView>(): {itemView != null}");

                if (itemView != null)
                {
                    Debug.Log($"[EquipmentView] Calling Setup() for {equipment.WeaponName}");
                    bool isEquipped = presenter.IsWeaponEquipped(equipment.InstanceId);
                    itemView.Setup(equipment, isEquipped, OnEquipClicked, OnEnhanceClicked);
                }
                else
                {
                    Debug.LogError($"[EquipmentView] EquipmentItemView component NOT FOUND on prefab!");
                }

                equipmentItems.Add(itemObj);
                Debug.Log($"[EquipmentView] Added equipment item: {equipment.WeaponName}");
            }
            
            Debug.Log($"[EquipmentView] Total equipment items: {equipmentItems.Count}");
        }

        private void OnEquipClicked(int instanceId)
        {
            Debug.Log($"[EquipmentView] Equip clicked for instance {instanceId}");
            presenter?.OnEquipWeapon(instanceId);
            Refresh();
        }

        private void OnEnhanceClicked(int instanceId)
        {
            Debug.Log($"[EquipmentView] Enhance clicked for instance {instanceId}");
            bool success = presenter?.TryEnhanceWeapon(instanceId) ?? false;
            if (success)
            {
                Debug.Log($"[EquipmentView] Enhancement successful for instance {instanceId}");
            }
            else
            {
                Debug.Log($"[EquipmentView] Enhancement failed for instance {instanceId}");
            }
            Refresh();
        }

        private void OnDestroy()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(OnCloseClicked);
            }

            if (unequipButton != null)
            {
                unequipButton.onClick.RemoveListener(OnUnequipClicked);
            }
        }
    }

}
