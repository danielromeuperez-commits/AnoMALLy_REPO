using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClickScene : MonoBehaviour
{
    [Header("Nombre de la escena a cargar")]
    [SerializeField] string sceneName;

    [Header("Fade negro")]
    [SerializeField] CanvasGroup blackFadeCanvasGroup;
    [SerializeField] float fadeDuration = 1.5f;

    [Header("Audio")]
    [SerializeField] bool fadeOutMusic = true;
    [SerializeField] float musicFadeOutDuration = 1.5f;

    [Header("SFX")]
    [SerializeField] bool playClickSFX = true;
    [SerializeField] int clickSFXIndex = 3;

    bool isLoading;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 1f;

        if (blackFadeCanvasGroup != null)
        {
            blackFadeCanvasGroup.alpha = 0f;
            blackFadeCanvasGroup.blocksRaycasts = false;
            blackFadeCanvasGroup.interactable = false;
            blackFadeCanvasGroup.gameObject.SetActive(true);
        }
    }

    public void LoadScene()
    {
        if (isLoading) return;

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("No se ha asignado el nombre de la escena.");
            return;
        }

        StartCoroutine(LoadSceneWithFade());
    }

    IEnumerator LoadSceneWithFade()
    {
        isLoading = true;

        if (playClickSFX && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(clickSFXIndex);
        }

        if (fadeOutMusic && AudioManager.Instance != null)
        {
            AudioManager.Instance.FadeOutMusic(musicFadeOutDuration);
        }

        if (blackFadeCanvasGroup != null)
        {
            blackFadeCanvasGroup.gameObject.SetActive(true);
            blackFadeCanvasGroup.blocksRaycasts = true;
            blackFadeCanvasGroup.interactable = false;

            yield return StartCoroutine(FadeCanvasGroup(blackFadeCanvasGroup, 0f, 1f, fadeDuration));
        }
        else
        {
            yield return new WaitForSecondsRealtime(fadeDuration);
        }

        SceneManager.LoadScene(sceneName);
    }

    IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
    {
        if (canvasGroup == null) yield break;

        float timer = 0f;
        canvasGroup.alpha = startAlpha;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / duration);
            t = Mathf.SmoothStep(0f, 1f, t);

            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);

            yield return null;
        }

        canvasGroup.alpha = endAlpha;
    }
}