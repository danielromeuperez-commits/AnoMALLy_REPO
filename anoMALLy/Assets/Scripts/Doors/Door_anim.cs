using UnityEngine;

public class Door_Anim : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] Animator anim;

    [Header("Audio")]
    [SerializeField] bool playDoorOpenSound = true;
    [SerializeField] int doorOpenSFXIndex = 7; // Door_Open

    [SerializeField] bool playDoorCloseSound = false;
    [SerializeField] int doorCloseSFXIndex = 6; // Door_Closing

    bool isOpen;

    private void Awake()
    {
        if (anim == null)
        {
            anim = GetComponent<Animator>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        anim.SetBool("character_nearby", true);

        if (!isOpen)
        {
            isOpen = true;

            if (playDoorOpenSound && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(doorOpenSFXIndex);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        anim.SetBool("character_nearby", false);

        if (isOpen)
        {
            isOpen = false;

            if (playDoorCloseSound && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(doorCloseSFXIndex);
            }
        }
    }
}