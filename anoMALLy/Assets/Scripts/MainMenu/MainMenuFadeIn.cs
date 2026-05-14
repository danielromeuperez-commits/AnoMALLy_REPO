using System.Collections;
using UnityEngine;

public class MainMenuFadeIn : MonoBehaviour
{
    [Header("Fade negro")]
    [SerializeField] CanvasGroup blackFadeCanvasGroup;
    [SerializeField] float fadeOutDuration = 2f;

    private void Start()
    {
        if (blackFadeCanvasGroup == null) return;

        blackFadeCanvasGroup.gameObject.SetActive(true);
        blackFadeCanvasGroup.alpha = 1f;

        StartCoroutine(FadeOutFromBlack());
    }

    IEnumerator FadeOutFromBlack()
    {
        float timer = 0f;

        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeOutDuration;

            blackFadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        blackFadeCanvasGroup.alpha = 0f;
    }
}