using UnityEngine;
using UnityEngine.UI;

public class UIChargeTime : MonoBehaviour
{
    [SerializeField] private Image chargedDetectorFill;
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private float delayBeforeCharge = 0.5f;

    [SerializeField] private AnomalyDetector anodetector;

    private float delayTimer;
    private bool startedCharging;

    void Update()
    {
        if (chargedDetectorFill == null || anodetector == null)
            return;

        // Está cargando
        if (anodetector.CurrentFixTime > 0f)
        {
            // Espera inicial
            if (!startedCharging)
            {
                delayTimer += Time.deltaTime;

                chargedDetectorFill.fillAmount = 0f;

                if (delayTimer >= delayBeforeCharge)
                {
                    startedCharging = true;
                }

                return;
            }

            // Progreso visual desde 0
            float visualProgress =
                (anodetector.CurrentFixTime - delayBeforeCharge) /
                (anodetector.TimeToFix - delayBeforeCharge);

            visualProgress = Mathf.Clamp01(visualProgress);

            chargedDetectorFill.fillAmount = Mathf.Lerp(
                chargedDetectorFill.fillAmount,
                visualProgress,
                Time.deltaTime * smoothSpeed
            );
        }
        else
        {
            // Reset
            delayTimer = 0f;
            startedCharging = false;

            chargedDetectorFill.fillAmount = Mathf.Lerp(
                chargedDetectorFill.fillAmount,
                0f,
                Time.deltaTime * smoothSpeed
            );
        }
    }
}