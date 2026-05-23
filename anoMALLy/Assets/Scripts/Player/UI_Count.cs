using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_CountAnomalies : MonoBehaviour
{
    public TMP_Text textAnomaly;
    public AnomalyDetector AnoDet;

    void Update()
    {
        ActualizarUI();
    }

    void ActualizarUI()
    {
        textAnomaly.text = AnoDet.intentos + " Attempts";
    }
}