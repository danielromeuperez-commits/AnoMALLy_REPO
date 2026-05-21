using UnityEngine;

public class NeonLightAudio : MonoBehaviour
{
    [Header("Audio neón")]
    [SerializeField] int neonSFXIndex = 13;
    [SerializeField] AudioSource neonAudioSource;

    [Header("Volumen")]
    [SerializeField, Range(0f, 1f)] float neonVolume = 0.25f;

    [Header("Opciones")]
    [SerializeField] bool playOnStart = true;
    [SerializeField] bool use3DSound = true;

    private void Start()
    {
        if (neonAudioSource == null)
        {
            neonAudioSource = GetComponent<AudioSource>();
        }

        if (neonAudioSource == null)
        {
            neonAudioSource = gameObject.AddComponent<AudioSource>();
        }

        SetupAudioSource();

        if (playOnStart)
        {
            PlayNeonAudio();
        }
    }

    void SetupAudioSource()
    {
        if (neonAudioSource == null) return;

        neonAudioSource.playOnAwake = false;
        neonAudioSource.loop = true;
        neonAudioSource.volume = neonVolume;
        neonAudioSource.spatialBlend = use3DSound ? 1f : 0f;

        if (AudioManager.Instance == null) return;
        if (AudioManager.Instance.sfxLibrary == null) return;
        if (neonSFXIndex < 0 || neonSFXIndex >= AudioManager.Instance.sfxLibrary.Length) return;

        neonAudioSource.clip = AudioManager.Instance.sfxLibrary[neonSFXIndex];
    }

    public void PlayNeonAudio()
    {
        if (neonAudioSource == null) return;

        if (neonAudioSource.clip == null)
        {
            SetupAudioSource();
        }

        if (neonAudioSource.clip == null) return;

        neonAudioSource.volume = neonVolume;
        neonAudioSource.loop = true;

        if (!neonAudioSource.isPlaying)
        {
            neonAudioSource.Play();
        }
    }

    public void StopNeonAudio()
    {
        if (neonAudioSource == null) return;

        neonAudioSource.Stop();
    }
}