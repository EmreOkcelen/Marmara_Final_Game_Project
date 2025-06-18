using UnityEngine;

public class VRButtonInputExample : MonoBehaviour
{
    [Header("VR Input Controller Reference")]
    public VRInputController vrInputController;
    
    [Header("Action Settings")]
    public bool enableDebugLogs = true;
    
    void Start()
    {
        // Eğer VRInputController atanmamışsa otomatik bul
        if (vrInputController == null)
        {
            vrInputController = FindFirstObjectByType<VRInputController>();
        }
        
        if (vrInputController == null)
        {
            Debug.LogError("VRInputController bulunamadı!");
        }
    }
    
    void Update()
    {
        if (vrInputController == null) return;
        
        // Trigger butonları kontrolü
        CheckTriggerInputs();
        
        // Grip butonları kontrolü  
        CheckGripInputs();
        
        // Joystick inputları
        CheckJoystickInputs();
        
        // Primary butonları (A/X butonları)
        CheckPrimaryButtons();
    }
    
    private void CheckTriggerInputs()
    {
        // Sol trigger
        if (vrInputController.IsLeftTriggerPressed)
        {
            if (enableDebugLogs)
                Debug.Log("Sol trigger basılı!");
            
            // Sol trigger ile yapılacak işlemler
            OnLeftTriggerPressed();
        }
        
        // Sağ trigger
        if (vrInputController.IsRightTriggerPressed)
        {
            if (enableDebugLogs)
                Debug.Log("Sağ trigger basılı!");
            
            // Sağ trigger ile yapılacak işlemler
            OnRightTriggerPressed();
        }
    }
    
    private void CheckGripInputs()
    {
        // Sol grip
        if (vrInputController.IsLeftGripPressed)
        {
            if (enableDebugLogs)
                Debug.Log("Sol grip basılı!");
            
            OnLeftGripPressed();
        }
        
        // Sağ grip
        if (vrInputController.IsRightGripPressed)
        {
            if (enableDebugLogs)
                Debug.Log("Sağ grip basılı!");
            
            OnRightGripPressed();
        }
    }
    
    private void CheckJoystickInputs()
    {
        // Sol joystick
        Vector2 leftJoystick = vrInputController.LeftJoystickInput;
        if (leftJoystick.magnitude > 0.1f) // Dead zone
        {
            if (enableDebugLogs)
                Debug.Log($"Sol joystick: {leftJoystick}");
            
            OnLeftJoystickMoved(leftJoystick);
        }
        
        // Sağ joystick
        Vector2 rightJoystick = vrInputController.RightJoystickInput;
        if (rightJoystick.magnitude > 0.1f)
        {
            if (enableDebugLogs)
                Debug.Log($"Sağ joystick: {rightJoystick}");
            
            OnRightJoystickMoved(rightJoystick);
        }
    }
    
    private void CheckPrimaryButtons()
    {
        // Bu kısım için VRInputController'a property eklemeniz gerekebilir
        // Şimdilik Input System ile direkt kontrol edelim
    }
    
    #region Button Action Methods
    
    private void OnLeftTriggerPressed()
    {
        // Sol trigger eylem örnekleri:
        // - Obje tutma
        // - Silah ateşleme
        // - Menu açma
        
        Debug.Log("Sol trigger eylemi gerçekleştiriliyor!");
    }
    
    private void OnRightTriggerPressed()
    {
        // Sağ trigger eylem örnekleri:
        // - Obje seçme
        // - Teleportasyon başlatma
        // - UI etkileşimi
        
        Debug.Log("Sağ trigger eylemi gerçekleştiriliyor!");
    }
    
    private void OnLeftGripPressed()
    {
        // Sol grip eylem örnekleri:
        // - Güçlü tutma
        // - Menu navigasyonu
        
        Debug.Log("Sol grip eylemi gerçekleştiriliyor!");
    }
    
    private void OnRightGripPressed()
    {
        // Sağ grip eylem örnekleri:
        // - Rotasyon kontrolü
        // - Gesture aktivasyonu
        
        Debug.Log("Sağ grip eylemi gerçekleştiriliyor!");
    }
    
    private void OnLeftJoystickMoved(Vector2 input)
    {
        // Sol joystick eylem örnekleri:
        // - Karakter hareketi
        // - Menu kaydırma
        
        // Yön kontrolü
        if (input.y > 0.7f)
            Debug.Log("İleri hareket!");
        else if (input.y < -0.7f)
            Debug.Log("Geri hareket!");
        
        if (input.x > 0.7f)
            Debug.Log("Sağa hareket!");
        else if (input.x < -0.7f)
            Debug.Log("Sola hareket!");
    }
    
    private void OnRightJoystickMoved(Vector2 input)
    {
        // Sağ joystick eylem örnekleri:
        // - Kamera rotasyonu
        // - UI scroll
        
        Debug.Log($"Sağ joystick hareketi: X={input.x:F2}, Y={input.y:F2}");
    }
    
    #endregion
} 