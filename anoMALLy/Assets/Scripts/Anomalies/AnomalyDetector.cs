using UnityEngine;

public class AnomalyDetector : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] Camera playerCamera;

    [Header("Raycast")]
    [SerializeField] float detectionDistance = 5f;
    [SerializeField] LayerMask anomalyLayer;

    [Header("Detección")]
    [SerializeField] float timeToFix = 2f;
    [SerializeField] bool resetProgressWhenNotLooking = true;

    [Header("Debug")]
    [SerializeField] bool showDebugRay = true;

    bool isHoldingDetect;
    float currentFixTime;
    AnomalyTarget currentAnomaly;

    private void Update()
    {
        DetectAnomaly();
    }

    public void SetDetecting(bool value)
    {
        isHoldingDetect = value;

        if (!isHoldingDetect)
        {
            ResetDetection();
        }
    }

    void DetectAnomaly()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (showDebugRay)
        {
            Debug.DrawRay(ray.origin, ray.direction * detectionDistance, Color.red);
        }

        if (!isHoldingDetect)
            return;

        if (Physics.Raycast(ray, out RaycastHit hit, detectionDistance, anomalyLayer))
        {
            AnomalyTarget anomaly = hit.collider.GetComponentInParent<AnomalyTarget>();

            if (anomaly != null && !anomaly.IsFixed)
            {
                if (currentAnomaly != anomaly)
                {
                    currentAnomaly = anomaly;
                    currentFixTime = 0f;

                    Debug.Log("Detectando anomalía: " + anomaly.gameObject.name);
                }

                currentFixTime += Time.deltaTime;

                float progress = currentFixTime / timeToFix;
                currentAnomaly.SetDetectionProgress(progress);

                if (currentFixTime >= timeToFix)
                {
                    currentAnomaly.FixAnomaly();
                    ResetDetection();
                }

                return;
            }
        }

        if (resetProgressWhenNotLooking)
        {
            ResetDetection();
        }
    }

    void ResetDetection()
    {
        if (currentAnomaly != null && !currentAnomaly.IsFixed)
        {
            currentAnomaly.SetDetectionProgress(0f);
            Debug.Log("Corrección cancelada: " + currentAnomaly.gameObject.name);
        }

        currentAnomaly = null;
        currentFixTime = 0f;
    }
}