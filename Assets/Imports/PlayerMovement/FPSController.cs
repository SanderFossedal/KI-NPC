using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class FPSController : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float walkingSpeed = 3.0f;
    [SerializeField] private float gravity = 8.0f;

    [Header("Look Parameters")]
    [SerializeField, Range(1, 10)] private float lookSpeedX = 2.0f;
    [SerializeField, Range(1, 10)] private float lookSpeedY = 2.0f;
    [SerializeField, Range(1, 180)] private float upperLookLimit = 80.0f;
    [SerializeField, Range(1, 180)] private float lowerLookLimit = 80.0f;

    private Camera playerCamera;
    private CharacterController characterController;

    private Vector3 forward;
    private Vector3 right;
    private Vector3 moveDirection;
    private Vector2 currentInput;

    private float rotationX = 0;

    void Awake()
    {
        CacheComponents();
        LockCursor(CursorLockMode.Locked);
    }


    void Update()
    {
        HandleMovementInput();
        HandleMouseLook();

        HandleResetCheck();

        ApplyFinalMovements();
    }

    private void CacheComponents()
    {
        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();
    }

    private void HandleMovementInput()
    {
        forward = transform.TransformDirection(Vector3.forward);
        right = transform.TransformDirection(Vector3.right);

        // Calculate movement direction from input and speed
        currentInput = new Vector2(walkingSpeed * Input.GetAxis("Vertical"), walkingSpeed * Input.GetAxis("Horizontal"));

        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * currentInput.x) + (right * currentInput.y);
        moveDirection.y = movementDirectionY;
    }

    private void HandleMouseLook()
    {
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeedY;
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeedX, 0);
    }

    private void ApplyFinalMovements()
    {
        // Apply gravity
        if (!characterController.isGrounded)
            moveDirection.y -= gravity * Time.deltaTime;

        // Apply final movements
        characterController.Move(moveDirection * Time.deltaTime);
    }

    public void LockCursor(CursorLockMode lockMode)
    {
        Cursor.lockState = lockMode;
        Cursor.visible = lockMode == CursorLockMode.Locked ? false : true;
    }

    public void HandleResetCheck()
    {
        if (Input.GetKeyDown(KeyCode.R))
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}