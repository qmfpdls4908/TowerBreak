using System.Collections;
using UnityEngine;
using TMPro;

namespace TowerBreak.Combat
{
    /// <summary>
    /// 데미지 숫자가 위로 떠오르며 사라지는 팝업
    /// </summary>
    public sealed class DamagePopup : MonoBehaviour
    {
        private TextMeshPro textMesh;
        private float floatSpeed = 2f;
        private float fadeDuration = 0.8f;
        private Color startColor;

        private static TMP_FontAsset cachedFont;

        /// <summary>
        /// 데미지 팝업 생성
        /// </summary>
        public static DamagePopup Create(Vector3 position, int damage)
        {
            GameObject popupObj = new GameObject("DamagePopup");
            popupObj.transform.position = position + Vector3.up * 0.5f;

            DamagePopup popup = popupObj.AddComponent<DamagePopup>();
            popup.Setup(damage);

            return popup;
        }

        private void Setup(int damage)
        {
            textMesh = gameObject.AddComponent<TextMeshPro>();
            textMesh.text = damage.ToString();
            textMesh.fontSize = 8;
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.sortingOrder = 100;

            // rrr TMP 폰트 로드
            if (cachedFont == null)
            {
                cachedFont = Resources.Load<TMP_FontAsset>("Fonts/rrr");
                if (cachedFont == null)
                {
                    // 직접 경로로 시도
                    cachedFont = Resources.Load<TMP_FontAsset>("rrr");
                }
            }

            if (cachedFont != null)
            {
                textMesh.font = cachedFont;
            }
            else
            {
                Debug.LogWarning("[DamagePopup] rrr TMP font not found in Resources!");
            }

            // 색상 설정 (노란색)
            startColor = new Color(1f, 0.9f, 0.2f, 1f);
            textMesh.color = startColor;

            // 외곽선
            textMesh.outlineWidth = 0.3f;
            textMesh.outlineColor = Color.black;

            StartCoroutine(FloatAndFade());
        }

        private IEnumerator FloatAndFade()
        {
            float elapsed = 0f;
            Vector3 startPos = transform.position;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;

                // 위로 떠오름
                transform.position = startPos + Vector3.up * (floatSpeed * t);

                // 서서히 투명해짐 (후반부에 빠르게)
                float alpha = Mathf.Lerp(1f, 0f, t * t);
                textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

                // 크기 살짝 커지면서
                float scale = Mathf.Lerp(1f, 1.3f, t);
                transform.localScale = Vector3.one * scale;

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
