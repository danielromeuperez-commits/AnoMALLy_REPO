using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneRandom : MonoBehaviour
{
    [Header("Escenas aleatorias")]
    [SerializeField] string[] escenas;

    [Header("Fade negro")]
    [SerializeField] CanvasGroup blackFadeCanvasGroup;
    [SerializeField] float fadeDuration = 2f;
    [SerializeField] float holdBlackTime = 0.4f;

    [Header("Opciones")]
    [SerializeField] bool lockPlayerDuringFade = true;
    [SerializeField] MonoBehaviour[] playerScriptsToDisable;

    bool isLoading;

    private void Start()
    {
        if (blackFadeCanvasGroup != null)
        {
            blackFadeCanvasGroup.gameObject.SetActive(true);
            blackFadeCanvasGroup.alpha = 0f;
            blackFadeCanvasGroup.blocksRaycasts = false;
            blackFadeCanvasGroup.interactable = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isLoading) return;
        if (!other.CompareTag("Player")) return;

        StartCoroutine(LoadRandomSceneWithFade());
    }

    IEnumerator LoadRandomSceneWithFade()
    {
        isLoading = true;

        if (escenas == null || escenas.Length == 0)
        {
            Debug.LogWarning("No hay escenas asignadas en SceneRandom.");
            yield break;
        }

        if (lockPlayerDuringFade)
        {
            DisablePlayerScripts();
        }

        if (blackFadeCanvasGroup != null)
        {
            blackFadeCanvasGroup.gameObject.SetActive(true);
            blackFadeCanvasGroup.blocksRaycasts = true;
            blackFadeCanvasGroup.interactable = false;

            yield return StartCoroutine(FadeCanvasGroup(blackFadeCanvasGroup, 0f, 1f, fadeDuration));
        }

        yield return new WaitForSecondsRealtime(holdBlackTime);

        int indice = Random.Range(0, escenas.Length);
        SceneManager.LoadScene(escenas[indice]);
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

            // Fade más cinematográfico: empieza suave y termina suave.
            t = Mathf.SmoothStep(0f, 1f, t);

            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);

            yield return null;
        }

        canvasGroup.alpha = endAlpha;
    }

    void DisablePlayerScripts()
    {
        for (int i = 0; i < playerScriptsToDisable.Length; i++)
        {
            if (playerScriptsToDisable[i] != null)
            {
                playerScriptsToDisable[i].enabled = false;
            }
        }
    }
}