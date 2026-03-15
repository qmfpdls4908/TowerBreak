using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TowerBreak.UIFlow.Lobby
{
    public class EquipmentItemView : MonoBehaviour
    {
        [SerializeField] private Image weaponImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI attackText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Button equipButton;
        [SerializeField] private Button enhanceButton;
        [SerializeField] private TextMeshProUGUI enhanceCostText;
        [SerializeField] private GameObject equippedIndicator;

        private int instanceId;
        private System.Action<int> onEquipCallback;
        private System.Action<int> onEnhanceCallback;

        public void Setup(EquipmentDisplayInfo info, bool isEquipped, System.Action<int> onEquip, System.Action<int> onEnhance)
        {
            instanceId = info.InstanceId;
            onEquipCallback = onEquip;
            onEnhanceCallback = onEnhance;

            Debug.Log($"[EquipmentItemView] Setting up: WeaponName={info.WeaponName}, InstanceId={info.InstanceId}, Level={info.EnhancementLevel}");
            
            if (nameText != null)
            {
                Debug.Log($"[EquipmentItemView] Setting nameText to: {info.WeaponName}");
                nameText.text = info.WeaponName;
            }
            else
            {
                Debug.LogWarning("[EquipmentItemView] nameText is null!");
            }

            if (attackText != null)
            {
                attackText.text = $"ATK: {info.AttackPower}";
            }

            if (levelText != null)
            {
                levelText.text = $"Lv.{info.EnhancementLevel}";
            }

            // 무기 이미지 로드 및 표시
            if (weaponImage != null && !string.IsNullOrEmpty(info.WeaponSpritePath))
            {
                Sprite weaponSprite = Resources.Load<Sprite>(info.WeaponSpritePath);
                if (weaponSprite != null)
                {
                    weaponImage.sprite = weaponSprite;
                    weaponImage.gameObject.SetActive(true);
                    weaponImage.SetNativeSize();
                    Debug.Log($"[EquipmentItemView] Loaded weapon sprite: {info.WeaponSpritePath}");
                }
                else
                {
                    Debug.LogWarning($"[EquipmentItemView] Failed to load weapon sprite: {info.WeaponSpritePath}");
                    weaponImage.gameObject.SetActive(false);
                }
            }
            else if (weaponImage != null)
            {
                weaponImage.gameObject.SetActive(false);
            }

            if (equippedIndicator != null)
            {
                equippedIndicator.SetActive(isEquipped);
            }

            if (equipButton != null)
            {
                equipButton.gameObject.SetActive(!isEquipped);
                equipButton.onClick.RemoveAllListeners();
                equipButton.onClick.AddListener(() => onEquipCallback?.Invoke(instanceId));
            }

            if (enhanceButton != null)
            {
                enhanceButton.gameObject.SetActive(info.NextEnhancementCost > 0);
                enhanceButton.onClick.RemoveAllListeners();
                enhanceButton.onClick.AddListener(() => onEnhanceCallback?.Invoke(instanceId));
                enhanceButton.interactable = info.CanEnhance;
            }

            if (enhanceCostText != null)
            {
                if (info.NextEnhancementCost > 0)
                {
                    enhanceCostText.text = $"{info.NextEnhancementCost}G";
                    enhanceCostText.gameObject.SetActive(true);
                }
                else
                {
                    enhanceCostText.text = "MAX";
                    enhanceCostText.gameObject.SetActive(true);
                }
            }
        }

        private void OnDestroy()
        {
            if (equipButton != null)
            {
                equipButton.onClick.RemoveAllListeners();
            }

            if (enhanceButton != null)
            {
                enhanceButton.onClick.RemoveAllListeners();
            }
        }
    }
}
