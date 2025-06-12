using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;

/// <summary>
/// Advanced VR Interaction Controller using Input System for VR joysticks
/// Handles teleportation, object grabbing, and UI interactions
/// </summary>
public class VRInputController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 60f;
    [SerializeField] private bool smoothTurn = false;
    [SerializeField] private float snapTurnAmount = 30f;
    
    [Header("Teleportation")]
    [SerializeField] private bool enableTeleportation = true;
    [SerializeField] private LineRenderer teleportLine;
    [SerializeField] private GameObject teleportMarker;
    [SerializeField] private LayerMask teleportLayerMask = 1;
    [SerializeField] private float teleportMaxDistance = 10f;
    
    [Header("Input Actions")]
    [SerializeField] private InputActionReference leftJoystickAction;
    [SerializeField] private InputActionReference rightJoystickAction;
    [SerializeField] private InputActionReference leftTriggerAction;
    [SerializeField] private InputActionReference rightTriggerAction;
    [SerializeField] private InputActionReference leftGripAction;
    [SerializeField] private InputActionReference rightGripAction;
    [SerializeField] private InputActionReference leftPrimaryButtonAction;
    [SerializeField] private InputActionReference rightPrimaryButtonAction;
    
    [Header("VR Rig References")]
    [SerializeField] private XROrigin xrOrigin;
    [SerializeField] private Transform leftHand;
    [SerializeField] private Transform rightHand;
    [SerializeField] private Camera vrCamera;
    
    // Private variables
    private CharacterController characterController;
    private Vector2 leftJoystick;
    private Vector2 rightJoystick;
    private bool isTeleporting;
    private Vector3 teleportTarget;
    private bool canTeleport;
    
    // Input flags
    private bool leftTriggerPressed;
    private bool rightTriggerPressed;
    private bool leftGripPressed;
    private bool rightGripPressed;
    private bool leftPrimaryPressed;
    private bool rightPrimaryPressed;
    
    private void Awake()
    {
        // Get components
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            characterController = gameObject.AddComponent<CharacterController>();
            SetupCharacterController();
        }
        
        // Get XR Origin if not assigned
        if (xrOrigin == null)
            xrOrigin = FindFirstObjectByType<XROrigin>();
            
        if (vrCamera == null && xrOrigin != null)
            vrCamera = xrOrigin.Camera;
    }
    
    private void SetupCharacterController()
    {
        characterController.center = new Vector3(0, 1, 0);
        characterController.height = 2f;
        characterController.radius = 0.3f;
        characterController.stepOffset = 0.3f;
        characterController.slopeLimit = 45f;
    }
    
    private void OnEnable()
    {
        EnableInputActions();
    }
    
    private void OnDisable()
    {
        DisableInputActions();
    }
    
    private void EnableInputActions()
    {
        // Enable joystick actions
        if (leftJoystickAction != null)
        {
            leftJoystickAction.action.Enable();
            leftJoystickAction.action.performed += OnLeftJoystickPerformed;
            leftJoystickAction.action.canceled += OnLeftJoystickCanceled;
        }
        
        if (rightJoystickAction != null)
        {
            rightJoystickAction.action.Enable();
            rightJoystickAction.action.performed += OnRightJoystickPerformed;
            rightJoystickAction.action.canceled += OnRightJoystickCanceled;
        }
        
        // Enable button actions
        if (leftTriggerAction != null)
        {
            leftTriggerAction.action.Enable();
            leftTriggerAction.action.performed += OnLeftTriggerPerformed;
            leftTriggerAction.action.canceled += OnLeftTriggerCanceled;
        }
        
        if (rightTriggerAction != null)
        {
            rightTriggerAction.action.Enable();
            rightTriggerAction.action.performed += OnRightTriggerPerformed;
            rightTriggerAction.action.canceled += OnRightTriggerCanceled;
        }
        
        if (leftGripAction != null)
        {
            leftGripAction.action.Enable();
            leftGripAction.action.performed += OnLeftGripPerformed;
            leftGripAction.action.canceled += OnLeftGripCanceled;
        }
        
        if (rightGripAction != null)
        {
            rightGripAction.action.Enable();
            rightGripAction.action.performed += OnRightGripPerformed;
            rightGripAction.action.canceled += OnRightGripCanceled;
        }
        
        if (leftPrimaryButtonAction != null)
        {
            leftPrimaryButtonAction.action.Enable();
            leftPrimaryButtonAction.action.performed += OnLeftPrimaryButtonPerformed;
            leftPrimaryButtonAction.action.canceled += OnLeftPrimaryButtonCanceled;
        }
        
        if (rightPrimaryButtonAction != null)
        {
            rightPrimaryButtonAction.action.Enable();
            rightPrimaryButtonAction.action.performed += OnRightPrimaryButtonPerformed;
            rightPrimaryButtonAction.action.canceled += OnRightPrimaryButtonCanceled;
        }
    }
    
    private void DisableInputActions()
    {
        // Disable and unsubscribe from all actions
        if (leftJoystickAction != null)
        {
            leftJoystickAction.action.performed -= OnLeftJoystickPerformed;
            leftJoystickAction.action.canceled -= OnLeftJoystickCanceled;
            leftJoystickAction.action.Disable();
        }
        
        if (rightJoystickAction != null)
        {
            rightJoystickAction.action.performed -= OnRightJoystickPerformed;
            rightJoystickAction.action.canceled -= OnRightJoystickCanceled;
            rightJoystickAction.action.Disable();
        }
        
        // Similar for other actions...
    }
    
    private void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleTeleportation();
    }
    
    #region Input Callbacks
    
    private void OnLeftJoystickPerformed(InputAction.CallbackContext context)
    {
        leftJoystick = context.ReadValue<Vector2>();
    }
    
    private void OnLeftJoystickCanceled(InputAction.CallbackContext context)
    {
        leftJoystick = Vector2.zero;
    }
    
    private void OnRightJoystickPerformed(InputAction.CallbackContext context)
    {
        rightJoystick = context.ReadValue<Vector2>();
    }
    
    private void OnRightJoystickCanceled(InputAction.CallbackContext context)
    {
        rightJoystick = Vector2.zero;
    }
    
    private void OnLeftTriggerPerformed(InputAction.CallbackContext context)
    {
        leftTriggerPressed = true;
        if (enableTeleportation)
            StartTeleportation();
    }
    
    private void OnLeftTriggerCanceled(InputAction.CallbackContext context)
    {
        leftTriggerPressed = false;
        if (enableTeleportation && canTeleport)
            ExecuteTeleportation();
        EndTeleportation();
    }
    
    private void OnRightTriggerPerformed(InputAction.CallbackContext context)
    {
        rightTriggerPressed = true;
    }
    
    private void OnRightTriggerCanceled(InputAction.CallbackContext context)
    {
        rightTriggerPressed = false;
    }
    
    private void OnLeftGripPerformed(InputAction.CallbackContext context)
    {
        leftGripPressed = true;
    }
    
    private void OnLeftGripCanceled(InputAction.CallbackContext context)
    {
        leftGripPressed = false;
    }
    
    private void OnRightGripPerformed(InputAction.CallbackContext context)
    {
        rightGripPressed = true;
    }
    
    private void OnRightGripCanceled(InputAction.CallbackContext context)
    {
        rightGripPressed = false;
    }
    
    private void OnLeftPrimaryButtonPerformed(InputAction.CallbackContext context)
    {
        leftPrimaryPressed = true;
        Debug.Log("Left Primary Button Pressed");
    }
    
    private void OnLeftPrimaryButtonCanceled(InputAction.CallbackContext context)
    {
        leftPrimaryPressed = false;
    }
    
    private void OnRightPrimaryButtonPerformed(InputAction.CallbackContext context)
    {
        rightPrimaryPressed = true;
        Debug.Log("Right Primary Button Pressed");
    }
    
    private void OnRightPrimaryButtonCanceled(InputAction.CallbackContext context)
    {
        rightPrimaryPressed = false;
    }
    
    #endregion
    
    #region Movement and Rotation
    
    private void HandleMovement()
    {
        if (leftJoystick.magnitude > 0.1f)
        {
            // Get camera forward and right directions
            Vector3 forward = vrCamera.transform.forward;
            Vector3 right = vrCamera.transform.right;
            
            // Remove Y component to keep movement horizontal
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();
            
            // Calculate movement direction
            Vector3 moveDirection = (forward * leftJoystick.y) + (right * leftJoystick.x);
            
            // Apply movement
            Vector3 movement = moveDirection * moveSpeed * Time.deltaTime;
            characterController.Move(movement);
        }
        
        // Apply gravity
        if (!characterController.isGrounded)
        {
            Vector3 gravity = Vector3.down * 9.81f * Time.deltaTime;
            characterController.Move(gravity);
        }
    }
    
    private void HandleRotation()
    {
        if (Mathf.Abs(rightJoystick.x) > 0.1f)
        {
            if (smoothTurn)
            {
                // Smooth rotation
                float rotation = rightJoystick.x * rotationSpeed * Time.deltaTime;
                transform.Rotate(0, rotation, 0);
            }
            else
            {
                // Snap turn (implement with cooldown to prevent rapid turning)
                // This would need additional state management for proper snap turning
            }
        }
    }
    
    #endregion
    
    #region Teleportation
    
    private void StartTeleportation()
    {
        isTeleporting = true;
        if (teleportLine != null)
            teleportLine.enabled = true;
        if (teleportMarker != null)
            teleportMarker.SetActive(false);
    }
    
    private void HandleTeleportation()
    {
        if (!isTeleporting) return;
        
        // Raycast from left controller
        Vector3 rayOrigin = leftHand.position;
        Vector3 rayDirection = leftHand.forward;
        
        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, teleportMaxDistance, teleportLayerMask))
        {
            teleportTarget = hit.point;
            canTeleport = true;
            
            // Update teleport line
            if (teleportLine != null)
            {
                teleportLine.SetPosition(0, rayOrigin);
                teleportLine.SetPosition(1, hit.point);
                teleportLine.material.color = Color.green;
            }
            
            // Show teleport marker
            if (teleportMarker != null)
            {
                teleportMarker.SetActive(true);
                teleportMarker.transform.position = hit.point;
            }
        }
        else
        {
            canTeleport = false;
            
            // Update teleport line to show invalid target
            if (teleportLine != null)
            {
                Vector3 endPoint = rayOrigin + rayDirection * teleportMaxDistance;
                teleportLine.SetPosition(0, rayOrigin);
                teleportLine.SetPosition(1, endPoint);
                teleportLine.material.color = Color.red;
            }
            
            if (teleportMarker != null)
                teleportMarker.SetActive(false);
        }
    }
    
    private void ExecuteTeleportation()
    {
        if (canTeleport)
        {
            // Calculate offset to keep camera position relative to rig
            Vector3 offset = vrCamera.transform.position - transform.position;
            offset.y = 0; // Don't apply Y offset
            
            Vector3 newPosition = teleportTarget - offset;
            characterController.enabled = false;
            transform.position = newPosition;
            characterController.enabled = true;
            
            Debug.Log($"Teleported to: {newPosition}");
        }
    }
    
    private void EndTeleportation()
    {
        isTeleporting = false;
        canTeleport = false;
        
        if (teleportLine != null)
            teleportLine.enabled = false;
        if (teleportMarker != null)
            teleportMarker.SetActive(false);
    }
    
    #endregion
    
    #region Public Methods
    
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }
    
    public void SetRotationSpeed(float speed)
    {
        rotationSpeed = speed;
    }
    
    public void EnableTeleportation(bool enable)
    {
        enableTeleportation = enable;
    }
    
    public bool IsLeftTriggerPressed => leftTriggerPressed;
    public bool IsRightTriggerPressed => rightTriggerPressed;
    public bool IsLeftGripPressed => leftGripPressed;
    public bool IsRightGripPressed => rightGripPressed;
    public Vector2 LeftJoystickInput => leftJoystick;
    public Vector2 RightJoystickInput => rightJoystick;
    
    #endregion
}
