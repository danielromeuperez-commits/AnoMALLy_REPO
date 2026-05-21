using UnityEngine;

public class EyesManagerTrigger : MonoBehaviour
{
    [Header("Objetos")]
    [SerializeField] GameObject eyes;
    [SerializeField] GameObject trigger;

    [Header("Audio al aparecer")]
    [SerializeField] bool playAppearSFX = true;
    [SerializeField] int appearSFXIndex = 2; // JumpScare

    [Header("Audio constante de ojos")]
    [SerializeField] AudioSource eyesAudioSource;
    [SerializeField, Range(0f, 1f)] float eyesVolume = 0.5f;

    bool hasTriggered;

    private void Awake()
    {
        if (eyesAudioSource == null && eyes != null)
        {
            eyesAudioSource = eyes.GetComponent<AudioSource>();
        }

        if (eyesAudioSource != null)
        {
            eyesAudioSource.playOnAwake = false;
            eyesAudioSource.loop = true;
            eyesAudioSource.volume = eyesVolume;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        hasTriggered = true;

        if (eyes != null)
        {
            eyes.SetActive(true);
        }

        if (trigger != null)
        {
            trigger.SetActive(false);
        }

        if (playAppearSFX && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(appearSFXIndex);
        }

        PlayEyesLoopAudio();
    }

    void PlayEyesLoopAudio()
    {
        if (eyesAudioSource == null) return;

        eyesAudioSource.loop = true;
        eyesAudioSource.volume = eyesVolume;

        if (!eyesAudioSource.isPlaying)
        {
            eyesAudioSource.Play();
        }
    }

    public void StopEyesAudio()
    {
        if (eyesAudioSource == null) return;

        eyesAudioSource.Stop();
    }
}