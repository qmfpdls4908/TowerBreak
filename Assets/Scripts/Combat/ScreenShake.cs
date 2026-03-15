using System.Collections;
using UnityEngine;

namespace TowerBreak.Combat
{
    public sealed class ScreenShake : MonoBehaviour
    {
        public static ScreenShake Instance { get; private set; }

        private Vector3 originalPosition;
        private bool isShaking = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// 화면 흔들림 시작
        /// </summary>
        /// <param name="duration">흔들림 지속 시간 (초)</param>
        /// <param name="magnitude">흔들림 강도</param>
        public void Shake(float duration = 0.15f, float magnitude = 0.15f)
        {
            if (!isShaking)
            {
                StartCoroutine(ShakeCoroutine(duration, magnitude));
            }
        }

        private IEnumerator ShakeCoroutine(float duration, float magnitude)
        {
            isShaking = true;
            originalPosition = transform.localPosition;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;

                transform.localPosition = originalPosition + new Vector3(x, y, 0f);

                elapsed += Time.deltaTime;

                // 시간이 지남에 따라 흔들림 감소
                magnitude = Mathf.Lerp(magnitude, 0f, elapsed / duration);

                yield return null;
            }

            transform.localPosition = originalPosition;
            isShaking = false;
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
