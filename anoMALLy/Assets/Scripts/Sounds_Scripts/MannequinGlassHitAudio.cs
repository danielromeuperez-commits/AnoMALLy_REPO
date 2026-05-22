using UnityEngine;

public class MannequinGlassHitAudio : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] Animator animator;

    [Header("Frames de golpes")]
    [SerializeField] int[] hitFrames = { 24, 102, 121, 129, 136, 144 };

    [Tooltip("Número total de frames de la animación.")]
    [SerializeField] int totalAnimationFrames = 149;

    [Header("Audio local 3D")]
    [SerializeField] AudioClip glassHitClip;
    [SerializeField] AudioSource glassHitAudioSource;
    [SerializeField, Range(0f, 1f)] float volume = 0.5f;

    [Header("Distancia del sonido")]
    [SerializeField] float minDistance = 1f;
    [SerializeField] float maxDistance = 8f;

    bool[] playedHitsThisLoop;
    int lastLoopIndex = -1;
    int lastFrame = -1;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (glassHitAudioSource == null)
        {
            glassHitAudioSource = GetComponent<AudioSource>();
        }

        if (glassHitAudioSource == null)
        {
            glassHitAudioSource = gameObject.AddComponent<AudioSource>();
        }

        SetupAudioSource();
        SetupPlayedArray();
    }

    private void OnValidate()
    {
        SetupPlayedArray();
    }

    private void Update()
    {
        if (animator == null) return;
        if (hitFrames == null || hitFrames.Length == 0) return;
        if (totalAnimationFrames <= 0) return;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        float normalizedTime = stateInfo.normalizedTime;
        int currentLoopIndex = Mathf.FloorToInt(normalizedTime);
        float currentLoopProgress = normalizedTime % 1f;

        int currentFrame = Mathf.FloorToInt(currentLoopProgress * totalAnimationFrames);

        if (currentLoopIndex != lastLoopIndex)
        {
            lastLoopIndex = currentLoopIndex;
            lastFrame = -1;
            ResetPlayedHits();
        }

        for (int i = 0; i < hitFrames.Length; i++)
        {
            if (i >= playedHitsThisLoop.Length) continue;
            if (playedHitsThisLoop[i]) continue;

            int targetFrame = hitFrames[i];

            if (lastFrame < targetFrame && currentFrame >= targetFrame)
            {
                PlayGlassHitSound();
                playedHitsThisLoop[i] = true;
            }
        }

        lastFrame = currentFrame;
    }

    void SetupAudioSource()
    {
        if (glassHitAudioSource == null) return;

        glassHitAudioSource.playOnAwake = false;
        glassHitAudioSource.loop = false;
        glassHitAudioSource.spatialBlend = 1f;
        glassHitAudioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        glassHitAudioSource.minDistance = minDistance;
        glassHitAudioSource.maxDistance = maxDistance;
        glassHitAudioSource.volume = volume;
    }

    void PlayGlassHitSound()
    {
        if (glassHitAudioSource == null) return;
        if (glassHitClip == null) return;

        glassHitAudioSource.PlayOneShot(glassHitClip, volume);
    }

    void SetupPlayedArray()
    {
        if (hitFrames == null)
        {
            playedHitsThisLoop = new bool[0];
            return;
        }

        if (playedHitsThisLoop == null || playedHitsThisLoop.Length != hitFrames.Length)
        {
            playedHitsThisLoop = new bool[hitFrames.Length];
        }
    }

    void ResetPlayedHits()
    {
        SetupPlayedArray();

        for (int i = 0; i < playedHitsThisLoop.Length; i++)
        {
            playedHitsThisLoop[i] = false;
        }
    }
}