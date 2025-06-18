using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.XR;
using System.Collections.Generic;
using UnityEngine.Events;
public class InteractableUIController : MonoBehaviour
{
    [Header("Settings")]
    [TextArea] public string interactionText;
    public Vector3 boxSize = Vector3.one * 5f;
    public Vector3 promptOffset = Vector3.up * 2f;
    public GameObject promptObject;
    private InputDevice leftController;
    private InputDevice rightController;
    private TextMeshPro prompt3DText;
    private bool isInRange;
    private bool isPanelOpen;
    
    // Button press tracking
    private bool leftSecondaryButtonPreviousState;
    private bool rightSecondaryButtonPreviousState;
    void Awake()
    {
        InputDevices.deviceConnected += OnDeviceConnected;
        InitializeOpenXRControllers();
    }

    void OnDestroy()
    {
        InputDevices.deviceConnected -= OnDeviceConnected;
    }

    void OnDeviceConnected(InputDevice device)
    {
        if ((device.characteristics & InputDeviceCharacteristics.Left) != 0 &&
            (device.characteristics & InputDeviceCharacteristics.Controller) != 0)
        {
            leftController = device;
        }
        if ((device.characteristics & InputDeviceCharacteristics.Right) != 0 &&
            (device.characteristics & InputDeviceCharacteristics.Controller) != 0)
        {
            rightController = device;
        }
        
    }   

    void InitializeOpenXRControllers()
    {
        var allDevices = new List<InputDevice>();
        InputDevices.GetDevices(allDevices);    

        foreach (var d in allDevices)
        {
            if ((d.characteristics & InputDeviceCharacteristics.Left) != 0 &&
                (d.characteristics & InputDeviceCharacteristics.Controller) != 0)
            {
                leftController = d;
            }
            if ((d.characteristics & InputDeviceCharacteristics.Right) != 0 &&
                (d.characteristics & InputDeviceCharacteristics.Controller) != 0)
            {
                rightController = d;
            }
        }   
    }

    void Start()
    {
        if (promptObject != null)
        {
            prompt3DText = promptObject.GetComponent<TextMeshPro>();
            promptObject.SetActive(false);
        }// Örnek: hareketi kilitle

    }

    void Update()
    {
        UpdateRange();
        
        // Panel açıkken veya mevcut görevse etkileşim kontrolü yap
        if ((isInRange && TaskManager.Instance.currentTask == GetComponent<MyTask>()) || isPanelOpen)
            CheckForInteraction();
    }

    void LateUpdate()
    {
        if (isInRange && promptObject.activeSelf)
            UpdatePromptTransform();
    }

    // --- Range Detection & Prompt ---
    void UpdateRange()
    {
        bool playerNear = Physics.OverlapBox(transform.position, boxSize * 0.5f, transform.rotation)
                               .Any(c => c.CompareTag("Player"));

        if (playerNear && !isInRange && TaskManager.Instance.currentTask == GetComponent<MyTask>())
            ShowPrompt();
        else if (!playerNear && isInRange)
            HidePrompt();

        isInRange = playerNear;
    }

    void ShowPrompt()
    {
        if (promptObject == null) return;
        prompt3DText.text = $"E) Interact with {gameObject.name}";
        promptObject.SetActive(true);
    }

    void HidePrompt()
    {
        if (promptObject != null)
            promptObject.SetActive(false);

        if (isPanelOpen)
            ClosePanel();
    }

    void UpdatePromptTransform()
    {
        Vector3 worldPos = transform.position + promptOffset;
        promptObject.transform.position = worldPos;
        promptObject.transform.rotation = Quaternion.LookRotation(worldPos - Camera.main.transform.position);
    }

    // --- Input Handling ---
    void CheckForInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isPanelOpen) ClosePanel();
            else OpenPanel();
        }
        
        // Sol kontrolcü button down detection
        if(leftController.TryGetFeatureValue(CommonUsages.secondaryButton, out bool leftSecondaryButton))
        {
            if(leftSecondaryButton && !leftSecondaryButtonPreviousState)
            {
                if(isPanelOpen) ClosePanel();
                else OpenPanel();
            }
            leftSecondaryButtonPreviousState = leftSecondaryButton;
        }
        
        // Sağ kontrolcü button down detection
        if(rightController.TryGetFeatureValue(CommonUsages.secondaryButton, out bool rightSecondaryButton))
        {
            if(rightSecondaryButton && !rightSecondaryButtonPreviousState)
            {
                if(isPanelOpen) ClosePanel();
                else OpenPanel();
            }
            rightSecondaryButtonPreviousState = rightSecondaryButton;
        }
    }

    // --- Panel Control ---
    void OpenPanel()
    {
        isPanelOpen = true;
        promptObject.SetActive(false);
        
        // MyTask ile etkileşime geç
        MyTask myTask = GetComponent<MyTask>();
        if (myTask != null)
        {
            myTask.isInteracted = true;
            myTask.Interact(); // Task sistemine etkileşimi bildir
        }
        
        UIManager.Instance.ShowInteractionPanel(interactionText);
        EventManager.Trigger("UIOpen");
    }

    void ClosePanel()
    {
        isPanelOpen = false;
        UIManager.Instance.HideInteractionPanel();
        EventManager.Trigger("UIClose");
    }

    // --- Editor Gizmo ---
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        Gizmos.DrawWireCube(Vector3.zero, boxSize);
        Gizmos.matrix = Matrix4x4.identity;
    }
}