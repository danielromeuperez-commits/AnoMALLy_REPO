using UnityEngine;

public class EyesManagerTrigger : MonoBehaviour
{
    public GameObject eyes;
    public GameObject trigger;

    private void OnTriggerEnter(Collider other)
    {
        eyes.SetActive(true);
        trigger.SetActive(false);
        AudioManager.Instance.PlaySFX(2);
    }
}
