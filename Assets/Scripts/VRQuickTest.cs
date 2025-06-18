using UnityEngine;

/// <summary>
/// VRInputController'ı test etmek için en basit script
/// Sadece input'ları console'da gösterir
/// </summary>
public class VRQuickTest : MonoBehaviour
{
    private VRInputController vrInput;
    
    void Start()
    {
        // VR Input Controller'ı bul
        vrInput = FindFirstObjectByType<VRInputController>();
        
        if (vrInput == null)
        {
            Debug.LogError("❌ VRInputController bulunamadı!");
            Debug.Log("🔧 Lütfen sahneye VRInputController ekleyin");
        }
        else
        {
            Debug.Log("✅ VR Test başlatıldı! Controller butonlarına basın.");
        }
    }
    
    void Update()
    {
        if (vrInput == null) return;
        
        // Basit input testleri
        TestTriggers();
        TestGrips(); 
        TestJoysticks();
    }
    
    private void TestTriggers()
    {
        // Sol Trigger
        if (vrInput.IsLeftTriggerPressed)
        {
            Debug.Log("🔫 SOL TRIGGER BASILDI!");
        }
        
        // Sağ Trigger  
        if (vrInput.IsRightTriggerPressed)
        {
            Debug.Log("🔫 SAĞ TRIGGER BASILDI!");
        }
    }
    
    private void TestGrips()
    {
        // Sol Grip
        if (vrInput.IsLeftGripPressed)
        {
            Debug.Log("✊ SOL GRIP BASILDI!");
        }
        
        // Sağ Grip
        if (vrInput.IsRightGripPressed)
        {
            Debug.Log("✊ SAĞ GRIP BASILDI!");
        }
    }
    
    private void TestJoysticks()
    {
        // Sol Joystick
        Vector2 leftStick = vrInput.LeftJoystickInput;
        if (leftStick.magnitude > 0.5f)
        {
            Debug.Log($"🕹️ SOL JOYSTICK: {leftStick}");
        }
        
        // Sağ Joystick
        Vector2 rightStick = vrInput.RightJoystickInput;
        if (rightStick.magnitude > 0.5f)
        {
            Debug.Log($"🕹️ SAĞ JOYSTICK: {rightStick}");
        }
    }
} 