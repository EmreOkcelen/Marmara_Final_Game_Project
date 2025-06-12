using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;

/// <summary>
/// VR Controller script that handles VR joystick inputs for movement and interaction
/// Compatible with Meta Quest/Oculus and other VR headsets
/// </summary>
public class VRController : MonoBehaviour
{
    [Header("VR Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float sprintMultiplier = 2f;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private bool enableSnapTurn = true;
    [SerializeField] private float snapTurnAngle = 30f;
    
    [Header("VR References")]
    [SerializeField] private XROrigin xrOrigin;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private CharacterController characterController;
    
    [Header("Input Actions")]
    [SerializeField] private XRController leftController;
    [SerializeField] private XRController rightController;
    
    [Header("Physics Settings")]
    [SerializeField] private float gravity = 0;
    [SerializeField] private bool enableGravity = true;
    
    // Private variables
    private Vector2 leftJoystickInput;
    private Vector2 rightJoystickInput;
    private Vector3 velocity;
    private bool isMoving;
    private bool isSprinting;
    private float lastSnapTurnTime;
    private float snapTurnCooldown = 0.3f;
    
    // Input device references
    private InputDevice leftHandDevice;
    private InputDevice rightHandDevice;
    
    private void Start()
    {
        InitializeVRController();
    }
    
    private void InitializeVRController()
    {
        // Get XR Origin if not assigned
        if (xrOrigin == null)
            xrOrigin = FindFirstObjectByType<XROrigin>();
            
        // Get camera transform if not assigned
        if (cameraTransform == null && xrOrigin != null)
            cameraTransform = xrOrigin.Camera.transform;
            
        // Get character controller if not assigned
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
            
        // If no character controller, add one
        if (characterController == null)
        {
            characterController = gameObject.AddComponent<CharacterController>();
            characterController.center = new Vector3(0, 1, 0);
            characterController.height = 2f;
            characterController.radius = 0.3f;
        }
        
        StartCoroutine(InitializeInputDevices());
    }
    
    private IEnumerator InitializeInputDevices()
    {
        // Wait for VR devices to initialize
        while (!leftHandDevice.isValid || !rightHandDevice.isValid)
        {
            leftHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            rightHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            yield return new WaitForSeconds(0.1f);
        }
        
        Debug.Log("VR Controllers initialized successfully!");
    }
    
    private void Update()
    {
        if (leftHandDevice.isValid && rightHandDevice.isValid)
        {
            HandleVRInput();
            HandleMovement();
            HandleRotation();
        }
    }
    
    private void HandleVRInput()
    {
        // Get joystick inputs
        leftHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out leftJoystickInput);
        rightHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out rightJoystickInput);
        
        // Get button inputs for sprint
        leftHandDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool leftPrimaryButton);
        rightHandDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool rightPrimaryButton);
        
        isSprinting = leftPrimaryButton || rightPrimaryButton;
        
        // Check if moving
        isMoving = leftJoystickInput.magnitude > 0.1f;
    }
    
    private void HandleMovement()
    {
        if (!isMoving) return;
        
        // Get forward direction from camera
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;
        
        // Remove Y component to keep movement on ground level
        cameraForward.y = 0;
        cameraRight.y = 0;
        
        // Normalize directions
        cameraForward.Normalize();
        cameraRight.Normalize();
        
        // Calculate movement direction
        Vector3 moveDirection = (cameraForward * leftJoystickInput.y) + (cameraRight * leftJoystickInput.x);
        
        // Apply speed
        float currentSpeed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;
        Vector3 movement = moveDirection * currentSpeed;
        
        // Apply gravity if enabled
        if (enableGravity)
        {
            if (characterController.isGrounded && velocity.y < 0)
            {
                velocity.y = -2f; // Small downward force to keep grounded
            }
            velocity.y += gravity * Time.deltaTime;
            movement.y = velocity.y;
        }
        
        // Move the character
        characterController.Move(movement * Time.deltaTime);
    }
    
    private void HandleRotation()
    {
        if (!enableSnapTurn) return;
        
        // Snap turn with right joystick
        if (Mathf.Abs(rightJoystickInput.x) > 0.7f && Time.time - lastSnapTurnTime > snapTurnCooldown)
        {
            float turnDirection = Mathf.Sign(rightJoystickInput.x);
            transform.Rotate(0, turnDirection * snapTurnAngle, 0);
            lastSnapTurnTime = Time.time;
        }
    }
    
    // Public methods for external control
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }
    
    public void SetSprintMultiplier(float multiplier)
    {
        sprintMultiplier = multiplier;
    }
    
    public void EnableSnapTurn(bool enable)
    {
        enableSnapTurn = enable;
    }
    
    public void SetSnapTurnAngle(float angle)
    {
        snapTurnAngle = angle;
    }
    
    // Haptic feedback methods
    public void SendHapticFeedback(XRNode hand, float amplitude = 0.5f, float duration = 0.1f)
    {
        InputDevice device = (hand == XRNode.LeftHand) ? leftHandDevice : rightHandDevice;
        if (device.isValid)
        {
            HapticCapabilities capabilities;
            if (device.TryGetHapticCapabilities(out capabilities) && capabilities.supportsImpulse)
            {
                device.SendHapticImpulse(0, amplitude, duration);
            }
        }
    }
    
    public void SendHapticFeedbackBothHands(float amplitude = 0.5f, float duration = 0.1f)
    {
        SendHapticFeedback(XRNode.LeftHand, amplitude, duration);
        SendHapticFeedback(XRNode.RightHand, amplitude, duration);
    }
    
    // Getters for current state
    public bool IsMoving => isMoving;
    public bool IsSprinting => isSprinting;
    public Vector2 LeftJoystickInput => leftJoystickInput;
    public Vector2 RightJoystickInput => rightJoystickInput;
    public Vector3 CurrentVelocity => characterController.velocity;
    
    private void OnDisable()
    {
        // Clean up if needed
    }
    
    // Debug information
    private void OnGUI()
    {
        if (Application.isEditor)
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label($"Left Joystick: {leftJoystickInput}");
            GUILayout.Label($"Right Joystick: {rightJoystickInput}");
            GUILayout.Label($"Is Moving: {isMoving}");
            GUILayout.Label($"Is Sprinting: {isSprinting}");
            GUILayout.Label($"Velocity: {characterController.velocity}");
            GUILayout.EndArea();
        }
    }
}
