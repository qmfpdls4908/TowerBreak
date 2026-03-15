using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TowerBreak.UIFlow.Lobby
{
    public class EnhancementItemView : MonoBehaviour
    {
        [SerializeField] private Image weaponImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI currentAttackText;
        [SerializeField] private TextMeshProUGUI nextAttackText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Button enhanceButton;
        [SerializeField] private GameObject maxLevelIndicator;

        private int instanceId;
        private System.Action<int> onEnhanceCallback;

        public void Setup(EnhancementDisplayInfo info, System.Action<int> onEnhance)
        {
            instanceId = info.InstanceId;
            onEnhanceCallback = onEnhance;

            if (weaponImage != null && info.WeaponIcon != null)
            {
                weaponImage.sprite = info.WeaponIcon;
            }

            if (nameText != null)
            {
                nameText.text = info.WeaponName;
            }

            if (levelText != null)
            {
                levelText.text = $"Lv.{info.CurrentLevel}";
            }

            if (currentAttackText != null)
            {
                currentAttackText.text = $"ATK: {info.CurrentAttack}";
            }

            if (nextAttackText != null)
            {
                if (info.IsMaxLevel)
                {
                    nextAttackText.text = "MAX";
                }
                else
                {
                    nextAttackText.text = $"→ {info.NextAttack}";
                }
            }

            if (costText != null)
            {
                if (info.IsMaxLevel)
                {
                    costText.text = "MAX LEVEL";
                }
                else
                {
                    costText.text = $"Cost: {info.Cost}G";
                }
            }

            if (maxLevelIndicator != null)
            {
                maxLevelIndicator.SetActive(info.IsMaxLevel);
            }

            if (enhanceButton != null)
            {
                enhanceButton.gameObject.SetActive(!info.IsMaxLevel);
                enhanceButton.interactable = info.CanEnhance;
                enhanceButton.onClick.RemoveAllListeners();
                enhanceButton.onClick.AddListener(() => onEnhanceCallback?.Invoke(instanceId));
            }
        }

        private void OnDestroy()
        {
            if (enhanceButton != null)
            {
                enhanceButton.onClick.RemoveAllListeners();
            }
        }
    }
}
