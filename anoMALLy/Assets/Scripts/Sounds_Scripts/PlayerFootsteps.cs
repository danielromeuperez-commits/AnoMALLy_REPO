using UnityEngine;

public class PlayerFootsteps : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] AudioClip footstepClip;
    [SerializeField] AudioSource footstepAudioSource;
    [SerializeField, Range(0f, 1f)] float footstepVolume = 0.35f;

    [Header("Detección de movimiento horizontal")]
    [SerializeField] Transform playerTransform;

    [Tooltip("Velocidad horizontal mínima para considerar que el player está caminando.")]
    [SerializeField] float minimumHorizontalSpeed = 0.08f;

    [Tooltip("Tiempo que espera antes de parar el audio al dejar de moverte.")]
    [SerializeField] float stopDelay = 0.15f;

    [Header("Opciones")]
    [SerializeField] bool use3DSound = false;
    [SerializeField] float footstepPitch = 1f;

    Vector3 lastPosition;
    float notMovingTimer;
    bool footstepsPlaying;

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

        footstepAudioSource.clip = footstepClip;
        footstepAudioSource.playOnAwake = false;
        footstepAudioSource.loop = true;
        footstepAudioSource.volume = footstepVolume;
        footstepAudioSource.pitch = footstepPitch;
        footstepAudioSource.spatialBlend = use3DSound ? 1f : 0f;

        lastPosition = playerTransform.position;
    }

    private void Update()
    {
        if (playerTransform == null) return;
        if (footstepAudioSource == null) return;
        if (footstepClip == null) return;

        bool isMovingHorizontally = IsMovingHorizontally();

        if (isMovingHorizontally)
        {
            notMovingTimer = 0f;
            StartFootsteps();
        }
        else
        {
            notMovingTimer += Time.deltaTime;

            if (notMovingTimer >= stopDelay)
            {
                StopFootsteps();
            }
        }

        lastPosition = playerTransform.position;
    }

    bool IsMovingHorizontally()
    {
        Vector3 currentPosition = playerTransform.position;

        Vector3 currentFlatPosition = new Vector3(
            currentPosition.x,
            0f,
            currentPosition.z
        );

        Vector3 lastFlatPosition = new Vector3(
            lastPosition.x,
            0f,
            lastPosition.z
        );

        float distance = Vector3.Distance(currentFlatPosition, lastFlatPosition);
        float horizontalSpeed = distance / Mathf.Max(Time.deltaTime, 0.0001f);

        return horizontalSpeed > minimumHorizontalSpeed;
    }

    void StartFootsteps()
    {
        if (footstepsPlaying) return;

        footstepsPlaying = true;

        footstepAudioSource.clip = footstepClip;
        footstepAudioSource.volume = footstepVolume;
        footstepAudioSource.pitch = footstepPitch;
        footstepAudioSource.loop = true;

        footstepAudioSource.Play();
    }

    void StopFootsteps()
    {
        if (!footstepsPlaying) return;

        footstepsPlaying = false;

        if (footstepAudioSource.isPlaying)
        {
            footstepAudioSource.Stop();
        }
    }

    private void OnDisable()
    {
        StopFootsteps();
    }
}