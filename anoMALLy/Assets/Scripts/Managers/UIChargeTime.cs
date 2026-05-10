using UnityEngine;
using UnityEngine.UI;

public class UIChargeTime : MonoBehaviour
{
    [SerializeField] private Image chargedDetectorFill;
    [SerializeField] private float smoothSpeed = 5f;

    [SerializeField] private AnomalyDetector anodetector;

    void Update()
    {
        if (chargedDetectorFill == null || anodetector == null)
            return;

        float targetFill =
            anodetector.CurrentFixTime /
            anodetector.TimeToFix;

        chargedDetectorFill.fillAmount = Mathf.Lerp(
            chargedDetectorFill.fillAmount,
            targetFill,
            Time.deltaTime * smoothSpeed
        );
    }
}