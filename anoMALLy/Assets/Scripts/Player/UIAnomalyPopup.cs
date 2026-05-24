using System.Collections;
using UnityEngine;

public class UIAnomalyPopup : MonoBehaviour
{
    public static UIAnomalyPopup Instance;

    public enum SlideDirection
    {
        Right,
        Left,
        Up,
        Down
    }

    [Header("Referencias")]
    [SerializeField] RectTransform panelRect;

    [Header("Retraso solo al corregir")]
    [Tooltip("Tiempo que espera antes de mostrar el cuaderno cuando corriges una anomalía.")]
    [SerializeField] float delayBeforeShowing = 1.5f;

    [Header("Movimiento")]
    [SerializeField] SlideDirection slideDirection = SlideDirection.Right;

    [Tooltip("Cuánto se mueve el panel cuando aparece.")]
    [SerializeField] float slideDistance = 430f;

    [Tooltip("Duración del movimiento de entrada y salida.")]
    [SerializeField] float moveDuration = 0.35f;

    [Tooltip("Tiempo que se queda visible antes de volver a esconderse.")]
    [SerializeField] float visibleTime = 3f;

    Vector2 hiddenPosition;
    Vector2 visiblePosition;

    Coroutine popupCoroutine;

    private void Awake()
    {
        Instance = this;

        if (panelRect == null)
        {
            panelRect = GetComponent<RectTransform>();
        }
    }

    private void Start()
    {
        hiddenPosition = panelRect.anchoredPosition;
        visiblePosition = hiddenPosition + GetDirectionVector() * slideDistance;

        panelRect.anchoredPosition = hiddenPosition;
    }

    Vector2 GetDirectionVector()
    {
        switch (slideDirection)
        {
            case SlideDirection.Right:
                return Vector2.right;

            case SlideDirection.Left:
                return Vector2.left;

            case SlideDirection.Up:
                return Vector2.up;

            case SlideDirection.Down:
                return Vector2.down;
        }

        return Vector2.right;
    }

    // Para cuando corriges una anomalía: espera a que se vaya Fixing Anomaly.
    public void ShowPopup()
    {
        StartPopup(delayBeforeShowing);
    }

    // Para cuando fallas una anomalía: aparece directamente.
    public void ShowPopupInstant()
    {
        StartPopup(0f);
    }

    void StartPopup(float delay)
    {
        if (popupCoroutine != null)
        {
            StopCoroutine(popupCoroutine);
        }

        popupCoroutine = StartCoroutine(PopupRoutine(delay));
    }

    IEnumerator PopupRoutine(float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSecondsRealtime(delay);
        }

        yield return StartCoroutine(MovePanel(
            panelRect.anchoredPosition,
            visiblePosition,
            moveDuration
        ));

        yield return new WaitForSecondsRealtime(visibleTime);

        yield return StartCoroutine(MovePanel(
            panelRect.anchoredPosition,
            hiddenPosition,
            moveDuration
        ));

        popupCoroutine = null;
    }

    IEnumerator MovePanel(Vector2 startPos, Vector2 endPos, float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / duration);
            t = Mathf.SmoothStep(0f, 1f, t);

            panelRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

            yield return null;
        }

        panelRect.anchoredPosition = endPos;
    }
}