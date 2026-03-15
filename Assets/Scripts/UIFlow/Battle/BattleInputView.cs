using System;
using UnityEngine;
using UnityEngine.UI;

namespace TowerBreak.UIFlow.Battle
{
    public sealed class BattleInputView : MonoBehaviour, IBattleInputView
    {
        [SerializeField] private Button attackButton;
        [SerializeField] private Button guardButton;
        [SerializeField] private Button dashButton;
        
        public event Action OnAttackClicked;
        public event Action OnGuardClicked;
        public event Action OnDashClicked;
        
        private void Awake()
        {
            if (attackButton != null)
                attackButton.onClick.AddListener(() => OnAttackClicked?.Invoke());
            
            if (guardButton != null)
                guardButton.onClick.AddListener(() => OnGuardClicked?.Invoke());
            
            if (dashButton != null)
                dashButton.onClick.AddListener(() => OnDashClicked?.Invoke());
        }
        
        public void SetAttackButtonEnabled(bool enabled)
        {
            if (attackButton != null)
                attackButton.interactable = enabled;
        }
        
        public void SetGuardButtonEnabled(bool enabled)
        {
            if (guardButton != null)
                guardButton.interactable = enabled;
        }
        
        public void SetDashButtonEnabled(bool enabled)
        {
            if (dashButton != null)
                dashButton.interactable = enabled;
        }
        
        private void OnDestroy()
        {
            if (attackButton != null)
                attackButton.onClick.RemoveAllListeners();
            
            if (guardButton != null)
                guardButton.onClick.RemoveAllListeners();
            
            if (dashButton != null)
                dashButton.onClick.RemoveAllListeners();
        }
    }
}
