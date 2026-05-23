using UnityEngine;

public class AnomalyManager : MonoBehaviour
{
    [Header("Anomalías de la escena")]
    [SerializeField] public AnomalyTarget[] anomalies;

    [Header("Debug")]
    [SerializeField] bool showDebugLogs = true;

    public int fixedAnomalies;

    public bool AllAnomaliesFixed => fixedAnomalies >= anomalies.Length;

    private void Start()
    {
        fixedAnomalies = 0;

        if (showDebugLogs)
        {
            Debug.Log("Anomalías totales: " + anomalies.Length);
        }
    }

    public void RegisterFixedAnomaly(AnomalyTarget anomaly)
    {
        if (anomaly == null) return;

        fixedAnomalies++;

        if (showDebugLogs)
        {
            Debug.Log("Anomalía corregida registrada: " + anomaly.gameObject.name);
            Debug.Log("Progreso de anomalías: " + fixedAnomalies + " / " + anomalies.Length);
        }

        if (AllAnomaliesFixed)
        {
            Debug.Log("Todas las anomalías han sido corregidas. La puerta ya no se alejará.");
        }
    }
}