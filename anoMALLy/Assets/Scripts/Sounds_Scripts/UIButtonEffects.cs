using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class UIButtonEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Audio")]
    [SerializeField] int hoverSFXIndex = 4; // Buttons_select
    [SerializeField] int clickSFXIndex = 3; // Buttons_Click

    [Header("Color del botón")]
    [SerializeField] Image buttonImage;
    [SerializeField] Color normalColor = Color.white;
    [SerializeField] Color hoverColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    [SerializeField] Color clickColor = new Color(0.6f, 0.6f, 0.6f, 1f);

    [Header("Color del texto")]
    [SerializeField] TMP_Text buttonText;
    [SerializeField] Color normalTextColor = Color.black;
    [SerializeField] Color hoverTextColor = Color.black;

    [Header("Escala")]
    [SerializeField] bool useScaleEffect = true;
    [SerializeField] float normalScale = 1f;
    [SerializeField] float hoverScale = 1.05f;

    private void Awake()
    {
        if (buttonImage == null)
        {
            buttonImage = GetComponent<Image>();
        }

        if (buttonText == null)
        {
            buttonText = GetComponentInChildren<TMP_Text>();
        }

        SetNormalVisuals();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(hoverSFXIndex);
        }

        if (buttonImage != null)
        {
            buttonImage.color = hoverColor;
        }

        if (buttonText != null)
        {
            buttonText.color = hoverTextColor;
        }

        if (useScaleEffect)
        {
            transform.localScale = Vector3.one * hoverScale;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetNormalVisuals();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(clickSFXIndex);
        }

        if (buttonImage != null)
        {
            buttonImage.color = clickColor;
        }
    }

    void SetNormalVisuals()
    {
        if (buttonImage != null)
        {
            buttonImage.color = normalColor;
        }

        if (buttonText != null)
        {
            buttonText.color = normalTextColor;
        }

        if (useScaleEffect)
        {
            transform.localScale = Vector3.one * normalScale;
        }
    }
}