using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    private PlayerInputs playerInputs;
    private Vector2 moveInput; 
    private Vector2 lookInput; 

    [Header("Movement Settings")]
    public float movementSpeed = 5f;

    [Header("Rotation Settings")]
    private float xRotation = 0f; 

    public float xSensitivity = 30f;
    public float ySensitivity = 30f;
    public Transform cameraTransform;       

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        playerInputs = new PlayerInputs();
    }
    private void Start()
    {
        
    }

    private void OnEnable()
    {

        playerInputs.Player.Move.performed += OnMovePerformed;
        playerInputs.Player.Move.canceled += OnMoveCanceled;

        playerInputs.Player.Look.performed += OnLookPerformed;
        playerInputs.Player.Look.canceled += OnLookCanceled;
        playerInputs.Player.Enable();


        InputSystem.onAfterUpdate += OnAfterInputUpdate;
    }

    private void OnDisable()
    {
        playerInputs.Player.Move.performed -= OnMovePerformed;
        playerInputs.Player.Move.canceled -= OnMoveCanceled;
        playerInputs.Player.Look.performed -= OnLookPerformed;
        playerInputs.Player.Look.canceled -= OnLookCanceled;
        playerInputs.Player.Disable();

        InputSystem.onAfterUpdate -= OnAfterInputUpdate;
    }


    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>().normalized;
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }


    private void OnLookPerformed(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    private void OnLookCanceled(InputAction.CallbackContext context)
    {
        lookInput = Vector2.zero;
    }


    private void OnAfterInputUpdate()
    {

        if (lookInput.x != 0f || lookInput.y != 0f)
        {
            float mouseX = lookInput.x;
            float mouseY = lookInput.y;
            

            xRotation -= mouseY * Time.deltaTime * xSensitivity;
            xRotation = Mathf.Clamp(xRotation, -60f, 60f);

            if (cameraTransform != null)
            {
                cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            }

            transform.Rotate(Vector3.up * (mouseX * Time.deltaTime) * ySensitivity);
        }

        if (moveInput != Vector2.zero)
        {
            Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);
            transform.Translate(moveDirection * movementSpeed * Time.deltaTime, Space.Self);
        }
    }
}