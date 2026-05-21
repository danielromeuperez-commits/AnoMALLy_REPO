using UnityEngine;

public class PlayerFootsteps : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] int footstepSFXIndex = 10;
    [SerializeField] AudioSource footstepAudioSource;
    [SerializeField, Range(0f, 1f)] float footstepVolume = 0.35f;

    [Header("Detección de movimiento")]
    [SerializeField] Transform playerTransform;

    [Tooltip("Cuánto tiene que moverse realmente para considerarlo caminar. Súbelo si detecta micro saltitos.")]
    [SerializeField] float minMoveDistance = 0.025f;

    [Header("Ritmo de pasos")]
    [SerializeField] float walkStepInterval = 0.8f;
    [SerializeField] float runStepInterval = 0.5f;
    [SerializeField] bool useRunInterval = false;

    [Header("Detección de suelo")]
    [SerializeField] bool checkGrounded = false;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float groundCheckDistance = 1.3f;

    [Header("Opciones")]
    [SerializeField] bool playOnlyWhenMoving = true;
    [SerializeField] float randomPitchMin = 0.95f;
    [SerializeField] float randomPitchMax = 1.05f;

    [Header("Antisolapamiento")]
    [SerializeField] bool preventOverlappingSteps = true;

    [Header("Evitar cortes por microparadas")]
    [Tooltip("Tiempo que espera antes de cortar el paso cuando detecta que has dejado de moverte.")]
    [SerializeField] float stopDelay = 0.18f;

    Vector3 lastPosition;
    float stepTimer;
    float notMovingTimer;
    bool wasMovingLastFrame;

    private void Start()
    {
        if (playerTransform == null)
        {
            playerTransform = transform;
        }

        if (footstepAudioSource == null)
        {
            footstepAudioSource = GetComponent<AudioSource>();
        }

        if (footstepAudioSource == null)
        {
            footstepAudioSource = gameObject.AddComponent<AudioSource>();
        }

        footstepAudioSource.playOnAwake = false;
        footstepAudioSource.loop = false;
        footstepAudioSource.spatialBlend = 0f;
        footstepAudioSource.volume = footstepVolume;

        lastPosition = playerTransform.position;
        stepTimer = walkStepInterval;
    }

    private void Update()
    {
        if (playerTransform == null) return;

        bool isMoving = IsPlayerMoving();
        bool isGrounded = IsGrounded();

        if (checkGrounded && !isGrounded)
        {
            StopFootstepImmediately();

            wasMovingLastFrame = false;
            stepTimer = 0f;
            notMovingTimer = 0f;
            lastPosition = playerTransform.position;
            return;
        }

        if (playOnlyWhenMoving && !isMoving)
        {
            notMovingTimer += Time.deltaTime;

            if (notMovingTimer >= stopDelay)
            {
                StopFootstepImmediately();

                wasMovingLastFrame = false;
                stepTimer = 0f;
            }

            lastPosition = playerTransform.position;
            return;
        }

        notMovingTimer = 0f;

        float currentInterval = useRunInterval ? runStepInterval : walkStepInterval;

        if (!wasMovingLastFrame && isMoving)
        {
            stepTimer = currentInterval;
        }

        stepTimer += Time.deltaTime;

        if (stepTimer >= currentInterval)
        {
            PlayFootstep();
            stepTimer = 0f;
        }

        wasMovingLastFrame = isMoving;
        lastPosition = playerTransform.position;
    }

    bool IsPlayerMoving()
    {
        Vector3 currentPosition = playerTransform.position;

        Vector3 flatCurrentPosition = new Vector3(currentPosition.x, 0f, currentPosition.z);
        Vector3 flatLastPosition = new Vector3(lastPosition.x, 0f, lastPosition.z);

        float distanceMoved = Vector3.Distance(flatCurrentPosition, flatLastPosition);

        return distanceMoved > minMoveDistance;
    }

    bool IsGrounded()
    {
        if (!checkGrounded) return true;

        return Physics.Raycast(
            playerTransform.position,
            Vector3.down,
            groundCheckDistance,
            groundLayer
        );
    }

    void PlayFootstep()
    {
        if (AudioManager.Instance == null) return;
        if (AudioManager.Instance.sfxLibrary == null) return;
        if (footstepSFXIndex < 0 || footstepSFXIndex >= AudioManager.Instance.sfxLibrary.Length) return;

        AudioClip clip = AudioManager.Instance.sfxLibrary[footstepSFXIndex];

        if (clip == null) return;
        if (footstepAudioSource == null) return;

        if (preventOverlappingSteps && footstepAudioSource.isPlaying)
        {
            return;
        }

        footstepAudioSource.clip = clip;
        footstepAudioSource.volume = footstepVolume;
        footstepAudioSource.pitch = Random.Range(randomPitchMin, randomPitchMax);
        footstepAudioSource.Play();
    }

    void StopFootstepImmediately()
    {
        if (footstepAudioSource == null) return;

        if (footstepAudioSource.isPlaying)
        {
            footstepAudioSource.Stop();
        }
    }
}