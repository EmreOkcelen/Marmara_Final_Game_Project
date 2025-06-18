using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.InputSystem;

public class CustomVRControllerManager : MonoBehaviour
{
    [Header("Controller Input Action Manager")]
    public ControllerInputActionManager controllerManager;
    
    [Header("Interactors")]
    public XRRayInteractor rayInteractor;
    public XRRayInteractor teleportInteractor;
    public NearFarInteractor nearFarInteractor;
    
    [Header("Movement Settings")]
    [SerializeField] private bool useSmoothMovement = true;
    [SerializeField] private bool useSmoothTurn = true;
    [SerializeField] private bool enableUIScrolling = true;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    private void Start()
    {
        // ControllerInputActionManager'ı otomatik bul
        if (controllerManager == null)
            controllerManager = FindObjectOfType<ControllerInputActionManager>();
            
        if (controllerManager == null)
        {
            Debug.LogError("ControllerInputActionManager bulunamadı!");
            return;
        }
        
        // Başlangıç ayarlarını uygula
        ApplySettings();
        
        Debug.Log("CustomVRControllerManager başarıyla başlatıldı!");
    }
    
    private void ApplySettings()
    {
        if (controllerManager != null)
        {
            // Hareket ayarlarını uygula
            controllerManager.smoothMotionEnabled = useSmoothMovement;
            controllerManager.smoothTurnEnabled = useSmoothTurn;
            controllerManager.uiScrollingEnabled = enableUIScrolling;
            
            LogCurrentSettings();
        }
    }
    
    #region Public Control Methods
    
    /// <summary>
    /// Hareket modunu değiştirir (Smooth/Teleport)
    /// </summary>
    public void ToggleMovementMode()
    {
        if (controllerManager != null)
        {
            useSmoothMovement = !useSmoothMovement;
            controllerManager.smoothMotionEnabled = useSmoothMovement;
            
            Debug.Log($"Hareket modu değiştirildi: {(useSmoothMovement ? "Smooth Movement" : "Teleport")}");
        }
    }
    
    /// <summary>
    /// Dönüş modunu değiştirir (Smooth/Snap)
    /// </summary>
    public void ToggleTurnMode()
    {
        if (controllerManager != null)
        {
            useSmoothTurn = !useSmoothTurn;
            controllerManager.smoothTurnEnabled = useSmoothTurn;
            
            Debug.Log($"Dönüş modu değiştirildi: {(useSmoothTurn ? "Smooth Turn" : "Snap Turn")}");
        }
    }
    
    /// <summary>
    /// UI kaydırma özelliğini aç/kapat
    /// </summary>
    public void ToggleUIScrolling()
    {
        if (controllerManager != null)
        {
            enableUIScrolling = !enableUIScrolling;
            controllerManager.uiScrollingEnabled = enableUIScrolling;
            
            Debug.Log($"UI Kaydırma: {(enableUIScrolling ? "Açık" : "Kapalı")}");
        }
    }
    
    /// <summary>
    /// Smooth movement'ı etkinleştir
    /// </summary>
    public void EnableSmoothMovement()
    {
        if (controllerManager != null)
        {
            useSmoothMovement = true;
            controllerManager.smoothMotionEnabled = true;
            Debug.Log("Smooth Movement etkinleştirildi");
        }
    }
    
    /// <summary>
    /// Teleport'u etkinleştir
    /// </summary>
    public void EnableTeleport()
    {
        if (controllerManager != null)
        {
            useSmoothMovement = false;
            controllerManager.smoothMotionEnabled = false;
            Debug.Log("Teleport etkinleştirildi");
        }
    }
    
    /// <summary>
    /// Smooth turn'ü etkinleştir
    /// </summary>
    public void EnableSmoothTurn()
    {
        if (controllerManager != null)
        {
            useSmoothTurn = true;
            controllerManager.smoothTurnEnabled = true;
            Debug.Log("Smooth Turn etkinleştirildi");
        }
    }
    
    /// <summary>
    /// Snap turn'ü etkinleştir
    /// </summary>
    public void EnableSnapTurn()
    {
        if (controllerManager != null)
        {
            useSmoothTurn = false;
            controllerManager.smoothTurnEnabled = false;
            Debug.Log("Snap Turn etkinleştirildi");
        }
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Mevcut ayarları konsola yazdır
    /// </summary>
    public void LogCurrentSettings()
    {
        if (controllerManager != null && showDebugInfo)
        {
            Debug.Log("=== VR Controller Ayarları ===");
            Debug.Log($"Smooth Movement: {controllerManager.smoothMotionEnabled}");
            Debug.Log($"Smooth Turn: {controllerManager.smoothTurnEnabled}");
            Debug.Log($"UI Scrolling: {controllerManager.uiScrollingEnabled}");
            Debug.Log("==============================");
        }
    }
    
    /// <summary>
    /// Tüm ayarları varsayılana döndür
    /// </summary>
    public void ResetToDefaults()
    {
        useSmoothMovement = true;
        useSmoothTurn = true;
        enableUIScrolling = true;
        
        ApplySettings();
        Debug.Log("Ayarlar varsayılana döndürüldü");
    }
    
    /// <summary>
    /// Kontrolcü durumunu kontrol et
    /// </summary>
    public bool IsControllerManagerReady()
    {
        return controllerManager != null;
    }
    
    #endregion
    
    #region Input Handling (Klavye ile test için)
    
    private void Update()
    {
        // Test için klavye kontrolleri
        if (Input.GetKeyDown(KeyCode.M))
            ToggleMovementMode();
            
        if (Input.GetKeyDown(KeyCode.T))
            ToggleTurnMode();
            
        if (Input.GetKeyDown(KeyCode.U))
            ToggleUIScrolling();
            
        if (Input.GetKeyDown(KeyCode.R))
            ResetToDefaults();
            
        if (Input.GetKeyDown(KeyCode.L))
            LogCurrentSettings();
    }
    
    #endregion
    
    #region Unity Events
    
    private void OnValidate()
    {
        // Inspector'da değişiklik yapıldığında ayarları uygula
        if (Application.isPlaying && controllerManager != null)
        {
            ApplySettings();
        }
    }
    
    #endregion
} 