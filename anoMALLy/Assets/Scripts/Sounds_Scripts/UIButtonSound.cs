using UnityEngine;

public class UIButtonSound : MonoBehaviour
{
    [SerializeField] int buttonSFXIndex = 2;

    public void PlayButtonSound()
    {
        if (AudioManager.Instance == null) return;

        AudioManager.Instance.PlaySFX(buttonSFXIndex);
    }
}