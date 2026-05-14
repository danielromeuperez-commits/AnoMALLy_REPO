using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SceneLoader : MonoBehaviour
{
    [Header("Escena a cargar después")]
    [SerializeField] string sceneName = "Main Menu";

    [Header("Player")]
    [SerializeField] Transform player;

    [Tooltip("Scripts del player que se desactivarán cuando quede totalmente congelado.")]
    [SerializeField] MonoBehaviour[] playerScriptsToDisable;

    [Header("Anomalías")]
    [SerializeField] AnomalyManager anomalyManager;

    [Tooltip("Si está activado, el final solo empieza cuando todas las anomalías están corregidas.")]
    [SerializeField] bool requireAllAnomaliesFixed = true;

    [Header("Distancias")]
    [Tooltip("A esta distancia empieza el fade blanco y la ralentización.")]
    [SerializeField] float startEffectDistance = 8f;

    [Tooltip("A esta distancia el fade blanco llega al 100% y el jugador queda congelado.")]
    [SerializeField] float completeEffectDistance = 1.5f;

    [Header("Ralentización")]
    [Tooltip("Velocidad normal del tiempo.")]
    [SerializeField] float normalTimeScale = 1f;

    [Tooltip("Velocidad mínima antes de congelar totalmente.")]
    [SerializeField] float minimumTimeScale = 0.08f;

    [Header("Fade blanco")]
    [SerializeField] CanvasGroup whiteFadeCanvasGroup;

    [Header("Texto final")]
    [SerializeField] TMP_Text finalText;

    [TextArea(3, 6)]
    [SerializeField]
    string finalMessage =
        "Congratulations, you found all the anomalies and managed to escape.\n\nYou can play again and discover new anomalies.";

    [SerializeField] float textFadeInDuration = 1.5f;
    [SerializeField] float secondsShowingText = 5f;
    [SerializeField] float textFadeOutDuration = 1.5f;

    [Header("Fade negro antes de volver al menú")]
    [SerializeField] CanvasGroup blackFadeCanvasGroup;
    [SerializeField] float blackFadeDuration = 2f;

    [Header("Debug")]
    [SerializeField] bool showDebugLogs = true;

    bool sequenceStarted;
    bool finalSequenceStarted;

    float originalFixedDeltaTime;

    private void Start()
    {
        originalFixedDeltaTime = Time.fixedDeltaTime;

        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDeltaTime;

        if (anomalyManager == null)
        {
            anomalyManager = FindAnyObjectByType<AnomalyManager>();
        }

        if (whiteFadeCanvasGroup != null)
        {
            whiteFadeCanvasGroup.alpha = 0f;
            whiteFadeCanvasGroup.gameObject.SetActive(true);
        }

        if (blackFadeCanvasGroup != null)
        {
            blackFadeCanvasGroup.alpha = 0f;
            blackFadeCanvasGroup.gameObject.SetActive(true);
        }

        if (finalText != null)
        {
            finalText.text = finalMessage;
            finalText.alpha = 0f;
            finalText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (finalSequenceStarted) return;
        if (player == null) return;

        // CLAVE:
        // Si todavía NO has arreglado todas las anomalías,
        // no se activa ni el fade blanco ni la ralentización.
        if (requireAllAnomaliesFixed)
        {
            if (anomalyManager == null) return;

            if (!anomalyManager.AllAnomaliesFixed)
            {
                ResetApproachEffect();
                return;
            }
        }

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance > startEffectDistance)
        {
            if (sequenceStarted)
            {
                ResetApproachEffect();
            }

            return;
        }

        sequenceStarted = true;

        float progress = Mathf.InverseLerp(startEffectDistance, completeEffectDistance, distance);
        progress = Mathf.Clamp01(progress);

        ApplyWhiteFade(progress);
        ApplySlowMotion(progress);

        if (progress >= 1f)
        {
            finalSequenceStarted = true;
            StartCoroutine(FinalSequence());
        }
    }

    void ApplyWhiteFade(float progress)
    {
        if (whiteFadeCanvasGroup == null) return;

        whiteFadeCanvasGroup.alpha = progress;
    }

    void ApplySlowMotion(float progress)
    {
        float newTimeScale = Mathf.Lerp(normalTimeScale, minimumTimeScale, progress);

        Time.timeScale = newTimeScale;
        Time.fixedDeltaTime = originalFixedDeltaTime * Time.timeScale;
    }

    void ResetApproachEffect()
    {
        sequenceStarted = false;

        if (whiteFadeCanvasGroup != null)
        {
            whiteFadeCanvasGroup.alpha = 0f;
        }

        Time.timeScale = normalTimeScale;
        Time.fixedDeltaTime = originalFixedDeltaTime;
    }

    IEnumerator FinalSequence()
    {
        if (showDebugLogs)
        {
            Debug.Log("Final iniciado. Jugador congelado.");
        }

        FreezePlayerCompletely();

        if (whiteFadeCanvasGroup != null)
        {
            whiteFadeCanvasGroup.alpha = 1f;
        }

        Time.timeScale = 0f;
        Time.fixedDeltaTime = originalFixedDeltaTime;

        if (finalText != null)
        {
            finalText.gameObject.SetActive(true);
            finalText.text = finalMessage;

            yield return StartCoroutine(FadeText(0f, 1f, textFadeInDuration));
            yield return new WaitForSecondsRealtime(secondsShowingText);
            yield return StartCoroutine(FadeText(1f, 0f, textFadeOutDuration));
        }
        else
        {
            yield return new WaitForSecondsRealtime(secondsShowingText);
        }

        yield return StartCoroutine(FadeCanvasGroupUnscaled(blackFadeCanvasGroup, 0f, 1f, blackFadeDuration));

        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDeltaTime;

        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("No se ha asignado el nombre de la escena.");
        }
    }

    void FreezePlayerCompletely()
    {
        for (int i = 0; i < playerScriptsToDisable.Length; i++)
        {
            if (playerScriptsToDisable[i] != null)
            {
                playerScriptsToDisable[i].enabled = false;
            }
        }

        if (player != null)
        {
            Rigidbody rb = player.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    IEnumerator FadeCanvasGroupUnscaled(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
    {
        if (canvasGroup == null) yield break;

        float timer = 0f;
        canvasGroup.alpha = startAlpha;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / duration;

            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);

            yield return null;
        }

        canvasGroup.alpha = endAlpha;
    }

    IEnumerator FadeText(float startAlpha, float endAlpha, float duration)
    {
        if (finalText == null) yield break;

        float timer = 0f;
        finalText.alpha = startAlpha;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / duration;

            finalText.alpha = Mathf.Lerp(startAlpha, endAlpha, t);

            yield return null;
        }

        finalText.alpha = endAlpha;
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDeltaTime;
    }
}