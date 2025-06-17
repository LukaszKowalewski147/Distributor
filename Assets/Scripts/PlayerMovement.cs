using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5.5f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float accelerationTime = 0.2f;
    [SerializeField] private float decelerationTime = 0.1f;
    
    [Header("Jump")]
    [SerializeField] private float jumpForce = 3.8f;
    [SerializeField] private float jumpForceSprintMultiplier = 1.4f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float gravityMultiplier = 1.2f;

    [Header("Other")]
    [SerializeField] private float pushBackDistance = 1.5f;

    private CharacterController characterController;
    private InputSystem_Actions inputActions;
    private AnimationManager animationManager;
    private PlayerStats playerStats;
    
    private Vector3 moveDirection;
    private Vector3 lastMoveDirection;
    private Vector2 inputVector;

    private float rotationSpeed;
    private float verticalVelocity;
    private float currentSpeed;
    private bool movementDisabled;
    private bool isSprinting;
    private bool isJumping;
    private bool isJumpRequested;
    
    // [SerializeField] private RabbitMQManager rabbitMQManager; // Uncomment for RabbitMQ

    void Awake()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Player.Move.performed += ctx => inputVector = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => inputVector = Vector2.zero;
        inputActions.Player.Sprint.performed += ctx => isSprinting = true;
        inputActions.Player.Sprint.canceled += ctx => isSprinting = false;
        inputActions.Player.Jump.performed += ctx => isJumpRequested = true;
    }

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        animationManager = GetComponent<AnimationManager>();
        playerStats = GetComponent<PlayerStats>();
        // rabbitMQManager = FindObjectOfType<RabbitMQManager(); // Uncomment for RabbitMQ

        rotationSpeed = 0.0f;
        verticalVelocity = 0.0f;
        currentSpeed = 0.0f;
        movementDisabled = false;
        isSprinting = false;
        isJumping = false;
        isJumpRequested = false;

        lastMoveDirection = Vector3.forward; // Domyœlny kierunek
    }

    void OnEnable()
    {
        inputActions.Player.Enable();
    }

    void OnDisable()
    {
        inputActions.Player.Disable();
    }

    void Update()
    {
        Debug.Log("current speed: " + currentSpeed);
        // Blokuj ruch, poczas interakcji
        if (movementDisabled)
        {
            DontMove();
            return; // Pomiñ resztê logiki ruchu
        }

        Vector3 inputDirection = new Vector3(inputVector.x, 0, inputVector.y).normalized;

        // Obliczanie docelowej prêdkoœci
        float targetSpeed = inputDirection.magnitude > 0 ? moveSpeed * (isSprinting ? sprintMultiplier : 1f) : 0f;

        // P³ynne przyspieszenie lub hamowanie
        float t = Time.deltaTime / (targetSpeed > currentSpeed ? accelerationTime : decelerationTime);
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, t);

        // Ruch poziomy
        if (inputDirection.magnitude > 0.01f || currentSpeed > 0.01f)
        {
            if (inputDirection.magnitude > 0.01f)
            {
                // Aktualizuj kierunek ruchu i obrót podczas inputu
                float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
                float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationSpeed, 0.1f);
                transform.rotation = Quaternion.Euler(0, smoothAngle, 0);
                lastMoveDirection = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward; // Zapisz kierunek
                moveDirection = lastMoveDirection * currentSpeed;
            }
            else
            {
                // Hamowanie wzd³u¿ ostatniego kierunku
                moveDirection = lastMoveDirection * currentSpeed;
            }

            // Send position via RabbitMQ (uncomment for multiplayer)
            // if (rabbitMQManager != null)
            // {
            //     rabbitMQManager.SendMessage("move", playerId, transform.position, inputDirection.magnitude, inputVector.x, verticalVelocity, isSprinting, isJumpRequested);
            // }
        }
        else
        {
            moveDirection = Vector3.zero;
        }

        // Skok
        if (isJumpRequested && characterController.isGrounded && !isJumping)
        {
            verticalVelocity = isSprinting ? jumpForce * jumpForceSprintMultiplier : jumpForce;
            isJumping = true;
            isJumpRequested = false;
        }

        // Grawitacja
        if (!characterController.isGrounded)
        {
            verticalVelocity += gravity * gravityMultiplier * Time.deltaTime;
        }
        else if (verticalVelocity < 0)
        {
            verticalVelocity = -0.5f;
        }
        
        moveDirection.y = verticalVelocity;

        // Ruch postaci
        characterController.Move(moveDirection * Time.deltaTime);

        if (characterController.isGrounded && isJumping)
        {
            isJumping = false;
        }

        // Aktualizacja animacji
        animationManager.SetBaseMovement(inputDirection.magnitude, isSprinting, isJumping);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Sting"))
        {
            Vector3 cactusPosition = hit.gameObject.transform.position;

            TriggerMovementDisableForTime(1.0f);
            MoveAwayFromTarget(cactusPosition);
            RotateToTarget(cactusPosition);
            animationManager.GetHit();
            playerStats.DecreaseHpBy(1);
            Debug.Log("HP: " + playerStats.GetHp());
        }
    }

    private void MoveAwayFromTarget(Vector3 target)
    {
        Vector3 directionAwayFromTarget = (transform.position - target).normalized;
        directionAwayFromTarget.y = 1.0f; // Ignoruj oœ Y

        Vector3 pushBackPosition = transform.position + directionAwayFromTarget * pushBackDistance;
        characterController.Move(pushBackPosition - transform.position);
    }

    public void TriggerMovementDisableForTime(float timeInSeconds)
    {
        StartCoroutine(DisableMovementForTime(timeInSeconds));
    }

    public void DisableMovement()
    {
        movementDisabled = true;
    }

    public void EnableMovement()
    {
        movementDisabled = false;
    }

    public void SetIdleAnimation()
    {
        animationManager.Idle();
    }

    public void RotateToTarget(Vector3 target)
    {
        Vector3 directionToTarget = (target - transform.position).normalized;
        directionToTarget.y = 0; // Ignoruj oœ Y, aby obrót by³ tylko w poziomie
        if (directionToTarget != Vector3.zero)
        {
            StartCoroutine(PerformRotationToTarget(directionToTarget));
        }
    }

    private IEnumerator PerformRotationToTarget(Vector3 directionToTarget)
    {
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        float rotationTime = 0.3f; // Czas obrotu w sekundach
        float elapsed = 0f;

        while (elapsed < rotationTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / rotationTime);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null; // Czekaj na nastêpn¹ klatkê
        }

        transform.rotation = targetRotation;
        Debug.Log($"Obrócono postaæ w stronê celu! Czas: {elapsed:F2} s");
    }

    public IEnumerator DisableMovementForTime(float timeInSeconds)
    {
        DisableMovement();
        yield return new WaitForSeconds(timeInSeconds);
        EnableMovement();
    }

    private void DontMove()
    {
        // Zatrzymaj ruch poziomy
        moveDirection = Vector3.zero; 
        currentSpeed = 0.0f;

        if (characterController.isGrounded)
            verticalVelocity = -0.5f; // Minimalna grawitacja na ziemi
        else
            verticalVelocity += gravity * gravityMultiplier * Time.deltaTime; // Grawitacja w powietrzu

        moveDirection.y = verticalVelocity;
        characterController.Move(moveDirection * Time.deltaTime);
    }

    void OnDestroy()
    {
        inputActions.Dispose();
    }
}