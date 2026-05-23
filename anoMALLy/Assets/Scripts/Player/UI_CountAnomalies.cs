using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Count : MonoBehaviour
{
    public TMP_Text textAnomaly;
    public AnomalyManager AnoMan;

    void Update()
    {
        ActualizarUI();
    }

    void ActualizarUI()
    {
        textAnomaly.text = "Fixed Anomalies " + AnoMan.fixedAnomalies + "/ Total Anomalies " + AnoMan.anomalies.Length;
    }
}