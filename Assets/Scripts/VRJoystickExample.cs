using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// VR Joystick erişimi ve kullanımı için örnek script
/// </summary>
public class VRJoystickExample : MonoBehaviour
{
    [Header("VR Controller References")]
    public VRController vrController; // VRController script referansı
    
    // Direct XR API kullanımı için
    private InputDevice leftController;
    private InputDevice rightController;
    
    private void Start()
    {
        // VRController script'ini otomatik bul
        if (vrController == null)
            vrController = FindObjectOfType<VRController>();
            
        // Direct VR controller erişimi
        InitializeControllers();
    }
    
    private void InitializeControllers()
    {
        leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }
    
    private void Update()
    {
        // Method 1: VRController script üzerinden erişim
        if (vrController != null)
        {
            AccessViaVRController();
        }
        
        // Method 2: Direct XR API erişimi
        AccessViaDirectXR();
    }
    
    /// <summary>
    /// VRController script üzerinden joystick değerlerine erişim
    /// </summary>
    private void AccessViaVRController()
    {
        // Sol joystick değerleri
        Vector2 leftJoystick = vrController.LeftJoystickInput;
        Vector2 rightJoystick = vrController.RightJoystickInput;
        
        // Hareket durumu kontrolleri
        bool isMoving = vrController.IsMoving;
        bool isSprinting = vrController.IsSprinting;
        Vector3 currentVelocity = vrController.CurrentVelocity;
        
        // Joystick değerlerini kullanma
        if (leftJoystick.magnitude > 0.1f)
        {
            Debug.Log($"Sol joystick hareket ediyor: {leftJoystick}");
            
            // İleri/geri hareket
            if (leftJoystick.y > 0.5f)
                Debug.Log("İleri hareket");
            else if (leftJoystick.y < -0.5f)
                Debug.Log("Geri hareket");
                
            // Sağa/sola hareket
            if (leftJoystick.x > 0.5f)
                Debug.Log("Sağa hareket");
            else if (leftJoystick.x < -0.5f)
                Debug.Log("Sola hareket");
        }
        
        // Sağ joystick ile dönüş kontrolü
        if (Mathf.Abs(rightJoystick.x) > 0.7f)
        {
            if (rightJoystick.x > 0)
                Debug.Log("Sağa dönüyor");
            else
                Debug.Log("Sola dönüyor");
        }
    }
    
    /// <summary>
    /// Direct XR API ile joystick erişimi
    /// </summary>
    private void AccessViaDirectXR()
    {
        if (!leftController.isValid || !rightController.isValid)
        {
            InitializeControllers();
            return;
        }
        
        // Joystick değerlerini oku
        Vector2 leftThumbstick, rightThumbstick;
        leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out leftThumbstick);
        rightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out rightThumbstick);
        
        // Trigger değerlerini oku
        float leftTrigger, rightTrigger;
        leftController.TryGetFeatureValue(CommonUsages.trigger, out leftTrigger);
        rightController.TryGetFeatureValue(CommonUsages.trigger, out rightTrigger);
        
        // Grip değerlerini oku
        float leftGrip, rightGrip;
        leftController.TryGetFeatureValue(CommonUsages.grip, out leftGrip);
        rightController.TryGetFeatureValue(CommonUsages.grip, out rightGrip);
        
        // Button değerlerini oku
        bool leftPrimaryButton, rightPrimaryButton;
        leftController.TryGetFeatureValue(CommonUsages.primaryButton, out leftPrimaryButton);
        rightController.TryGetFeatureValue(CommonUsages.primaryButton, out rightPrimaryButton);
        
        // Secondary button (B/Y buttons)
        bool leftSecondaryButton, rightSecondaryButton;
        leftController.TryGetFeatureValue(CommonUsages.secondaryButton, out leftSecondaryButton);
        rightController.TryGetFeatureValue(CommonUsages.secondaryButton, out rightSecondaryButton);
        
        // Menu button
        bool menuButton;
        leftController.TryGetFeatureValue(CommonUsages.menuButton, out menuButton);
        
        // Değerleri kullan
        HandleCustomInput(leftThumbstick, rightThumbstick, leftTrigger, rightTrigger, 
                         leftGrip, rightGrip, leftPrimaryButton, rightPrimaryButton);
    }
    
    /// <summary>
    /// Özel input handling
    /// </summary>
    private void HandleCustomInput(Vector2 leftStick, Vector2 rightStick, 
                                  float leftTrigger, float rightTrigger,
                                  float leftGrip, float rightGrip,
                                  bool leftButton, bool rightButton)
    {
        // Özel hareket sistemi
        if (leftStick.magnitude > 0.1f)
        {
            // Kendi hareket kodun buraya
            CustomMovement(leftStick);
        }
        
        // Teleportation sistemi
        if (leftTrigger > 0.7f)
        {
            // Teleportation başlat
            StartTeleportation();
        }
        
        // Object grabbing
        if (rightGrip > 0.7f)
        {
            // Obje yakalama
            GrabObject();
        }
        
        // UI interaction
        if (rightButton)
        {
            // UI menüsünü aç/kapat
            ToggleUI();
        }
    }
    
    /// <summary>
    /// Özel hareket sistemi
    /// </summary>
    private void CustomMovement(Vector2 input)
    {
        // Kamera yönüne göre hareket
        Transform cameraTransform = Camera.main.transform;
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();
        
        Vector3 moveDirection = forward * input.y + right * input.x;
        
        // CharacterController ile hareket
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.Move(moveDirection * 3f * Time.deltaTime);
        }
        
        Debug.Log($"Özel hareket: {moveDirection}");
    }
    
    private void StartTeleportation()
    {
        Debug.Log("Teleportation başlatıldı!");
        // Teleportation logic buraya
    }
    
    private void GrabObject()
    {
        Debug.Log("Obje yakalanıyor!");
        // Object grabbing logic buraya
    }
    
    private void ToggleUI()
    {
        Debug.Log("UI toggle!");
        // UI toggle logic buraya
    }
    
    /// <summary>
    /// Haptic feedback gönder
    /// </summary>
    public void SendHapticFeedback(XRNode hand, float intensity = 0.5f)
    {
        InputDevice device = (hand == XRNode.LeftHand) ? leftController : rightController;
        
        if (device.isValid)
        {
            HapticCapabilities capabilities;
            if (device.TryGetHapticCapabilities(out capabilities) && capabilities.supportsImpulse)
            {
                device.SendHapticImpulse(0, intensity, 0.1f);
            }
        }
    }
}
