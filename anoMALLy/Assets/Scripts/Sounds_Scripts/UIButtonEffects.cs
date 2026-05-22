using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class UIButtonEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public static bool ButtonsLocked = false;

    [Header("Audio")]
    [SerializeField] int hoverSFXIndex = 4; // Buttons_select
    [SerializeField, Range(0f, 1f)] float hoverVolume = 0.35f;

    [SerializeField] int clickSFXIndex = 3; // Buttons_Click
    [SerializeField, Range(0f, 1f)] float clickVolume = 0.6f;

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
            buttonImage = GetComponent<Image>();

        if (buttonText == null)
            buttonText = GetComponentInChildren<TMP_Text>();

        SetNormalVisuals();
    }

    private void OnEnable()
    {
        ButtonsLocked = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ButtonsLocked) return;

        PlaySFXWithVolume(hoverSFXIndex, hoverVolume);

        if (buttonImage != null)
            buttonImage.color = hoverColor;

        if (buttonText != null)
            buttonText.color = hoverTextColor;

        if (useScaleEffect)
            transform.localScale = Vector3.one * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ButtonsLocked) return;

        SetNormalVisuals();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (ButtonsLocked) return;

        PlaySFXWithVolume(clickSFXIndex, clickVolume);

        if (buttonImage != null)
            buttonImage.color = clickColor;
    }

    public static void LockButtons()
    {
        ButtonsLocked = true;
    }

    void SetNormalVisuals()
    {
        if (buttonImage != null)
            buttonImage.color = normalColor;

        if (buttonText != null)
            buttonText.color = normalTextColor;

        if (useScaleEffect)
            transform.localScale = Vector3.one * normalScale;
    }

    void PlaySFXWithVolume(int sfxIndex, float volume)
    {
        if (AudioManager.Instance == null) return;
        if (AudioManager.Instance.sfxSource == null) return;
        if (AudioManager.Instance.sfxLibrary == null) return;
        if (sfxIndex < 0 || sfxIndex >= AudioManager.Instance.sfxLibrary.Length) return;

        AudioClip clip = AudioManager.Instance.sfxLibrary[sfxIndex];
        if (clip == null) return;

        AudioManager.Instance.sfxSource.PlayOneShot(clip, volume);
    }
}