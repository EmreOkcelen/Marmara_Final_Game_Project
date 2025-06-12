using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// Simple VR Joystick Controller for Meta Quest/Oculus VR
/// Handles basic movement and rotation using VR joysticks
/// </summary>
public class SimpleVRJoystickController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    
    [Header("Rotation Settings")]
    [SerializeField] private bool useSnapTurn = true;
    [SerializeField] private float snapTurnAngle = 30f;
    [SerializeField] private float smoothTurnSpeed = 90f;
    [SerializeField] private float snapTurnCooldown = 0.3f;
    
    [Header("VR Components")]
    [SerializeField] private Transform vrCamera;
    [SerializeField] private CharacterController characterController;
    
    // Input variables
    private Vector2 leftThumbstick;
    private Vector2 rightThumbstick;
    private bool leftTrigger;
    private bool rightTrigger;
    private bool leftGrip;
    private bool rightGrip;
    private bool leftPrimaryButton;
    private bool rightPrimaryButton;
    
    // Internal variables
    private InputDevice leftController;
    private InputDevice rightController;
    private float lastSnapTurnTime;
    private bool isInitialized = false;
    
    private void Start()
    {
        InitializeController();
    }
    
    private void InitializeController()
    {
        // Get VR camera if not assigned
        if (vrCamera == null)
        {
            Camera cam = Camera.main;
            if (cam == null) cam = FindObjectOfType<Camera>();
            if (cam != null) vrCamera = cam.transform;
        }
        
        // Get or add character controller
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
            if (characterController == null)
            {
                characterController = gameObject.AddComponent<CharacterController>();
                // Configure character controller
                characterController.center = new Vector3(0, 1, 0);
                characterController.height = 1.8f;
                characterController.radius = 0.3f;
                characterController.stepOffset = 0.3f;
            }
        }
        
        // Initialize VR controllers
        InitializeVRDevices();
    }
    
    private void InitializeVRDevices()
    {
        // Get VR input devices
        var devices = new System.Collections.Generic.List<InputDevice>();
        InputDevices.GetDevices(devices);
        
        foreach (var device in devices)
        {
            if (device.characteristics.HasFlag(InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller))
            {
                leftController = device;
            }
            else if (device.characteristics.HasFlag(InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller))
            {
                rightController = device;
            }
        }
        
        isInitialized = leftController.isValid && rightController.isValid;
        
        if (isInitialized)
        {
            Debug.Log("VR Controllers initialized successfully!");
        }
        else
        {
            Debug.LogWarning("VR Controllers not found. Make sure VR is active.");
        }
    }
    
    private void Update()
    {
        if (!isInitialized)
        {
            InitializeVRDevices();
            return;
        }
        
        ReadVRInput();
        HandleMovement();
        HandleRotation();
        HandleInteractions();
    }
    
    private void ReadVRInput()
    {
        // Read joystick inputs
        leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out leftThumbstick);
        rightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out rightThumbstick);
        
        // Read trigger inputs
        leftController.TryGetFeatureValue(CommonUsages.trigger, out float leftTriggerValue);
        rightController.TryGetFeatureValue(CommonUsages.trigger, out float rightTriggerValue);
        leftTrigger = leftTriggerValue > 0.7f;
        rightTrigger = rightTriggerValue > 0.7f;
        
        // Read grip inputs
        leftController.TryGetFeatureValue(CommonUsages.grip, out float leftGripValue);
        rightController.TryGetFeatureValue(CommonUsages.grip, out float rightGripValue);
        leftGrip = leftGripValue > 0.7f;
        rightGrip = rightGripValue > 0.7f;
        
        // Read button inputs
        leftController.TryGetFeatureValue(CommonUsages.primaryButton, out leftPrimaryButton);
        rightController.TryGetFeatureValue(CommonUsages.primaryButton, out rightPrimaryButton);
    }
    
    private void HandleMovement()
    {
        // Use left thumbstick for movement
        if (leftThumbstick.magnitude > 0.1f)
        {
            // Get movement direction relative to camera
            Vector3 forward = vrCamera.forward;
            Vector3 right = vrCamera.right;
            
            // Remove vertical component
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();
            
            // Calculate movement vector
            Vector3 moveDirection = forward * leftThumbstick.y + right * leftThumbstick.x;
            
            // Apply speed (sprint if grip is pressed)
            float currentSpeed = (leftGrip || rightGrip) ? moveSpeed * sprintMultiplier : moveSpeed;
            Vector3 movement = moveDirection * currentSpeed * Time.deltaTime;
            
            // Apply movement
            characterController.Move(movement);
        }
        
        // Apply gravity
        if (!characterController.isGrounded)
        {
            Vector3 gravity = Physics.gravity * Time.deltaTime;
            characterController.Move(gravity);
        }
    }
    
    private void HandleRotation()
    {
        // Use right thumbstick for rotation
        if (Mathf.Abs(rightThumbstick.x) > 0.7f)
        {
            if (useSnapTurn)
            {
                // Snap turn
                if (Time.time - lastSnapTurnTime > snapTurnCooldown)
                {
                    float turnDirection = Mathf.Sign(rightThumbstick.x);
                    transform.Rotate(0, turnDirection * snapTurnAngle, 0);
                    lastSnapTurnTime = Time.time;
                    
                    // Haptic feedback
                    SendHapticFeedback(rightController, 0.3f, 0.1f);
                }
            }
            else
            {
                // Smooth turn
                float turnAmount = rightThumbstick.x * smoothTurnSpeed * Time.deltaTime;
                transform.Rotate(0, turnAmount, 0);
            }
        }
    }
    
    private void HandleInteractions()
    {
        // Handle trigger interactions
        if (leftTrigger)
        {
            OnLeftTriggerPressed();
        }
        
        if (rightTrigger)
        {
            OnRightTriggerPressed();
        }
        
        // Handle button interactions
        if (leftPrimaryButton)
        {
            OnLeftPrimaryButtonPressed();
        }
        
        if (rightPrimaryButton)
        {
            OnRightPrimaryButtonPressed();
        }
    }
    
    #region Interaction Events
    
    private void OnLeftTriggerPressed()
    {
        Debug.Log("Left Trigger Pressed");
        // Add your left trigger logic here
    }
    
    private void OnRightTriggerPressed()
    {
        Debug.Log("Right Trigger Pressed");
        // Add your right trigger logic here
    }
    
    private void OnLeftPrimaryButtonPressed()
    {
        Debug.Log("Left Primary Button Pressed");
        // Add your left button logic here
    }
    
    private void OnRightPrimaryButtonPressed()
    {
        Debug.Log("Right Primary Button Pressed");
        // Add your right button logic here
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Send haptic feedback to a controller
    /// </summary>
    /// <param name="device">VR controller device</param>
    /// <param name="amplitude">Vibration strength (0-1)</param>
    /// <param name="duration">Duration in seconds</param>
    public void SendHapticFeedback(InputDevice device, float amplitude, float duration)
    {
        if (device.isValid)
        {
            HapticCapabilities capabilities;
            if (device.TryGetHapticCapabilities(out capabilities) && capabilities.supportsImpulse)
            {
                device.SendHapticImpulse(0, amplitude, duration);
            }
        }
    }
    
    /// <summary>
    /// Send haptic feedback to both controllers
    /// </summary>
    public void SendHapticFeedbackBothHands(float amplitude = 0.5f, float duration = 0.1f)
    {
        SendHapticFeedback(leftController, amplitude, duration);
        SendHapticFeedback(rightController, amplitude, duration);
    }
    
    #endregion
    
    #region Public Properties
    
    public Vector2 LeftThumbstick => leftThumbstick;
    public Vector2 RightThumbstick => rightThumbstick;
    public bool LeftTrigger => leftTrigger;
    public bool RightTrigger => rightTrigger;
    public bool LeftGrip => leftGrip;
    public bool RightGrip => rightGrip;
    public bool LeftPrimaryButton => leftPrimaryButton;
    public bool RightPrimaryButton => rightPrimaryButton;
    public bool IsInitialized => isInitialized;
    
    #endregion
    
    #region Debug Information
    
    private void OnGUI()
    {
        if (!Application.isEditor) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        GUILayout.Label("=== VR Controller Debug ===");
        GUILayout.Label($"Initialized: {isInitialized}");
        GUILayout.Label($"Left Thumbstick: {leftThumbstick}");
        GUILayout.Label($"Right Thumbstick: {rightThumbstick}");
        GUILayout.Label($"Left Trigger: {leftTrigger}");
        GUILayout.Label($"Right Trigger: {rightTrigger}");
        GUILayout.Label($"Left Grip: {leftGrip}");
        GUILayout.Label($"Right Grip: {rightGrip}");
        GUILayout.Label($"Left Button: {leftPrimaryButton}");
        GUILayout.Label($"Right Button: {rightPrimaryButton}");
        GUILayout.Label($"Grounded: {characterController?.isGrounded}");
        GUILayout.Label($"Velocity: {characterController?.velocity}");
        GUILayout.EndArea();
    }
    
    #endregion
}
