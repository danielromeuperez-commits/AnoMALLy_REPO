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
    public float TimeToFix => timeToFix;
    public float CurrentFixTime => currentFixTime;
    [SerializeField] bool resetProgressWhenNotLooking = true;

    [Header("Resaltado")]
    [SerializeField] float timeToShowHighlight = 1f;

    [Header("Debug")]
    [SerializeField] bool showDebugRay = true;

    bool isHoldingDetect;
    float currentFixTime;
    AnomalyTarget currentAnomaly;
    public Animator animator;

    bool esperandoRespuesta = false;
    bool detectedCorrectly;
    bool highlightShown;

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

        if (Physics.Raycast(ray, out RaycastHit hit, detectionDistance))
        {
            // ¿El objeto golpeado pertenece a anomalyLayer?
            if (((1 << hit.collider.gameObject.layer) & anomalyLayer) != 0)
            {
                AnomalyTarget anomaly = hit.collider.GetComponentInParent<AnomalyTarget>();

                if (anomaly != null && !anomaly.IsFixed)
                {
                    if (currentAnomaly != anomaly)
                    {
                        if (currentAnomaly != null)
                        {
                            currentAnomaly.StopHighlight();
                        }

                        currentAnomaly = anomaly;
                        currentFixTime = 0f;
                        highlightShown = false;

                        Debug.Log("Detectando: " + anomaly.gameObject.name);
                    }

                    currentFixTime += Time.deltaTime;

                    float progress = currentFixTime / timeToFix;
                    currentAnomaly.SetDetectionProgress(progress);

                    if (!highlightShown && currentFixTime >= timeToShowHighlight)
                    {
                        currentAnomaly.StartHighlight();
                        highlightShown = true;
                    }

                    if (currentFixTime >= timeToFix && !esperandoRespuesta)
                    {
                        MostrarMenu();
                    }

                    return;
                }
            }
        }

        // Si no está mirando una anomalía válida
        if (resetProgressWhenNotLooking)
        {
            ResetDetection();
        }

        void MostrarMenu()
        {
            esperandoRespuesta = true;

            if (currentAnomaly != null)
            {
                currentAnomaly.StopHighlight();
            }

            menuUI.SetActive(true);

            Time.timeScale = 0f;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            playerController.SetControls(false);
        }
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

            if (currentAnomaly != null)
            {
                currentAnomaly.PlayWrongSFX();
                currentAnomaly.StopHighlight();
            }

            animator.SetTrigger("Wrong");
            animator.SetBool("Point", false);
            isHoldingDetect = false;
            detectedCorrectly = false;

            if (intentos <= 0)
            {
                SceneManager.LoadScene(0);
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

        if (currentAnomaly != null)
        {
            currentAnomaly.StopHighlight();
            currentAnomaly.FixAnomaly();
        }

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
            currentAnomaly.StopHighlight();
        }

        currentAnomaly = null;
        currentFixTime = 0f;
        highlightShown = false;
    }
}