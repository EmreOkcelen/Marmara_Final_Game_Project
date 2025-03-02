using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerInputs playerInputs;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool sprintInput;
    
    [Header("Movement Settings")]
    public float movementSpeed = 5f;
    public float sprintSpeed = 10f;
    [Range(0f, 1f)]
    public float movementSmoothing = 0.2f; // Hız geçişlerinin yumuşaklığı
    private Vector3 currentVelocity = Vector3.zero;

    [Header("Rotation Settings")]
    private float targetCameraX = 0f; // Hedef dikey açı (pitch)
    public float xSensitivity = 30f;
    public float ySensitivity = 30f;
    public Transform cameraTransform;
    public float cameraLerpSpeed = 20f;

    [Header("Jump Settings")]
    public float jumpForce = 5f;
    private bool isGrounded = true;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerInputs = new PlayerInputs();
    }

    private void OnEnable()
    {
        playerInputs.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>().normalized;
        playerInputs.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        playerInputs.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        playerInputs.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        playerInputs.Player.Sprint.performed += ctx => sprintInput = true;
        playerInputs.Player.Sprint.canceled += ctx => sprintInput = false;

        playerInputs.Player.Jump.performed += ctx =>
        {
            if (isGrounded)
            {
                // Zıplama, transform üzerinden y ekseninde pozisyon eklemesiyle yapılıyor
                transform.position += Vector3.up * jumpForce;
                isGrounded = false;
            }
        };

        playerInputs.Player.Enable();
    }

    private void OnDisable()
    {
        playerInputs.Player.Disable();
    }

    private void Update()
    {
        // Dönüş: Yatay dönüş doğrudan uygulanıyor, dikey dönüş (kamera pitch) hedef değere aktarılıyor
        if (lookInput.sqrMagnitude > 0.001f)
        {
            transform.Rotate(Vector3.up, lookInput.x * Time.deltaTime * ySensitivity);
            targetCameraX -= lookInput.y * Time.deltaTime * xSensitivity;
            targetCameraX = Mathf.Clamp(targetCameraX, -60f, 60f);
        }

        // Kamera dönüşü: Slerp ile yumuşak geçiş uygulanıyor
        if (cameraTransform != null)
        {
            Quaternion targetRotation = Quaternion.Euler(targetCameraX, 0f, 0f);
            cameraTransform.localRotation = Quaternion.Slerp(cameraTransform.localRotation, targetRotation, cameraLerpSpeed * Time.deltaTime);
        }

        // Hareket: Input değerlerine göre istenen hareket vektörü hesaplanıyor
        float speed = sprintInput ? sprintSpeed : movementSpeed;
        Vector3 desiredMove = (transform.forward * moveInput.y + transform.right * moveInput.x) * speed;
        // Yumuşak hız geçişi
        currentVelocity = Vector3.Lerp(currentVelocity, desiredMove, movementSmoothing);
        // Dünya uzayında konum güncellemesi
        transform.Translate(currentVelocity * Time.deltaTime, Space.World);
    }

    // Basit yer temas kontrolü: Eğer nesne "Ground" tag'ine sahip bir obje ile çarpışırsa isGrounded true olur
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }
}


/*using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerInputs playerInputs;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool sprintInput;

    [Header("Movement Settings")]
    public float movementSpeed = 5f;
    public float sprintSpeed = 10f;
    public float movementSmoothing = 0.2f;

    [Header("Rotation Settings")]
    private float targetCameraX = 0f; // Hedef dikey açı (pitch)
    public float xSensitivity = 30f;
    public float ySensitivity = 30f;
    public Transform cameraTransform;
    // Kamera dönüşünü ne kadar hızlı slerp ile uygulayacağınızı ayarlayın (örneğin 15-30 civarı)
    public float cameraLerpSpeed = 20f;

    [Header("Jump Settings")]
    public float jumpForce = 5f;
    private Rigidbody rb;



    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        playerInputs = new PlayerInputs();
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void OnEnable()
    {
        playerInputs.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>().normalized;
        playerInputs.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        playerInputs.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        playerInputs.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        playerInputs.Player.Sprint.performed += ctx => sprintInput = true;
        playerInputs.Player.Sprint.canceled += ctx => sprintInput = false;

        playerInputs.Player.Jump.performed += ctx =>
        {
            if (Mathf.Abs(rb.linearVelocity.y) < 0.1f)
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        };

        playerInputs.Player.Enable();
    }

    private void OnDisable()
    {
        playerInputs.Player.Disable();
    }

    // Update: Oyuncunun yatay dönüşü ve hedef dikey açı hesaplanıyor
    private void Update()
    {
        if (lookInput.sqrMagnitude > 0.001f)
        {
            // Yatay dönüş anında uygulansın
            transform.Rotate(Vector3.up, lookInput.x * Time.deltaTime * ySensitivity);
            // Hedef kamera açısını güncelle (pitch)
            targetCameraX -= lookInput.y * Time.deltaTime * xSensitivity;
            targetCameraX = Mathf.Clamp(targetCameraX, -60f, 60f);
        }
    }

    // LateUpdate: Kamera dönüşünü Quaternion.Slerp ile yumuşakça uygula
    private void LateUpdate()
    {
        if (cameraTransform == null)
            return;

        Quaternion targetRotation = Quaternion.Euler(targetCameraX, 0f, 0f);
        // Slerp ile mevcut kamera dönüşü, hedef dönüşe doğru yumuşakça çekiliyor
        cameraTransform.localRotation = Quaternion.Slerp(
            cameraTransform.localRotation,
            targetRotation,
            cameraLerpSpeed * Time.deltaTime);
    }

    // FixedUpdate: Fizik tabanlı hareket işlemleri
    private void FixedUpdate()
    {
        float speed = sprintInput ? sprintSpeed : movementSpeed;
        Vector3 targetVelocity = Vector3.zero;

        if (moveInput.sqrMagnitude > 0.001f)
        {
            Vector3 moveDir = transform.TransformDirection(new Vector3(moveInput.x, 0f, moveInput.y));
            targetVelocity = moveDir * speed;
        }

        Vector3 currentHorVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        Vector3 newHorVel = Vector3.Lerp(currentHorVel, targetVelocity, movementSmoothing);
        rb.linearVelocity = new Vector3(newHorVel.x, rb.linearVelocity.y, newHorVel.z);

        if (sprintInput)
            PlayerStat.Instance.UseStamina(0.1f * Time.fixedDeltaTime);
    }
}*/