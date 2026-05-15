using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuFadeIn : MonoBehaviour
{
    [Header("Fade negro")]
    [SerializeField] CanvasGroup blackFadeCanvasGroup;
    [SerializeField] Image blackFadeImage;
    [SerializeField] float fadeOutDuration = 2f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        if (blackFadeCanvasGroup == null) return;

        blackFadeCanvasGroup.gameObject.SetActive(true);

        blackFadeCanvasGroup.alpha = 1f;
        blackFadeCanvasGroup.interactable = false;
        blackFadeCanvasGroup.blocksRaycasts = true;

        if (blackFadeImage != null)
        {
            blackFadeImage.raycastTarget = true;
        }

        StartCoroutine(FadeOutFromBlack());
    }

    IEnumerator FadeOutFromBlack()
    {
        float timer = 0f;

        while (timer < fadeOutDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = timer / fadeOutDuration;
            blackFadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        blackFadeCanvasGroup.alpha = 0f;

        blackFadeCanvasGroup.blocksRaycasts = false;
        blackFadeCanvasGroup.interactable = false;

        if (blackFadeImage != null)
        {
            blackFadeImage.raycastTarget = false;
        }

        blackFadeCanvasGroup.gameObject.SetActive(false);
    }
}