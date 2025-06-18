using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(Rigidbody))]
public class UnifiedVRPlayerController : MonoBehaviour
{
    // --- VR Kontrolcüleri ---
    private InputDevice leftController;
    private InputDevice rightController;

    // --- Hareket & Sprint ---
    private Vector2 moveInput;
    private bool sprintInput;

    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 10f;
    [Range(0f, 1f)] public float movementSmoothing = 0.2f;
    private Vector3 velocityRef;
    public bool CanMove = true;

    [Header("Turn Settings")]
    [Tooltip("Sağ stick ile yatayda hızlı dönüş hızı (derece/sn)")]
    public float fastTurnSpeed = 180f;

    [Header("Look Settings")]
    [Tooltip("Sağ stick ile dikey bakış hassasiyeti (derece/sn)")]
    public float xSensitivity = 30f;
    private float targetCameraX = 0f;

    [Header("Camera Settings")]
    public Transform cameraTransform;
    public float cameraLerpSpeed = 20f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        InputDevices.deviceConnected += OnDeviceConnected;
        InitializeOpenXRControllers();
    }

    private void OnDestroy()
    {
        InputDevices.deviceConnected -= OnDeviceConnected;
    }

    private void OnDeviceConnected(InputDevice device)
    {
        if ((device.characteristics & InputDeviceCharacteristics.Left) != 0 &&
            (device.characteristics & InputDeviceCharacteristics.Controller) != 0)
        {
            leftController = device;
            Debug.Log($"[Connected] Sol kontrolcü: {device.name}");
        }
        if ((device.characteristics & InputDeviceCharacteristics.Right) != 0 &&
            (device.characteristics & InputDeviceCharacteristics.Controller) != 0)
        {
            rightController = device;
            Debug.Log($"[Connected] Sağ kontrolcü: {device.name}");
        }
    }

    private void InitializeOpenXRControllers()
    {
        var allDevices = new List<InputDevice>();
        InputDevices.GetDevices(allDevices);

        foreach (var d in allDevices)
        {
            if ((d.characteristics & InputDeviceCharacteristics.Left) != 0 &&
                (d.characteristics & InputDeviceCharacteristics.Controller) != 0)
            {
                leftController = d;
                Debug.Log($"🤚 Sol kontrolcü: {d.name}");
            }
            if ((d.characteristics & InputDeviceCharacteristics.Right) != 0 &&
                (d.characteristics & InputDeviceCharacteristics.Controller) != 0)
            {
                rightController = d;
                Debug.Log($"🤚 Sağ kontrolcü: {d.name}");
            }
        }
    }

    private void Update()
    {
        // Sağ stick ile yatayda hızlı dönüş, dikeyde bakış
        if (rightController.isValid &&
            rightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 rightStick))
        {
            // Hızlı yaw
            float yaw = rightStick.x * fastTurnSpeed * Time.deltaTime;
            transform.Rotate(0f, yaw, 0f);

            // Dikey bakış
            targetCameraX = Mathf.Clamp(
                targetCameraX - rightStick.y * xSensitivity * Time.deltaTime,
                -90f, 90f
            );
        }
    }

    private void LateUpdate()
    {
        if (cameraTransform == null) return;
        var targetRot = Quaternion.Euler(targetCameraX, 0f, 0f);
        cameraTransform.localRotation = Quaternion.Slerp(
            cameraTransform.localRotation,
            targetRot,
            cameraLerpSpeed * Time.deltaTime
        );
    }

    private void FixedUpdate()
    {
        // Sol stick ile hareket, primaryButton ile sprint
        if (leftController.isValid)
        {
            leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out moveInput);
            leftController.TryGetFeatureValue(CommonUsages.primaryButton,    out sprintInput);
        }

        float speed = sprintInput ? sprintSpeed : walkSpeed;
        if (sprintInput)
            PlayerStat.Instance.UseStamina(10);

        // Kamera yatay yön vektörleri
        Vector3 camForward = cameraTransform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = cameraTransform.right;
        camRight.y = 0f;
        camRight.Normalize();

        // İleri/yan hareket
        Vector3 desiredDir = (camForward * moveInput.y + camRight * moveInput.x).normalized * speed;
        Vector3 currentXZ  = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        Vector3 smoothVel = Vector3.SmoothDamp(
            currentXZ,
            desiredDir,
            ref velocityRef,
            movementSmoothing
        );

        if (CanMove)
            rb.linearVelocity = new Vector3(smoothVel.x, rb.linearVelocity.y, smoothVel.z);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            // Zıplama kodu varsa buraya eklersin
        }
    }
}
