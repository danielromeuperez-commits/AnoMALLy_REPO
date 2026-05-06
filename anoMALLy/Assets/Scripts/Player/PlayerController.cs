using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento & Mirar")]
    [SerializeField] GameObject camHolder;
    [SerializeField] float speed = 5;
    [SerializeField] float sensitivity = 0.1f;

    [Header("Detección")]
    [SerializeField] AnomalyDetector anomalyDetector;

    [Header("Pause Player")]
    [SerializeField] bool controlEnabled = true;

    Rigidbody rb;
    Animator anim;

    Vector2 MoveInput;
    Vector2 lookInput;
    float lookRotation;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        controlEnabled = true;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Debug.DrawRay(camHolder.transform.position, camHolder.transform.forward * 100f, Color.red);
    }

    private void FixedUpdate()
    {
        Movement();
    }

    private void LateUpdate()
    {
        CameraLook();
    }

    void CameraLook()
    {
        if (!controlEnabled) return;

        transform.Rotate(Vector3.up * lookInput.x * sensitivity);

        lookRotation += -lookInput.y * sensitivity;
        lookRotation = Mathf.Clamp(lookRotation, -90, 90);

        camHolder.transform.localEulerAngles = new Vector3(lookRotation, 0f, 0f);
    }

    public void SetControls(bool value)
    {
        controlEnabled = value;

        if (!value)
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    void Movement()
    {
        if (!controlEnabled) return;

        Vector3 direction = new Vector3(MoveInput.x, 0, MoveInput.y);
        direction = transform.TransformDirection(direction);

        rb.linearVelocity = new Vector3(direction.x * speed, rb.linearVelocity.y, direction.z * speed);
    }

    #region INPUT METHODS

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!controlEnabled) return;

        if (anomalyDetector == null) return;

        if (context.started)
        {
            anomalyDetector.SetDetecting(true);
        }

        if (context.canceled)
        {
            anomalyDetector.SetDetecting(false);
        }
    }

    #endregion
}