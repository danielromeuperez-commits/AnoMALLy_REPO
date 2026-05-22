using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SceneLoader : MonoBehaviour
{
    [Header("Escena a cargar después")]
    [SerializeField] string sceneName = "MainMenu";

    [Header("Player")]
    [SerializeField] Transform player;

    [Tooltip("Scripts del player que se desactivarán cuando quede totalmente congelado.")]
    [SerializeField] MonoBehaviour[] playerScriptsToDisable;

    [Header("Anomalías")]
    [SerializeField] AnomalyManager anomalyManager;

    [Tooltip("Si está activado, el final solo empieza cuando todas las anomalías están corregidas.")]
    [SerializeField] bool requireAllAnomaliesFixed = true;

    [Header("Distancias")]
    [SerializeField] float startEffectDistance = 6f;
    [SerializeField] float completeEffectDistance = 1.5f;

    [Header("Ralentización")]
    [SerializeField] float normalTimeScale = 1f;
    [SerializeField] float minimumTimeScale = 0.08f;

    [Header("Fade blanco")]
    [SerializeField] CanvasGroup whiteFadeCanvasGroup;
    [SerializeField, Range(0f, 0.95f)] float whiteFadeStartProgress = 0.35f;
    [SerializeField, Range(1f, 5f)] float whiteFadeSoftness = 2.5f;

    [Header("Audio puerta final")]
    [SerializeField] bool playFinalDoorOpenSFX = true;
    [SerializeField] int finalDoorOpenSFXIndex = 9; // Final_Door_Opening

    [Header("Audio entrando a la puerta")]
    [SerializeField] bool playEnterDoorAudio = true;
    [SerializeField] int enterDoorSFXIndex = 11; // Sound_Enter_Final_Door
    [SerializeField] AudioSource enterDoorAudioSource;
    [SerializeField, Range(0f, 1f)] float enterDoorVolume = 0.6f;

    [Header("Sonidos de fondo que se apagan al entrar")]
    [SerializeField] bool fadeBackgroundSoundsWhileEntering = true;

    [Tooltip("Aquí arrastras los AudioSource de sonidos ambientales: TV estática, ojos, música del escenario, etc.")]
    [SerializeField] AudioSource[] backgroundAudioSourcesToFade;

    [SerializeField, Range(0f, 1f)] float minimumBackgroundVolume = 0f;

    [Header("Fade del audio al volver al menú")]
    [SerializeField] bool fadeOutEnterDoorAudioWithBlackFade = true;

    [Header("Pausa")]
    [SerializeField] bool disablePauseWhenEnteringFinalDoor = true;

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
    bool enterDoorAudioStarted;

    static bool finalDoorOpenSoundPlayedGlobal;

    float originalFixedDeltaTime;
    float[] originalBackgroundVolumes;

    private void Start()
    {
        finalDoorOpenSoundPlayedGlobal = false;

        originalFixedDeltaTime = Time.fixedDeltaTime;

        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDeltaTime;

        PauseMenu.CanPause = true;

        if (anomalyManager == null)
        {
            anomalyManager = FindAnyObjectByType<AnomalyManager>();
        }

        if (enterDoorAudioSource == null)
        {
            enterDoorAudioSource = GetComponent<AudioSource>();
        }

        if (enterDoorAudioSource == null)
        {
            enterDoorAudioSource = gameObject.AddComponent<AudioSource>();
        }

        SetupEnterDoorAudioSource();
        CacheBackgroundVolumes();

        if (whiteFadeCanvasGroup != null)
        {
            whiteFadeCanvasGroup.alpha = 0f;
            whiteFadeCanvasGroup.gameObject.SetActive(true);
            whiteFadeCanvasGroup.blocksRaycasts = false;
            whiteFadeCanvasGroup.interactable = false;
        }

        if (blackFadeCanvasGroup != null)
        {
            blackFadeCanvasGroup.blocksRaycasts = false;
            blackFadeCanvasGroup.interactable = false;
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

        if (!sequenceStarted)
        {
            sequenceStarted = true;

            if (disablePauseWhenEnteringFinalDoor)
            {
                DisablePause();
            }

            PlayFinalDoorOpenSound();
            StartEnterDoorAudio();
        }

        float progress = Mathf.InverseLerp(startEffectDistance, completeEffectDistance, distance);
        progress = Mathf.Clamp01(progress);

        ApplyWhiteFade(progress);
        ApplySlowMotion(progress);
        ApplyBackgroundFade(progress);

        if (progress >= 1f)
        {
            finalSequenceStarted = true;
            StartCoroutine(FinalSequence());
        }
    }

    void SetupEnterDoorAudioSource()
    {
        if (enterDoorAudioSource == null) return;

        enterDoorAudioSource.playOnAwake = false;
        enterDoorAudioSource.loop = true;
        enterDoorAudioSource.volume = enterDoorVolume;
        enterDoorAudioSource.spatialBlend = 1f;

        if (AudioManager.Instance == null) return;
        if (AudioManager.Instance.sfxLibrary == null) return;
        if (enterDoorSFXIndex < 0 || enterDoorSFXIndex >= AudioManager.Instance.sfxLibrary.Length) return;

        enterDoorAudioSource.clip = AudioManager.Instance.sfxLibrary[enterDoorSFXIndex];
    }

    void CacheBackgroundVolumes()
    {
        if (backgroundAudioSourcesToFade == null)
        {
            originalBackgroundVolumes = new float[0];
            return;
        }

        originalBackgroundVolumes = new float[backgroundAudioSourcesToFade.Length];

        for (int i = 0; i < backgroundAudioSourcesToFade.Length; i++)
        {
            if (backgroundAudioSourcesToFade[i] != null)
            {
                originalBackgroundVolumes[i] = backgroundAudioSourcesToFade[i].volume;
            }
        }
    }

    void PlayFinalDoorOpenSound()
    {
        if (!playFinalDoorOpenSFX) return;
        if (finalDoorOpenSoundPlayedGlobal) return;

        finalDoorOpenSoundPlayedGlobal = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(finalDoorOpenSFXIndex);
        }
    }

    void StartEnterDoorAudio()
    {
        if (!playEnterDoorAudio) return;
        if (enterDoorAudioStarted) return;
        if (enterDoorAudioSource == null) return;

        enterDoorAudioStarted = true;

        if (enterDoorAudioSource.clip == null)
        {
            SetupEnterDoorAudioSource();
        }

        if (enterDoorAudioSource.clip == null) return;

        enterDoorAudioSource.volume = enterDoorVolume;
        enterDoorAudioSource.loop = true;
        enterDoorAudioSource.Play();
    }

    void StopEnterDoorAudio()
    {
        if (enterDoorAudioSource == null) return;

        if (enterDoorAudioSource.isPlaying)
        {
            enterDoorAudioSource.Stop();
        }

        enterDoorAudioStarted = false;
    }

    void ApplyWhiteFade(float progress)
    {
        if (whiteFadeCanvasGroup == null) return;

        float delayedProgress = Mathf.InverseLerp(whiteFadeStartProgress, 1f, progress);
        delayedProgress = Mathf.Clamp01(delayedProgress);

        delayedProgress = Mathf.SmoothStep(0f, 1f, delayedProgress);
        delayedProgress = Mathf.Pow(delayedProgress, whiteFadeSoftness);

        whiteFadeCanvasGroup.alpha = delayedProgress;
    }

    void ApplySlowMotion(float progress)
    {
        float newTimeScale = Mathf.Lerp(normalTimeScale, minimumTimeScale, progress);

        Time.timeScale = newTimeScale;
        Time.fixedDeltaTime = originalFixedDeltaTime * Time.timeScale;
    }

    void ApplyBackgroundFade(float progress)
    {
        if (!fadeBackgroundSoundsWhileEntering) return;
        if (backgroundAudioSourcesToFade == null) return;
        if (originalBackgroundVolumes == null) return;

        float t = Mathf.SmoothStep(0f, 1f, progress);

        for (int i = 0; i < backgroundAudioSourcesToFade.Length; i++)
        {
            if (backgroundAudioSourcesToFade[i] == null) continue;
            if (backgroundAudioSourcesToFade[i] == enterDoorAudioSource) continue;
            if (i >= originalBackgroundVolumes.Length) continue;

            float targetVolume = Mathf.Lerp(originalBackgroundVolumes[i], minimumBackgroundVolume, t);
            backgroundAudioSourcesToFade[i].volume = targetVolume;
        }
    }

    void RestoreBackgroundVolumes()
    {
        if (backgroundAudioSourcesToFade == null) return;
        if (originalBackgroundVolumes == null) return;

        for (int i = 0; i < backgroundAudioSourcesToFade.Length; i++)
        {
            if (backgroundAudioSourcesToFade[i] == null) continue;
            if (i >= originalBackgroundVolumes.Length) continue;

            backgroundAudioSourcesToFade[i].volume = originalBackgroundVolumes[i];
        }
    }

    void ResetApproachEffect()
    {
        sequenceStarted = false;

        if (whiteFadeCanvasGroup != null)
        {
            whiteFadeCanvasGroup.alpha = 0f;
        }

        StopEnterDoorAudio();
        RestoreBackgroundVolumes();

        if (!finalSequenceStarted)
        {
            PauseMenu.CanPause = true;
        }

        Time.timeScale = normalTimeScale;
        Time.fixedDeltaTime = originalFixedDeltaTime;
    }

    void DisablePause()
    {
        PauseMenu.CanPause = false;

        PauseMenu pauseMenu = FindAnyObjectByType<PauseMenu>();

        if (pauseMenu != null)
        {
            pauseMenu.ForceClosePauseMenu();
        }
    }

    IEnumerator FinalSequence()
    {
        if (showDebugLogs)
        {
            Debug.Log("Final iniciado. Jugador congelado.");
        }

        DisablePause();
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

        if (blackFadeCanvasGroup != null)
        {
            blackFadeCanvasGroup.gameObject.SetActive(true);
            blackFadeCanvasGroup.blocksRaycasts = true;
            blackFadeCanvasGroup.interactable = false;

            if (fadeOutEnterDoorAudioWithBlackFade)
            {
                StartCoroutine(FadeOutEnterDoorAudio(blackFadeDuration));
            }

            yield return StartCoroutine(FadeCanvasGroupUnscaled(
                blackFadeCanvasGroup,
                blackFadeCanvasGroup.alpha,
                1f,
                blackFadeDuration
            ));
        }

        StopEnterDoorAudio();

        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDeltaTime;

        PauseMenu.CanPause = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

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
        PlayerFootsteps footsteps = player.GetComponent<PlayerFootsteps>();

        if (footsteps != null)
        {
            footsteps.enabled = false;
        }

        AudioSource[] playerAudioSources = player.GetComponentsInChildren<AudioSource>();

        for (int i = 0; i < playerAudioSources.Length; i++)
        {
            if (playerAudioSources[i] != null)
            {
                playerAudioSources[i].Stop();
            }
        }

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

    IEnumerator FadeOutEnterDoorAudio(float duration)
    {
        if (enterDoorAudioSource == null) yield break;

        float startVolume = enterDoorAudioSource.volume;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / duration);
            t = Mathf.SmoothStep(0f, 1f, t);

            enterDoorAudioSource.volume = Mathf.Lerp(startVolume, 0f, t);

            yield return null;
        }

        enterDoorAudioSource.volume = 0f;
    }

    IEnumerator FadeCanvasGroupUnscaled(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
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

    IEnumerator FadeText(float startAlpha, float endAlpha, float duration)
    {
        if (finalText == null) yield break;

        float timer = 0f;
        finalText.alpha = startAlpha;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / duration);

            t = Mathf.SmoothStep(0f, 1f, t);

            finalText.alpha = Mathf.Lerp(startAlpha, endAlpha, t);

            yield return null;
        }

        finalText.alpha = endAlpha;
    }

    private void OnDisable()
    {
        StopEnterDoorAudio();

        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDeltaTime;
        PauseMenu.CanPause = true;
    }
}