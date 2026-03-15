using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TowerBreak.UIFlow.Lobby
{
    public sealed class NoticePopupView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float displayDuration = 1.5f;
        [SerializeField] private float fadeDuration = 0.5f;

        private Coroutine activeCoroutine;

        public void Show(string message)
        {
            if (messageText != null)
            {
                messageText.text = message;
            }

            gameObject.SetActive(true);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }

            if (activeCoroutine != null)
            {
                StopCoroutine(activeCoroutine);
            }

            activeCoroutine = StartCoroutine(ShowAndFade());
        }

        private IEnumerator ShowAndFade()
        {
            yield return new WaitForSeconds(displayDuration);

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f - (elapsed / fadeDuration);
                }
                yield return null;
            }

            gameObject.SetActive(false);
            activeCoroutine = null;
        }
    }
}
