using UnityEngine;
using UnityEngine.Events;

public class AnomalyTarget : MonoBehaviour
{
    [Header("Estado")]
    [SerializeField] bool isFixed;

    [Header("Versiones del objeto")]
    [SerializeField] GameObject anomalyVersion;
    [SerializeField] GameObject fixedVersion;

    [Header("Eventos Opcionales")]
    [SerializeField] UnityEvent onFixed;

    public bool IsFixed => isFixed;

    private void Start()
    {
        if (!isFixed)
        {
            if (anomalyVersion != null)
                anomalyVersion.SetActive(true);

            if (fixedVersion != null)
                fixedVersion.SetActive(false);
        }
        else
        {
            if (anomalyVersion != null)
                anomalyVersion.SetActive(false);

            if (fixedVersion != null)
                fixedVersion.SetActive(true);
        }
    }

    public void SetDetectionProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);

        Debug.Log($"{gameObject.name} progreso de corrección: {Mathf.RoundToInt(progress * 100f)}%");
    }

    public void FixAnomaly()
    {
        if (isFixed) return;

        isFixed = true;

        Debug.Log("Anomalía corregida: " + gameObject.name);

        if (anomalyVersion != null)
            anomalyVersion.SetActive(false);

        if (fixedVersion != null)
            fixedVersion.SetActive(true);

        onFixed?.Invoke();
    }
}