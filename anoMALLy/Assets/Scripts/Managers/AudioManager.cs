using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;

    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogWarning("No hay AudioManager en la escena.");
            }

            return instance;
        }
    }

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Librerías")]
    public AudioClip[] musicLibrary;
    public AudioClip[] sfxLibrary;

    [Header("Volúmenes")]
    [SerializeField] float defaultMusicVolume = 0.6f;
    [SerializeField] float defaultSFXVolume = 1f;

    Coroutine musicFadeCoroutine;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (musicSource != null)
        {
            musicSource.volume = defaultMusicVolume;
        }

        if (sfxSource != null)
        {
            sfxSource.volume = defaultSFXVolume;
        }
    }

    public void PlayMusic(int musicToPlay)
    {
        if (musicSource == null) return;
        if (musicLibrary == null) return;
        if (musicToPlay < 0 || musicToPlay >= musicLibrary.Length) return;
        if (musicLibrary[musicToPlay] == null) return;

        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
            musicFadeCoroutine = null;
        }

        if (musicSource.clip == musicLibrary[musicToPlay] && musicSource.isPlaying)
        {
            return;
        }

        musicSource.clip = musicLibrary[musicToPlay];
        musicSource.loop = true;
        musicSource.volume = defaultMusicVolume;
        musicSource.Play();
    }

    public void PlaySFX(int sfxToPlay)
    {
        if (sfxSource == null) return;
        if (sfxLibrary == null) return;
        if (sfxToPlay < 0 || sfxToPlay >= sfxLibrary.Length) return;
        if (sfxLibrary[sfxToPlay] == null) return;

        sfxSource.PlayOneShot(sfxLibrary[sfxToPlay]);
    }

    public void StopMusic()
    {
        if (musicSource == null) return;

        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
            musicFadeCoroutine = null;
        }

        musicSource.Stop();
    }

    public void FadeOutMusic(float duration)
    {
        if (musicSource == null) return;

        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
        }

        musicFadeCoroutine = StartCoroutine(FadeOutMusicCoroutine(duration));
    }

    IEnumerator FadeOutMusicCoroutine(float duration)
    {
        float startVolume = musicSource.volume;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float t = timer / duration;
            t = Mathf.SmoothStep(0f, 1f, t);

            musicSource.volume = Mathf.Lerp(startVolume, 0f, t);

            yield return null;
        }

        musicSource.volume = 0f;
        musicSource.Stop();

        musicFadeCoroutine = null;
    }

    public void PauseMusic()
    {
        if (musicSource == null) return;

        musicSource.Pause();
    }

    public void UnPauseMusic()
    {
        if (musicSource == null) return;

        musicSource.UnPause();
    }
}