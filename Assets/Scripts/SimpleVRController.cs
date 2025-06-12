using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// Hata yapmayan basit VR Controller - XR Hands paketi gerektirmez
/// </summary>
public class SimpleVRController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float sprintSpeed = 6f;
    [SerializeField] private float turnSpeed = 90f;
    
    [Header("VR Settings")]
    [SerializeField] private Transform vrCamera;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private bool enableSnapTurn = true;
    [SerializeField] private float snapTurnAngle = 30f;
    [SerializeField] private float snapTurnCooldown = 0.3f;
    
    // Input variables
    private InputDevice leftController;
    private InputDevice rightController;
    private Vector2 leftJoystick;
    private Vector2 rightJoystick;
    private bool isSprinting;
    private float lastSnapTurnTime;
    private bool controllersInitialized;
    
    // Movement variables  
    private Vector3 velocity;
    
    private void Start()
    {
        InitializeVR();
        TryInitializeControllers();
    }
    
    private void InitializeVR()
    {
        // VR Camera'yı bul
        if (vrCamera == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                // XR Origin'den camera bul
                var xrOrigin = FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
                if (xrOrigin != null)
                    vrCamera = xrOrigin.Camera.transform;
            }
            else
                vrCamera = mainCam.transform;
        }
        
        // CharacterController'ı ayarla
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
            if (characterController == null)
            {
                characterController = gameObject.AddComponent<CharacterController>();
                SetupCharacterController();
            }
        }
    }
    
    private void SetupCharacterController()
    {
        characterController.center = new Vector3(0, 1, 0);
        characterController.height = 1.8f;
        characterController.radius = 0.3f;
        characterController.stepOffset = 0.3f;
        characterController.slopeLimit = 45f;
    }
    
    private void TryInitializeControllers()
    {
        leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        
        controllersInitialized = leftController.isValid && rightController.isValid;
        
        if (controllersInitialized)
        {
            Debug.Log("VR Controllers başarıyla başlatıldı!");
        }
        else
        {
            Debug.LogWarning("VR Controllers bulunamadı. VR aktif mi kontrol edin.");
        }
    }
    
    private void Update()
    {
        if (!controllersInitialized)
        {
            TryInitializeControllers();
            return;
        }
        
        ReadVRInput();
        HandleMovement();
        HandleRotation();
    }
    
    private void ReadVRInput()
    {
        // Joystick input'larını oku
        if (leftController.isValid)
            leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out leftJoystick);
            
        if (rightController.isValid)
            rightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out rightJoystick);
        
        // Sprint kontrolü (Grip butonları)
        bool leftGripPressed = false;
        bool rightGripPressed = false;
        
        if (leftController.isValid)
        {
            float leftGrip;
            leftController.TryGetFeatureValue(CommonUsages.grip, out leftGrip);
            leftGripPressed = leftGrip > 0.7f;
        }
        
        if (rightController.isValid)
        {
            float rightGrip;
            rightController.TryGetFeatureValue(CommonUsages.grip, out rightGrip);
            rightGripPressed = rightGrip > 0.7f;
        }
        
        isSprinting = leftGripPressed || rightGripPressed;
    }
    
    private void HandleMovement()
    {
        if (leftJoystick.magnitude < 0.1f) return;
        
        // VR Camera yönünü kullan
        Vector3 forward = vrCamera.forward;
        Vector3 right = vrCamera.right;
        
        // Y bileşenini sıfırla
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();
        
        // Hareket yönünü hesapla
        Vector3 moveDirection = forward * leftJoystick.y + right * leftJoystick.x;
        
        // Hızı uygula
        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;
        Vector3 movement = moveDirection * currentSpeed * Time.deltaTime;
        
        // Yerçekimi uygula
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Yerde tutmak için küçük kuvvet
        }
        
        velocity.y += Physics.gravity.y * Time.deltaTime;
        movement.y = velocity.y * Time.deltaTime;
        
        // Hareketi uygula
        characterController.Move(movement);
    }
    
    private void HandleRotation()
    {
        if (!enableSnapTurn || Mathf.Abs(rightJoystick.x) < 0.7f) return;
        
        // Snap turn cooldown kontrolü
        if (Time.time - lastSnapTurnTime < snapTurnCooldown) return;
        
        // Snap turn uygula
        float turnDirection = Mathf.Sign(rightJoystick.x);
        transform.Rotate(0, turnDirection * snapTurnAngle, 0);
        lastSnapTurnTime = Time.time;
        
        // Haptic feedback gönder
        SendHapticFeedback(rightController, 0.3f, 0.1f);
    }
    
    // Utility metodlar
    private void SendHapticFeedback(InputDevice device, float amplitude, float duration)
    {
        if (!device.isValid) return;
        
        HapticCapabilities capabilities;
        if (device.TryGetHapticCapabilities(out capabilities) && capabilities.supportsImpulse)
        {
            device.SendHapticImpulse(0, amplitude, duration);
        }
    }
    
    // Public metodlar
    public Vector2 GetLeftJoystick() => leftJoystick;
    public Vector2 GetRightJoystick() => rightJoystick;
    public bool IsMoving() => leftJoystick.magnitude > 0.1f;
    public bool IsSprinting() => isSprinting;
    public bool IsInitialized() => controllersInitialized;
    
    public void SetMoveSpeed(float speed) => moveSpeed = speed;
    public void SetSprintSpeed(float speed) => sprintSpeed = speed;
    public void SetTurnSpeed(float speed) => turnSpeed = speed;
    
    // Button kontrolleri
    public bool IsButtonPressed(XRNode hand, string buttonType)
    {
        InputDevice device = (hand == XRNode.LeftHand) ? leftController : rightController;
        if (!device.isValid) return false;
        
        switch (buttonType.ToLower())
        {
            case "trigger":
                float trigger;
                device.TryGetFeatureValue(CommonUsages.trigger, out trigger);
                return trigger > 0.7f;
                
            case "grip":
                float grip;
                device.TryGetFeatureValue(CommonUsages.grip, out grip);
                return grip > 0.7f;
                
            case "primary": // A/X button
                bool primary;
                device.TryGetFeatureValue(CommonUsages.primaryButton, out primary);
                return primary;
                
            case "secondary": // B/Y button
                bool secondary;
                device.TryGetFeatureValue(CommonUsages.secondaryButton, out secondary);
                return secondary;
                
            case "menu":
                bool menu;
                device.TryGetFeatureValue(CommonUsages.menuButton, out menu);
                return menu;
                
            default:
                return false;
        }
    }
    
    // Debug GUI
    private void OnGUI()
    {
        if (!Application.isEditor) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("=== Simple VR Controller ===");
        GUILayout.Label($"Initialized: {controllersInitialized}");
        GUILayout.Label($"Left Joystick: {leftJoystick}");
        GUILayout.Label($"Right Joystick: {rightJoystick}");
        GUILayout.Label($"Is Moving: {IsMoving()}");
        GUILayout.Label($"Is Sprinting: {IsSprinting()}");
        GUILayout.Label($"Grounded: {characterController?.isGrounded}");
        GUILayout.EndArea();
    }
}
