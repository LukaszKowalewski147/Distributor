using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;
    public float distance = 5f;
    public float height = 2f;
    public float mouseSensitivity = 100f;
    public float minPitch = -30f;
    public float maxPitch = 60f;

    private float currentYaw;
    private float currentPitch;
    private InputSystem_Actions inputActions;
    private Vector2 lookInput;

    void Awake()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => lookInput = Vector2.zero;
    }

    void Start()
    {
        currentYaw = transform.eulerAngles.y;
        currentPitch = transform.eulerAngles.x;
    }

    void OnEnable()
    {
        inputActions.Player.Enable();
    }

    void OnDisable()
    {
        inputActions.Player.Disable();
    }

    void LateUpdate()
    {
        // Update camera rotation based on mouse/gamepad input
        currentYaw += lookInput.x * mouseSensitivity * Time.deltaTime;
        currentPitch -= lookInput.y * mouseSensitivity * Time.deltaTime;
        currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);

        // Calculate camera position
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);
        Vector3 position = target.position - (rotation * Vector3.forward * distance);
        position.y += height;

        transform.position = position;
        transform.rotation = rotation;
    }

    void OnDestroy()
    {
        inputActions.Dispose();
    }
}