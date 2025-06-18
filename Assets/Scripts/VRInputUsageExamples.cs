using UnityEngine;

/// <summary>
/// VRInputController'ı kullanarak farklı oyun mekaniklerini uygulama örnekleri
/// Her bir örnek, farklı bir VR input kullanım senaryosu gösterir
/// </summary>
public class VRInputUsageExamples : MonoBehaviour
{
    [Header("VR Input Controller")]
    [SerializeField] private VRInputController vrInputController;
    
    [Header("Örnek 1: Obje Tutma Sistemi")]
    [SerializeField] private Transform leftHandGrabPoint;
    [SerializeField] private Transform rightHandGrabPoint;
    private GameObject leftGrabbedObject;
    private GameObject rightGrabbedObject;
    
    [Header("Örnek 2: Menu Sistemi")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject inventoryMenu;
    private bool isPaused = false;
    
    [Header("Örnek 3: Işınlanma Sistemi")]
    [SerializeField] private LineRenderer teleportLine;
    [SerializeField] private GameObject teleportMarker;
    [SerializeField] private LayerMask teleportLayerMask = 1;
    
    [Header("Örnek 4: Skor/Puan Sistemi")]
    [SerializeField] private int playerScore = 0;
    
    void Start()
    {
        // VRInputController'ı otomatik bul
        if (vrInputController == null)
        {
            vrInputController = FindFirstObjectByType<VRInputController>();
        }
        
        if (vrInputController == null)
        {
            Debug.LogError("VRInputController bulunamadı!");
            return;
        }
        
        Debug.Log("🎮 VR Input Examples başlatıldı!");
    }
    
    void Update()
    {
        if (vrInputController == null) return;
        
        // Farklı input örneklerini çalıştır
        HandleGrabSystem();          // Örnek 1
        HandleMenuSystem();          // Örnek 2  
        HandleTeleportSystem();      // Örnek 3
        HandleScoreSystem();         // Örnek 4
        HandleDebugInputs();         // Debug bilgileri
    }
    
    #region Örnek 1: Obje Tutma Sistemi
    
    /// <summary>
    /// Trigger ile obje tutma ve bırakma sistemi
    /// Sol Trigger = Sol el ile tut
    /// Sağ Trigger = Sağ el ile tut
    /// </summary>
    private void HandleGrabSystem()
    {
        // Sol El Tutma
        if (vrInputController.IsLeftTriggerPressed)
        {
            if (leftGrabbedObject == null)
            {
                TryGrabObject(true); // true = sol el
            }
        }
        else
        {
            if (leftGrabbedObject != null)
            {
                ReleaseObject(true); // Sol eli bırak
            }
        }
        
        // Sağ El Tutma
        if (vrInputController.IsRightTriggerPressed)
        {
            if (rightGrabbedObject == null)
            {
                TryGrabObject(false); // false = sağ el
            }
        }
        else
        {
            if (rightGrabbedObject != null)
            {
                ReleaseObject(false); // Sağ eli bırak
            }
        }
    }
    
    private void TryGrabObject(bool isLeftHand)
    {
        Transform handTransform = isLeftHand ? leftHandGrabPoint : rightHandGrabPoint;
        if (handTransform == null) return;
        
        // El pozisyonunun yakınındaki objeleri ara
        Collider[] nearbyObjects = Physics.OverlapSphere(handTransform.position, 0.2f);
        
        foreach (Collider obj in nearbyObjects)
        {
            // Sadece "Grabbable" tag'li objeleri tut
            if (obj.CompareTag("Grabbable"))
            {
                GameObject grabbedObj = obj.gameObject;
                
                // Objeyi ele bağla
                grabbedObj.transform.SetParent(handTransform);
                
                // Rigidbody'yi devre dışı bırak
                Rigidbody rb = grabbedObj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                }
                
                // Hangi elde tutulduğunu kaydet
                if (isLeftHand)
                {
                    leftGrabbedObject = grabbedObj;
                    Debug.Log($"✋ Sol el ile tutuldu: {grabbedObj.name}");
                }
                else
                {
                    rightGrabbedObject = grabbedObj;
                    Debug.Log($"✋ Sağ el ile tutuldu: {grabbedObj.name}");
                }
                
                break; // Sadece bir obje tut
            }
        }
    }
    
    private void ReleaseObject(bool isLeftHand)
    {
        GameObject objToRelease = isLeftHand ? leftGrabbedObject : rightGrabbedObject;
        if (objToRelease == null) return;
        
        // Objeyi serbest bırak
        objToRelease.transform.SetParent(null);
        
        // Rigidbody'yi aktif et
        Rigidbody rb = objToRelease.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }
        
        Debug.Log($"🤲 Bırakıldı: {objToRelease.name}");
        
        // Referansı temizle
        if (isLeftHand)
            leftGrabbedObject = null;
        else
            rightGrabbedObject = null;
    }
    
    #endregion
    
    #region Örnek 2: Menu Sistemi
    
    /// <summary>
    /// Grip butonları ile menu kontrolü
    /// Sol Grip = Pause Menu
    /// Sağ Grip = Inventory Menu
    /// </summary>
    private void HandleMenuSystem()
    {
        // Sol Grip - Pause Menu
        if (vrInputController.IsLeftGripPressed)
        {
            if (!isPaused)
            {
                OpenPauseMenu();
            }
        }
        
        // Sağ Grip - Inventory Menu
        if (vrInputController.IsRightGripPressed)
        {
            ToggleInventoryMenu();
        }
    }
    
    private void OpenPauseMenu()
    {
        isPaused = true;
        Time.timeScale = 0f; // Oyunu duraklat
        
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(true);
            Debug.Log("⏸️ Oyun duraklatıldı!");
        }
    }
    
    public void ClosePauseMenu()
    {
        isPaused = false;
        Time.timeScale = 1f; // Oyunu devam ettir
        
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
            Debug.Log("▶️ Oyun devam ediyor!");
        }
    }
    
    private void ToggleInventoryMenu()
    {
        if (inventoryMenu != null)
        {
            bool isActive = inventoryMenu.activeInHierarchy;
            inventoryMenu.SetActive(!isActive);
            
            Debug.Log($"🎒 Envanter: {(!isActive ? "Açık" : "Kapalı")}");
        }
    }
    
    #endregion
    
    #region Örnek 3: Basit Işınlanma Sistemi
    
    /// <summary>
    /// Primary butonlar ile işınlanma
    /// Sol Primary (X) = Işınlanma noktası belirle
    /// Sağ Primary (A) = Işınlan
    /// </summary>
    private void HandleTeleportSystem()
    {
        // Bu örnek basit işınlanma, detaylı versiyon VRInputController'da mevcut
    }
    
    #endregion
    
    #region Örnek 4: Skor/Puan Sistemi
    
    /// <summary>
    /// Joystick ile puan kontrolü (test amaçlı)
    /// Sol Joystick Yukarı = Puan artır
    /// Sol Joystick Aşağı = Puan azalt
    /// </summary>
    private void HandleScoreSystem()
    {
        Vector2 leftJoystick = vrInputController.LeftJoystickInput;
        
        // Yukarı hareket - Puan artır
        if (leftJoystick.y > 0.8f)
        {
            IncreaseScore(10);
        }
        // Aşağı hareket - Puan azalt
        else if (leftJoystick.y < -0.8f)
        {
            DecreaseScore(5);
        }
    }
    
    private void IncreaseScore(int amount)
    {
        playerScore += amount;
        Debug.Log($"📈 Skor arttı! Yeni skor: {playerScore}");
    }
    
    private void DecreaseScore(int amount)
    {
        playerScore = Mathf.Max(0, playerScore - amount);
        Debug.Log($"📉 Skor azaldı! Yeni skor: {playerScore}");
    }
    
    #endregion
    
    #region Debug ve Test Fonksiyonları
    
    private void HandleDebugInputs()
    {
        // Sağ joystick ile debug modları
        Vector2 rightJoystick = vrInputController.RightJoystickInput;
        
        if (rightJoystick.magnitude > 0.9f)
        {
            ShowControllerStatus();
        }
    }
    
    private void ShowControllerStatus()
    {
        Debug.Log($@"
🎮 VR CONTROLLER DURUMU:
├── Sol Trigger: {(vrInputController.IsLeftTriggerPressed ? "✅ Basılı" : "❌ Basılı Değil")}
├── Sağ Trigger: {(vrInputController.IsRightTriggerPressed ? "✅ Basılı" : "❌ Basılı Değil")}
├── Sol Grip: {(vrInputController.IsLeftGripPressed ? "✅ Basılı" : "❌ Basılı Değil")}
├── Sağ Grip: {(vrInputController.IsRightGripPressed ? "✅ Basılı" : "❌ Basılı Değil")}
├── Sol Joystick: {vrInputController.LeftJoystickInput}
├── Sağ Joystick: {vrInputController.RightJoystickInput}
├── Tutulan Objeler: Sol={leftGrabbedObject?.name ?? "Yok"}, Sağ={rightGrabbedObject?.name ?? "Yok"}
└── Skor: {playerScore}
        ");
    }
    
    #endregion
    
    #region Public Methods - Dışarıdan Çağrılabilir
    
    public void ResetScore()
    {
        playerScore = 0;
        Debug.Log("🔄 Skor sıfırlandı!");
    }
    
    public void AddScore(int amount)
    {
        IncreaseScore(amount);
    }
    
    public int GetCurrentScore()
    {
        return playerScore;
    }
    
    public bool IsHoldingObject(bool leftHand)
    {
        return leftHand ? leftGrabbedObject != null : rightGrabbedObject != null;
    }
    
    public GameObject GetHeldObject(bool leftHand)
    {
        return leftHand ? leftGrabbedObject : rightGrabbedObject;
    }
    
    #endregion
} 