using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento & Mirar")]
    [SerializeField] GameObject camHolder; //ref al obj q tiene como hijo la cámara (rota por la cámara)
    [SerializeField] float speed = 5;
    [SerializeField] float sensitivity = 0.1f; //Sensibilidad para input

    //Variables de ref privadas:
    Rigidbody rb; //ref al rb del PL
    Animator anim; //ref añ animator del PL

    //Variables para input:
    Vector2 MoveInput;
    Vector2 lookInput;
    float lookRotation;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //lock cursor ratón
        Cursor.lockState = CursorLockMode.Locked; //Mueve curson a centro
        Cursor.visible = false; //Oculta cursor
    }

    // Update is called once per frame
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
        //rotacion horizontal del cuerpo del PJ
        transform.Rotate(Vector3.up * lookInput.x * sensitivity);
        //Rotacion vertical (la camara la lleva)
        lookRotation += (-lookInput.y * sensitivity);
        lookRotation = Mathf.Clamp(lookRotation, -90, 90);
        camHolder.transform.localEulerAngles = new Vector3(lookRotation, 0f, 0f);
    }
    void Movement()
    {
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
    #endregion
}
