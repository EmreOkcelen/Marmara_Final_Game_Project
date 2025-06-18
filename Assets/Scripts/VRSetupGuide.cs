using UnityEngine;

/// <summary>
/// VRInputController kurulum kılavuzu ve örnek kullanım
/// Bu script size VRInputController'ı nasıl kuracağınızı ve kullanacağınızı gösterir
/// </summary>
public class VRSetupGuide : MonoBehaviour
{
    [Header("ADIM 1: Bu Referansları Inspector'da Atayın")]
    [SerializeField] private VRInputController vrInputController;
    
    [Header("ADIM 2: Test Ayarları")]
    [SerializeField] private bool enableInputLogging = true;
    [SerializeField] private bool enableTestActions = true;
    
    [Header("ADIM 3: Oyun Nesneleri (Opsiyonel)")]
    [SerializeField] private GameObject menuToToggle;
    [SerializeField] private GameObject objectToGrab;
    
    void Start()
    {
        // VRInputController'ı otomatik bul
        if (vrInputController == null)
        {
            vrInputController = FindFirstObjectByType<VRInputController>();
        }
        
        if (vrInputController == null)
        {
            Debug.LogError("❌ VRInputController bulunamadı! Lütfen sahneye ekleyin.");
            LogSetupInstructions();
        }
        else
        {
            Debug.Log("✅ VRInputController bulundu!");
        }
    }
    
    void Update()
    {
        if (vrInputController == null) return;
        
        // Input'ları kontrol et
        CheckAllInputs();
        
        // Test eylemleri
        if (enableTestActions)
        {
            HandleTestActions();
        }
    }
    
    private void CheckAllInputs()
    {
        if (!enableInputLogging) return;
        
        // Trigger kontrolü
        if (vrInputController.IsLeftTriggerPressed)
        {
            Debug.Log("🔫 Sol Trigger basıldı!");
        }
        
        if (vrInputController.IsRightTriggerPressed)
        {
            Debug.Log("🔫 Sağ Trigger basıldı!");
        }
        
        // Grip kontrolü
        if (vrInputController.IsLeftGripPressed)
        {
            Debug.Log("✊ Sol Grip basıldı!");
        }
        
        if (vrInputController.IsRightGripPressed)
        {
            Debug.Log("✊ Sağ Grip basıldı!");
        }
        
        // Joystick kontrolü
        Vector2 leftJoystick = vrInputController.LeftJoystickInput;
        if (leftJoystick.magnitude > 0.1f)
        {
            Debug.Log($"🕹️ Sol Joystick: X={leftJoystick.x:F2}, Y={leftJoystick.y:F2}");
        }
        
        Vector2 rightJoystick = vrInputController.RightJoystickInput;
        if (rightJoystick.magnitude > 0.1f)
        {
            Debug.Log($"🕹️ Sağ Joystick: X={rightJoystick.x:F2}, Y={rightJoystick.y:F2}");
        }
    }
    
    private void HandleTestActions()
    {
        // Sol Trigger - Menu Toggle
        if (vrInputController.IsLeftTriggerPressed && menuToToggle != null)
        {
            // Menu'yu aç/kapat
            menuToToggle.SetActive(!menuToToggle.activeInHierarchy);
            Debug.Log($"📱 Menu durumu: {menuToToggle.activeInHierarchy}");
        }
        
        // Sağ Trigger - Obje Seçme
        if (vrInputController.IsRightTriggerPressed)
        {
            SelectNearestObject();
        }
        
        // Sol Grip - Özel Eylem
        if (vrInputController.IsLeftGripPressed)
        {
            PerformSpecialAction();
        }
        
        // Sağ Grip - Reset Pozisyon
        if (vrInputController.IsRightGripPressed)
        {
            ResetPlayerPosition();
        }
        
        // Joystick Hareket Kontrolü
        HandleMovementFeedback();
    }
    
    private void SelectNearestObject()
    {
        // En yakın objeyi bul ve seç
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, 2f);
        
        if (nearbyObjects.Length > 0)
        {
            Debug.Log($"🎯 Obje seçildi: {nearbyObjects[0].name}");
            
            // Objeyi vurgula (örnek)
            Renderer objRenderer = nearbyObjects[0].GetComponent<Renderer>();
            if (objRenderer != null)
            {
                objRenderer.material.color = Color.yellow;
            }
        }
    }
    
    private void PerformSpecialAction()
    {
        Debug.Log("✨ Özel eylem gerçekleştirildi!");
        
        // Örnek: Işık efekti
        StartCoroutine(FlashEffect());
    }
    
    private System.Collections.IEnumerator FlashEffect()
    {
        // Kısa bir ışık efekti
        GameObject flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flash.transform.position = transform.position + Vector3.up * 2f;
        flash.transform.localScale = Vector3.one * 0.1f;
        
        Renderer flashRenderer = flash.GetComponent<Renderer>();
        flashRenderer.material.color = Color.cyan;
        
        for (float t = 0; t < 1f; t += Time.deltaTime * 3f)
        {
            flash.transform.localScale = Vector3.Lerp(Vector3.one * 0.1f, Vector3.one * 2f, t);
            Color flashColor = Color.Lerp(Color.cyan, Color.clear, t);
            flashRenderer.material.color = flashColor;
            yield return null;
        }
        
        Destroy(flash);
    }
    
    private void ResetPlayerPosition()
    {
        // Oyuncuyu başlangıç pozisyonuna götür
        transform.position = Vector3.zero;
        Debug.Log("🏠 Oyuncu pozisyonu sıfırlandı!");
    }
    
    private void HandleMovementFeedback()
    {
        Vector2 leftStick = vrInputController.LeftJoystickInput;
        Vector2 rightStick = vrInputController.RightJoystickInput;
        
        // Sol joystick - Hareket yönü göster
        if (leftStick.magnitude > 0.5f)
        {
            string direction = GetDirectionName(leftStick);
            Debug.Log($"🚶‍♂️ Hareket yönü: {direction}");
        }
        
        // Sağ joystick - Bakış yönü göster
        if (rightStick.magnitude > 0.5f)
        {
            string lookDirection = GetDirectionName(rightStick);
            Debug.Log($"👀 Bakış yönü: {lookDirection}");
        }
    }
    
    private string GetDirectionName(Vector2 input)
    {
        float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;
        
        if (angle >= -45f && angle < 45f) return "Sağ";
        else if (angle >= 45f && angle < 135f) return "İleri";
        else if (angle >= 135f || angle < -135f) return "Sol";
        else return "Geri";
    }
    
    private void LogSetupInstructions()
    {
        Debug.Log(@"
📋 VRInputController KURULUM ADIMLARI:

1️⃣ SAHNE HAZIRLIĞI:
   • Empty GameObject oluştur
   • Adını 'VR Player Controller' yap
   
2️⃣ COMPONENT'LER:
   • VRInputController.cs ekle
   • CharacterController ekle
   
3️⃣ VR RIG BAĞLANTILARI:
   • XR Origin'i ata
   • Left Hand Transform'u ata
   • Right Hand Transform'u ata
   • VR Camera'yı ata
   
4️⃣ INPUT ACTIONS:
   • Input Action References'ları ata
   • Left/Right Joystick Actions
   • Left/Right Trigger Actions
   • Left/Right Grip Actions
   • Left/Right Primary Button Actions
   
5️⃣ TELEPORTASYONn (Opsiyonel):
   • Teleport Line (LineRenderer) ata
   • Teleport Marker GameObject ata
   • Teleport Layer Mask ayarla
        ");
    }
    
    #region Public Methods - Dışarıdan Kullanım İçin
    
    public void OnLeftTriggerPressed()
    {
        Debug.Log("🎮 Sol Trigger event'i dışarıdan çağrıldı!");
    }
    
    public void OnRightTriggerPressed()
    {
        Debug.Log("🎮 Sağ Trigger event'i dışarıdan çağrıldı!");
    }
    
    public bool IsAnyButtonPressed()
    {
        if (vrInputController == null) return false;
        
        return vrInputController.IsLeftTriggerPressed ||
               vrInputController.IsRightTriggerPressed ||
               vrInputController.IsLeftGripPressed ||
               vrInputController.IsRightGripPressed;
    }
    
    public Vector2 GetMovementInput()
    {
        if (vrInputController == null) return Vector2.zero;
        return vrInputController.LeftJoystickInput;
    }
    
    public Vector2 GetLookInput()
    {
        if (vrInputController == null) return Vector2.zero;
        return vrInputController.RightJoystickInput;
    }
    
    #endregion
} 