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

    [Header("Sound Anomaly")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] float pitchAnomaly = -1f;
    [SerializeField] float pitchDefault = 1f;

    private void Awake()
    {
        // Auto-detectar tipo por tag
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
    }

    private void Start()
    {
        if (objetoCorregido != null && !isFixed)
        {
            objetoCorregido.SetActive(false);
        }
    }

    public void SetDetectionProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);
        Debug.Log($"{gameObject.name} progreso: {Mathf.RoundToInt(progress * 100f)}%");
    }

    public void FixAnomaly()
    {
        if (isFixed) return;

        isFixed = true;

        Debug.Log("Anomalía corregida: " + gameObject.name);

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

        if (anomalyManager != null)
        {
            anomalyManager.RegisterFixedAnomaly(this);
        }
        else
        {
            Debug.LogWarning("No hay AnomalyManager asignado en: " + gameObject.name);
        }
    }
}