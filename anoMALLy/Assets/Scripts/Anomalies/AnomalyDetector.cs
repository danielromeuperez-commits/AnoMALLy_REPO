using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AnomalyDetector : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] Camera playerCamera;
    [SerializeField] GameObject menuUI;
    [SerializeField] GameObject fixingAnomalyCanvas;
    [SerializeField] PlayerController playerController;

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
    public Animator animator;

    bool esperandoRespuesta = false;
    bool detectedCorrectly;

    public int intentos = 3;

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

        if (!isHoldingDetect || esperandoRespuesta)
            return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (showDebugRay)
        {
            Debug.DrawRay(ray.origin, ray.direction * detectionDistance, Color.red);
        }

        if (Physics.Raycast(ray, out RaycastHit hit, detectionDistance, anomalyLayer))
        {
            AnomalyTarget anomaly = hit.collider.GetComponentInParent<AnomalyTarget>();

            if (anomaly != null && !anomaly.IsFixed)
            {
                if (currentAnomaly != anomaly)
                {
                    currentAnomaly = anomaly;
                    currentFixTime = 0f;

                    Debug.Log("Detectando: " + anomaly.gameObject.name);
                }

                currentFixTime += Time.deltaTime;

                float progress = currentFixTime / timeToFix;
                currentAnomaly.SetDetectionProgress(progress);

                if (currentFixTime >= timeToFix && !esperandoRespuesta)
                {
                    MostrarMenu();
                }

                return;
            }
        }

        if (resetProgressWhenNotLooking)
        {
            ResetDetection();
        }
    }

    void MostrarMenu()
    {
        esperandoRespuesta = true;

        menuUI.SetActive(true);

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        playerController.SetControls(false);
    }

    public void ElegirAnomalia(int tipoElegido)
    {
        Time.timeScale = 1f;

        menuUI.SetActive(false);
        esperandoRespuesta = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerController.SetControls(true);

        if (currentAnomaly == null) return;

        if ((int)currentAnomaly.Tipo == tipoElegido)
        {
            StartCoroutine(FixAnomalySequence());
        }
        else
        {
            intentos--;
            animator.SetTrigger("Wrong");
            animator.SetBool("Point", false);
            isHoldingDetect = false;

            if (intentos <= 0)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            }
        }

        ResetDetection();
        SetDetecting(false);
    }

    IEnumerator FixAnomalySequence()
    {
        fixingAnomalyCanvas.SetActive(true);
        AudioManager.Instance.PlaySFX(0);
        Time.timeScale = 0f;
        currentAnomaly.FixAnomaly();
        detectedCorrectly = true;
        yield return new WaitForSecondsRealtime(2f);
        fixingAnomalyCanvas.SetActive(false);
        Time.timeScale = 1f;
        animator.SetTrigger("Correct");
        animator.SetBool("Point", false);
    }

    public bool CheckAnomaly()
    {
        return detectedCorrectly;
    }

    void ResetDetection()
    {
        if (currentAnomaly != null && !currentAnomaly.IsFixed)
        {
            currentAnomaly.SetDetectionProgress(0f);
        }

        currentAnomaly = null;
        currentFixTime = 0f;
    }
}