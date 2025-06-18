using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private PlayerInputs playerInputs;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool sprintInput;
    private bool isUIOpen = false;

    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 10f;
    [Range(0f, 1f)] public float movementSmoothing = 0.2f;
    private Vector3 velocityRef;  // SmoothDamp referansı
    public bool CanMove = true;  // Hareket kontrolü için değişken

    [Header("Rotation Settings")]
    private float targetCameraX = 0f;
    public float xSensitivity = 30f;
    public float ySensitivity = 30f;
    public Transform cameraTransform;
    public float cameraLerpSpeed = 20f;

    [Header("Jump Settings")]
    public float jumpForce = 5f;
    private bool isGrounded = true;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        playerInputs = new PlayerInputs();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        Debug.Log("PlayerController: Awake completed, CanMove = " + CanMove);
    }

    private void OnEnable()
    {
        var p = playerInputs.Player;
        p.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        p.Move.canceled  += ctx => moveInput = Vector2.zero;
        p.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        p.Look.canceled  += ctx => lookInput = Vector2.zero;
        p.Sprint.performed += _ => sprintInput = true;
        p.Sprint.canceled  += _ => sprintInput = false;
        // Jump fonksiyonu kaldırıldı - Space tuşu Game2'de teleportasyon için kullanılıyor
        // p.Jump.performed += _ =>
        // {
        //     if (isGrounded && !isUIOpen)
        //     {
        //         rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        //         isGrounded = false;
        //     }
        // };
        p.Enable();
        // UI açıldığında hareketi kilitle
        // UI kapandığında hareketi aç
        EventManager.Subscribe("LockPlayerMovement", () => rb.isKinematic = true);
        EventManager.Subscribe("UnlockPlayerMovement", () => rb.isKinematic = false);
        EventManager.Subscribe("UIOpen", () => EventManager.Trigger("LockPlayerMovement"));
        EventManager.Subscribe("UIClose", () => EventManager.Trigger("UnlockPlayerMovement"));
        
        Debug.Log("PlayerController: OnEnable completed, input system enabled");
    }

    private void OnDisable()
    {
        playerInputs.Player.Disable();
        EventManager.Unsubscribe("LockPlayerMovement", () => rb.isKinematic = true);
        EventManager.Unsubscribe("UnlockPlayerMovement", () => rb.isKinematic = false);
        EventManager.Unsubscribe("UIOpen", () => EventManager.Trigger("LockPlayerMovement"));
        EventManager.Unsubscribe("UIClose", () => EventManager.Trigger("UnlockPlayerMovement"));
    }

    private void Update()
    {
        // Debug: T tuşu ile PlayerController durumunu kontrol et
        if (Input.GetKeyDown(KeyCode.T))
        {
            DebugPlayerState();
        }
        
        // Sadece hareket edebiliyorsak kamera kontrolü yap
        if (!CanMove || isUIOpen) return;

        // Yatay dönüş
        transform.Rotate(Vector3.up * lookInput.x * Time.deltaTime * ySensitivity);
        // Dikey kamera açısı
        targetCameraX -= lookInput.y * Time.deltaTime * xSensitivity;
        targetCameraX = Mathf.Clamp(targetCameraX, -90f, 90f);
    }

    private void LateUpdate()
    {
        if (cameraTransform == null || !CanMove || isUIOpen) return;
        
        Quaternion q = Quaternion.Euler(targetCameraX, 0f, 0f);
        cameraTransform.localRotation = Quaternion.Slerp(
            cameraTransform.localRotation, q, cameraLerpSpeed * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        // Hareket edemiyor veya UI açıksa hiçbir şey yapma
        if (!CanMove || isUIOpen) return;

        float speed = sprintInput ? sprintSpeed : walkSpeed;
        if (sprintInput)
            PlayerStat.Instance.UseStamina(10);

        Vector3 desiredDir = (transform.forward * moveInput.y + transform.right * moveInput.x).normalized * speed;

        // Mevcut yatay hız (y ekseni 0)
        Vector3 currentVelXZ = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        // Smooth geçiş: tuş bırakıldığında desiredDir=0 olacağı için yavaşça duracak
        Vector3 smoothVel = Vector3.SmoothDamp(
            currentVelXZ,
            desiredDir,
            ref velocityRef,
            movementSmoothing
        );

        // Uygula, dikey hızı koru
        rb.linearVelocity = new Vector3(smoothVel.x, rb.linearVelocity.y, smoothVel.z);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    /// <summary>
    /// Debug fonksiyonu - PlayerController durumunu kontrol etmek için
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DebugPlayerState()
    {
        Debug.Log($"PlayerController Debug:" +
                  $"\n- CanMove: {CanMove}" +
                  $"\n- isUIOpen: {isUIOpen}" +
                  $"\n- moveInput: {moveInput}" +
                  $"\n- lookInput: {lookInput}" +
                  $"\n- rb.isKinematic: {rb.isKinematic}" +
                  $"\n- rb.linearVelocity: {rb.linearVelocity}" +
                  $"\n- sprintInput: {sprintInput}");
    }
}