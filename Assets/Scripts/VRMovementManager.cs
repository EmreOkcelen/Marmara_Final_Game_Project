using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// Basit VR Hareket Yöneticisi - Joystick kontrolü için
/// </summary>
public class VRMovementManager : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float turnSpeed = 90f;
    
    [Header("VR Settings")]
    [SerializeField] private Transform vrCamera;
    [SerializeField] private CharacterController characterController;
    
    // Input variables
    private InputDevice leftController;
    private InputDevice rightController;
    private Vector2 leftJoystick;
    private Vector2 rightJoystick;
    private bool isRunning;
    
    // Movement variables
    private Vector3 velocity;
    private bool isInitialized;
    
    private void Start()
    {
        SetupVR();
        InitializeControllers();
    }
    
    private void SetupVR()
    {
        // VR Camera'yı bul
        if (vrCamera == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
                vrCamera = mainCam.transform;
        }
        
        // CharacterController'ı al veya ekle
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
            if (characterController == null)
            {
                characterController = gameObject.AddComponent<CharacterController>();
                characterController.center = new Vector3(0, 1, 0);
                characterController.height = 1.8f;
                characterController.radius = 0.3f;
            }
        }
    }
    
    private void InitializeControllers()
    {
        leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        
        if (leftController.isValid && rightController.isValid)
        {
            isInitialized = true;
            Debug.Log("VR Controllers başarıyla başlatıldı!");
        }
    }
    
    private void Update()
    {
        if (!isInitialized)
        {
            InitializeControllers();
            return;
        }
        
        ReadInput();
        HandleMovement();
        HandleRotation();
    }
    
    /// <summary>
    /// VR Controller input'larını oku
    /// </summary>
    private void ReadInput()
    {
        // Joystick değerlerini oku
        leftController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out leftJoystick);
        rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out rightJoystick);
        
        // Koşma kontrolü (grip tuşları)
        float leftGrip, rightGrip;
        leftController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.grip, out leftGrip);
        rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.grip, out rightGrip);
        isRunning = leftGrip > 0.7f || rightGrip > 0.7f;
    }
    
    /// <summary>
    /// Hareket kontrolü
    /// </summary>
    private void HandleMovement()
    {
        if (leftJoystick.magnitude < 0.1f) return;
        
        // VR Camera'nın yönünü al
        Vector3 cameraForward = vrCamera.forward;
        Vector3 cameraRight = vrCamera.right;
        
        // Y bileşenini sıfırla (yerde kalması için)
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();
        
        // Hareket yönünü hesapla
        Vector3 moveDirection = (cameraForward * leftJoystick.y) + (cameraRight * leftJoystick.x);
        
        // Hızı uygula
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        Vector3 movement = moveDirection * currentSpeed * Time.deltaTime;
        
        // Yerçekimi uygula
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += Physics.gravity.y * Time.deltaTime;
        movement.y = velocity.y * Time.deltaTime;
        
        // Hareketi uygula
        characterController.Move(movement);
        
        // Debug bilgisi
        Debug.Log($"Hareket: {leftJoystick} | Hız: {currentSpeed} | Koşuyor: {isRunning}");
    }
    
    /// <summary>
    /// Dönüş kontrolü
    /// </summary>
    private void HandleRotation()
    {
        if (Mathf.Abs(rightJoystick.x) < 0.1f) return;
        
        // Smooth turn
        float turnAmount = rightJoystick.x * turnSpeed * Time.deltaTime;
        transform.Rotate(0, turnAmount, 0);
        
        Debug.Log($"Dönüş: {rightJoystick.x} | Açı: {turnAmount}");
    }
    
    /// <summary>
    /// Public metodlar - dışarıdan erişim için
    /// </summary>
    
    // Joystick değerlerini al
    public Vector2 GetLeftJoystick() => leftJoystick;
    public Vector2 GetRightJoystick() => rightJoystick;
    
    // Hareket durumu
    public bool IsMoving() => leftJoystick.magnitude > 0.1f;
    public bool IsRunning() => isRunning;
    
    // VR Controller erişimi
    public InputDevice GetLeftController() => leftController;
    public InputDevice GetRightController() => rightController;
    
    // Hızları ayarla
    public void SetWalkSpeed(float speed) => walkSpeed = speed;
    public void SetRunSpeed(float speed) => runSpeed = speed;
    public void SetTurnSpeed(float speed) => turnSpeed = speed;
    
    /// <summary>
    /// VR Controller butonlarını kontrol et
    /// </summary>
    public bool IsButtonPressed(XRNode hand, string buttonType)
    {
        InputDevice device = (hand == XRNode.LeftHand) ? leftController : rightController;
        
        switch (buttonType.ToLower())
        {
            case "trigger":
                float trigger;
                device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out trigger);
                return trigger > 0.7f;
                
            case "grip":
                float grip;
                device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.grip, out grip);
                return grip > 0.7f;
                
            case "primary":
                bool primary;
                device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out primary);
                return primary;
                
            case "secondary":
                bool secondary;
                device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out secondary);
                return secondary;
                
            case "menu":
                bool menu;
                device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.menuButton, out menu);
                return menu;
                
            default:
                return false;
        }
    }
    
    /// <summary>
    /// Haptic feedback gönder
    /// </summary>
    public void Vibrate(XRNode hand, float intensity = 0.5f, float duration = 0.1f)
    {
        InputDevice device = (hand == XRNode.LeftHand) ? leftController : rightController;
        
        if (device.isValid)
        {
            HapticCapabilities capabilities;
            if (device.TryGetHapticCapabilities(out capabilities) && capabilities.supportsImpulse)
            {
                device.SendHapticImpulse(0, intensity, duration);
            }
        }
    }
    
    /// <summary>
    /// Debug GUI
    /// </summary>
    private void OnGUI()
    {
        if (!Application.isEditor) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("=== VR Movement Manager ===");
        GUILayout.Label($"Initialized: {isInitialized}");
        GUILayout.Label($"Left Joystick: {leftJoystick}");
        GUILayout.Label($"Right Joystick: {rightJoystick}");
        GUILayout.Label($"Is Moving: {IsMoving()}");
        GUILayout.Label($"Is Running: {IsRunning()}");
        GUILayout.Label($"Grounded: {characterController?.isGrounded}");
        GUILayout.EndArea();
    }
}
