using UnityEngine;
using UnityEngine.Events;

public class AnomalyTarget : MonoBehaviour
{
    public enum TipoAnomalia
    {
        Sound,
        Entity,
        ExtraObject,
        Texture,
        ModifiedObject
    }

    [Header("Tipo")]
    [SerializeField] TipoAnomalia tipo;
    public TipoAnomalia Tipo => tipo;

    [Header("Estado")]
    [SerializeField] bool isFixed;
    public bool IsFixed => isFixed;

    [Header("Objeto alternativo (para cambiar)")]
    [SerializeField] GameObject objetoCorregido;

    [Header("Managers")]
    [SerializeField] AnomalyManager anomalyManager;

    [Header("Eventos Opcionales")]
    [SerializeField] UnityEvent onFixed;

    [Header("Audio SFX")]
    [SerializeField] bool playCorrectSFX = true;
    [SerializeField] int correctSFXIndex = 0;

    [SerializeField] bool playWrongSFX = true;
    [SerializeField] int wrongSFXIndex = 1;

    [Header("Audio ambiente / estática")]
    [SerializeField] bool playAmbientAudioOnStart = false;
    [SerializeField] bool stopAmbientAudioOnFixed = true;
    [SerializeField] AudioSource ambientAudioSource;

    [Header("Resaltado de borde")]
    [SerializeField] bool useOutline = true;
    [SerializeField] AnomalyOutline anomalyOutline;

    [Header("Sound Anomaly")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] float pitchAnomaly = -1f;
    [SerializeField] float pitchDefault = 1f;

    private void Awake()
    {
        if (anomalyManager == null)
        {
            anomalyManager = FindAnyObjectByType<AnomalyManager>();
        }

        switch (gameObject.tag)
        {
            case "sound_anomaly":
                tipo = TipoAnomalia.Sound;
                break;

            case "entity_anomalies":
                tipo = TipoAnomalia.Entity;
                break;

            case "extra_obj_anomaly":
                tipo = TipoAnomalia.ExtraObject;
                break;

            case "texture_anomaly":
                tipo = TipoAnomalia.Texture;
                break;

            case "modificated_obj_anomaly":
                tipo = TipoAnomalia.ModifiedObject;
                break;
        }

        if (tipo == TipoAnomalia.Sound)
        {
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();

            if (!isFixed && audioSource != null)
            {
                audioSource.pitch = pitchAnomaly;
            }
        }

        SetupOutline();
    }

    private void Start()
    {
        if (anomalyManager == null)
        {
            anomalyManager = FindAnyObjectByType<AnomalyManager>();
        }

        if (objetoCorregido != null && !isFixed)
        {
            objetoCorregido.SetActive(false);
        }

        SetupAmbientAudio();
    }

    void SetupAmbientAudio()
    {
        if (!playAmbientAudioOnStart) return;

        if (ambientAudioSource == null)
        {
            ambientAudioSource = GetComponent<AudioSource>();
        }

        if (ambientAudioSource == null) return;

        ambientAudioSource.loop = true;
        ambientAudioSource.playOnAwake = false;

        if (!isFixed)
        {
            ambientAudioSource.Play();
        }
    }

    void StopAmbientAudio()
    {
        if (ambientAudioSource == null) return;

        ambientAudioSource.Stop();
    }

    void SetupOutline()
    {
        if (!useOutline) return;

        if (anomalyOutline == null)
        {
            anomalyOutline = GetComponent<AnomalyOutline>();
        }

        if (anomalyOutline == null)
        {
            anomalyOutline = gameObject.AddComponent<AnomalyOutline>();
        }

        anomalyOutline.SetVisible(false);
    }

    public void StartHighlight()
    {
        if (!useOutline) return;
        if (isFixed) return;
        if (anomalyOutline == null) return;

        anomalyOutline.SetVisible(true);
    }

    public void StopHighlight()
    {
        if (anomalyOutline == null) return;

        anomalyOutline.SetVisible(false);
    }

    public void SetDetectionProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);
        Debug.Log($"{gameObject.name} progreso: {Mathf.RoundToInt(progress * 100f)}%");
    }

    public void FixAnomaly()
    {
        if (isFixed) return;

        StopHighlight();

        isFixed = true;

        Debug.Log("Anomalía corregida: " + gameObject.name);

        if (stopAmbientAudioOnFixed)
        {
            StopAmbientAudio();
        }

        if (playCorrectSFX && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(correctSFXIndex);
        }

        switch (tipo)
        {
            case TipoAnomalia.Entity:
            case TipoAnomalia.ExtraObject:
                gameObject.SetActive(false);
                break;

            case TipoAnomalia.Texture:
            case TipoAnomalia.ModifiedObject:
                gameObject.SetActive(false);

                if (objetoCorregido != null)
                    objetoCorregido.SetActive(true);
                break;

            case TipoAnomalia.Sound:
                if (audioSource == null)
                    audioSource = gameObject.GetComponent<AudioSource>();

                if (audioSource != null)
                {
                    audioSource.pitch = pitchDefault;
                }
                break;
        }

        onFixed?.Invoke();

        if (anomalyManager == null)
        {
            anomalyManager = FindAnyObjectByType<AnomalyManager>();
        }

        if (anomalyManager != null)
        {
            anomalyManager.RegisterFixedAnomaly(this);
        }
        else
        {
            Debug.LogWarning("No hay AnomalyManager asignado en: " + gameObject.name);
        }
    }

    public void PlayWrongSFX()
    {
        if (!playWrongSFX) return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(wrongSFXIndex);
        }
    }
}