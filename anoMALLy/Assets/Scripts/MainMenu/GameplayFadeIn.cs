using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameplayFadeIn : MonoBehaviour
{
    [Header("Fade negro")]
    [SerializeField] CanvasGroup blackFadeCanvasGroup;
    [SerializeField] Image blackFadeImage;
    [SerializeField] float fadeOutDuration = 3f;

    [Header("Suavidad")]
    [SerializeField] bool useSmoothFade = true;

    [Header("Cursor")]
    [SerializeField] bool lockCursorOnStart = true;

    private void Awake()
    {
        // Esto se ejecuta antes que Start.
        // Así evitamos que se vea la escena un frame antes del negro.
        if (blackFadeCanvasGroup != null)
        {
            blackFadeCanvasGroup.gameObject.SetActive(true);
            blackFadeCanvasGroup.alpha = 1f;
            blackFadeCanvasGroup.blocksRaycasts = true;
            blackFadeCanvasGroup.interactable = false;
        }

        if (blackFadeImage != null)
        {
            blackFadeImage.color = Color.black;
            blackFadeImage.raycastTarget = true;
        }
    }

    private void Start()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        if (lockCursorOnStart)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (blackFadeCanvasGroup == null) return;

        StartCoroutine(FadeOutFromBlack());
    }

    IEnumerator FadeOutFromBlack()
    {
        float timer = 0f;

        while (timer < fadeOutDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / fadeOutDuration);

            if (useSmoothFade)
            {
                t = Mathf.SmoothStep(0f, 1f, t);
            }

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