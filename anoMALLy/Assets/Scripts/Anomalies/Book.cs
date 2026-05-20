using UnityEngine;

public class Book : MonoBehaviour
{
    [SerializeField] private float targetX = 2f;
    [SerializeField] private float targetZ = 2f;
    [SerializeField] private float speed = 2f;

    private void Update()
    {
        Vector3 scale = transform.localScale;

        scale.x = Mathf.Lerp(scale.x, targetX, speed * Time.deltaTime);
        scale.z = Mathf.Lerp(scale.z, targetZ, speed * Time.deltaTime);

        transform.localScale = scale;
    }
}