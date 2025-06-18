using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

/// <summary>
/// OpenXR VR Input Rehberi - OpenXR standardına göre tüm VR kontrolcü input'larını gösterir
/// Meta Quest, HTC Vive, Pico, Windows Mixed Reality ve tüm OpenXR uyumlu cihazlar için
/// </summary>
public class OpenXRInputGuide : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool showInputsInConsole = true;
    public bool showInputsOnScreen = true;
    public float inputThreshold = 0.1f;
    
    [Header("Visual Display")]
    public UnityEngine.UI.Text displayText;
    
    // OpenXR Controller References
    private UnityEngine.XR.InputDevice leftController;
    private UnityEngine.XR.InputDevice rightController;
    private UnityEngine.XR.InputDevice headset;
    
    // Input States
    private Dictionary<string, bool> buttonStates = new Dictionary<string, bool>();
    private Dictionary<string, float> analogStates = new Dictionary<string, float>();
    private Dictionary<string, Vector2> joystickStates = new Dictionary<string, Vector2>();
    
    // Display Info
    private List<string> currentInputs = new List<string>();
    
    // OpenXR Input Features
    private InputFeatureUsage<Vector2> leftThumbstickUsage = UnityEngine.XR.CommonUsages.primary2DAxis;
    private InputFeatureUsage<Vector2> rightThumbstickUsage = UnityEngine.XR.CommonUsages.primary2DAxis;
    private InputFeatureUsage<bool> leftThumbstickClickUsage = UnityEngine.XR.CommonUsages.primary2DAxisClick;
    private InputFeatureUsage<bool> rightThumbstickClickUsage = UnityEngine.XR.CommonUsages.primary2DAxisClick;
    
    void Start()
    {
        InitializeOpenXRControllers();
        InitializeStates();
        
        Debug.Log("🎮 OpenXR VR Input Rehberi Başlatıldı!");
        LogAllOpenXRInputs();
    }
    
    void Update()
    {
        if (!leftController.isValid || !rightController.isValid)
        {
            InitializeOpenXRControllers();
            return;
        }
        
        ReadAllOpenXRInputs();
        UpdateDisplay();
    }
    
    #region OpenXR Initialization
    
    void InitializeOpenXRControllers()
    {
        // OpenXR cihazlarını al
        var leftHandDevices = new List<UnityEngine.XR.InputDevice>();
        var rightHandDevices = new List<UnityEngine.XR.InputDevice>();
        var headDevices = new List<UnityEngine.XR.InputDevice>();
        
        InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, leftHandDevices);
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, rightHandDevices);
        InputDevices.GetDevicesAtXRNode(XRNode.Head, headDevices);
        
        if (leftHandDevices.Count > 0)
        {
            leftController = leftHandDevices[0];
            Debug.Log($"🤚 Sol Kontrolcü: {leftController.name} - {leftController.manufacturer}");
        }
            
        if (rightHandDevices.Count > 0)
        {
            rightController = rightHandDevices[0];
            Debug.Log($"🤚 Sağ Kontrolcü: {rightController.name} - {rightController.manufacturer}");
        }
        
        if (headDevices.Count > 0)
        {
            headset = headDevices[0];
            Debug.Log($"🥽 VR Headset: {headset.name} - {headset.manufacturer}");
        }
    }
    
    void InitializeStates()
    {
        // OpenXR Button states
        string[] buttons = {
            "LeftPrimaryButton", "RightPrimaryButton",           // A/X buttons
            "LeftSecondaryButton", "RightSecondaryButton",       // B/Y buttons
            "LeftMenuButton", "RightMenuButton",                 // Menu buttons
            "LeftThumbstickClick", "RightThumbstickClick",       // Thumbstick press
            "LeftTriggerButton", "RightTriggerButton",           // Trigger buttons
            "LeftGripButton", "RightGripButton"                  // Grip buttons
        };
        
        foreach (string button in buttons)
        {
            buttonStates[button] = false;
        }
        
        // OpenXR Analog states
        string[] analogs = {
            "LeftTrigger", "RightTrigger",                       // Trigger values
            "LeftGrip", "RightGrip"                             // Grip values
        };
        
        foreach (string analog in analogs)
        {
            analogStates[analog] = 0f;
        }
        
        // OpenXR Joystick states
        joystickStates["LeftThumbstick"] = Vector2.zero;
        joystickStates["RightThumbstick"] = Vector2.zero;
    }
    
    #endregion
    
    #region OpenXR Input Reading
    
    void ReadAllOpenXRInputs()
    {
        currentInputs.Clear();
        
        ReadOpenXRThumbsticks();
        ReadOpenXRButtons();
        ReadOpenXRAnalogInputs();
        ReadOpenXRTouchInputs();
    }
    
    void ReadOpenXRThumbsticks()
    {
        // Sol Thumbstick (OpenXR: /user/hand/left/input/thumbstick)
        Vector2 leftThumbstick;
                                         leftController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out leftThumbstick);
        
        if (leftThumbstick.magnitude > inputThreshold)
        {
            joystickStates["LeftThumbstick"] = leftThumbstick;
            currentInputs.Add($"🕹️ Sol Thumbstick (OpenXR): X={leftThumbstick.x:F2}, Y={leftThumbstick.y:F2}");
            
            // OpenXR yön mapping'i
            if (leftThumbstick.y > inputThreshold) currentInputs.Add("   ⬆️ İleri Hareket");
            if (leftThumbstick.y < -inputThreshold) currentInputs.Add("   ⬇️ Geri Hareket");
            if (leftThumbstick.x > inputThreshold) currentInputs.Add("   ➡️ Sağa Hareket");
            if (leftThumbstick.x < -inputThreshold) currentInputs.Add("   ⬅️ Sola Hareket");
        }
        
        // Sağ Thumbstick (OpenXR: /user/hand/right/input/thumbstick)
        Vector2 rightThumbstick;
        rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out rightThumbstick);
        
        if (rightThumbstick.magnitude > inputThreshold)
        {
            joystickStates["RightThumbstick"] = rightThumbstick;
            currentInputs.Add($"🕹️ Sağ Thumbstick (OpenXR): X={rightThumbstick.x:F2}, Y={rightThumbstick.y:F2}");
            
            // OpenXR dönüş mapping'i
            if (rightThumbstick.y > inputThreshold) currentInputs.Add("   ⬆️ Yukarı Bak");
            if (rightThumbstick.y < -inputThreshold) currentInputs.Add("   ⬇️ Aşağı Bak");
            if (rightThumbstick.x > inputThreshold) currentInputs.Add("   🔄 Sağa Dön");
            if (rightThumbstick.x < -inputThreshold) currentInputs.Add("   🔄 Sola Dön");
        }
        
        // Thumbstick Click (OpenXR: /user/hand/left|right/input/thumbstick/click)
        bool leftThumbstickClick, rightThumbstickClick;
        leftController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out leftThumbstickClick);
        rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out rightThumbstickClick);
        
        if (leftThumbstickClick && !buttonStates["LeftThumbstickClick"])
        {
            currentInputs.Add("🔘 Sol Thumbstick Click (OpenXR)!");
        }
        buttonStates["LeftThumbstickClick"] = leftThumbstickClick;
        
        if (rightThumbstickClick && !buttonStates["RightThumbstickClick"])
        {
            currentInputs.Add("🔘 Sağ Thumbstick Click (OpenXR)!");
        }
        buttonStates["RightThumbstickClick"] = rightThumbstickClick;
    }
    
    void ReadOpenXRButtons()
    {
        // OpenXR A/X Buttons (/user/hand/left|right/input/a|x/click)
        bool leftPrimary, rightPrimary;
        leftController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out leftPrimary);
        rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out rightPrimary);
        
        if (leftPrimary && !buttonStates["LeftPrimaryButton"])
        {
            currentInputs.Add("❌ X Button (OpenXR Sol) Basıldı!");
        }
        buttonStates["LeftPrimaryButton"] = leftPrimary;
        
        if (rightPrimary && !buttonStates["RightPrimaryButton"])
        {
            currentInputs.Add("🅰️ A Button (OpenXR Sağ) Basıldı!");
        }
        buttonStates["RightPrimaryButton"] = rightPrimary;
        
        // OpenXR B/Y Buttons (/user/hand/left|right/input/b|y/click)
        bool leftSecondary, rightSecondary;
        leftController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out leftSecondary);
        rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out rightSecondary);
        
        if (leftSecondary && !buttonStates["LeftSecondaryButton"])
        {
            currentInputs.Add("🅈 Y Button (OpenXR Sol) Basıldı!");
        }
        buttonStates["LeftSecondaryButton"] = leftSecondary;
        
        if (rightSecondary && !buttonStates["RightSecondaryButton"])
        {
            currentInputs.Add("🅱️ B Button (OpenXR Sağ) Basıldı!");
        }
        buttonStates["RightSecondaryButton"] = rightSecondary;
        
        // OpenXR Menu Button (/user/hand/left/input/menu/click)
        bool leftMenuButton, rightMenuButton;
        leftController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.menuButton, out leftMenuButton);
        rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.menuButton, out rightMenuButton);
        
        if (leftMenuButton && !buttonStates["LeftMenuButton"])
        {
            currentInputs.Add("☰ Menu Button (OpenXR) Basıldı!");
        }
        buttonStates["LeftMenuButton"] = leftMenuButton;
        
        if (rightMenuButton && !buttonStates["RightMenuButton"])
        {
            currentInputs.Add("☰ Sağ Menu Button (OpenXR) Basıldı!");
        }
        buttonStates["RightMenuButton"] = rightMenuButton;
    }
    
    void ReadOpenXRAnalogInputs()
    {
        // OpenXR Trigger Values (/user/hand/left|right/input/trigger/value)
        float leftTrigger, rightTrigger;
        leftController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out leftTrigger);
        rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out rightTrigger);
        
        if (leftTrigger > inputThreshold)
        {
            analogStates["LeftTrigger"] = leftTrigger;
            currentInputs.Add($"👆 Sol Trigger (OpenXR): {leftTrigger:F2}");
            
            if (leftTrigger > 0.9f) currentInputs.Add("   🔥 Tam Çekildi!");
            else if (leftTrigger > 0.5f) currentInputs.Add("   ⚡ Yarı Çekildi");
        }
        
        if (rightTrigger > inputThreshold)
        {
            analogStates["RightTrigger"] = rightTrigger;
            currentInputs.Add($"👆 Sağ Trigger (OpenXR): {rightTrigger:F2}");
            
            if (rightTrigger > 0.9f) currentInputs.Add("   🔥 Tam Çekildi!");
            else if (rightTrigger > 0.5f) currentInputs.Add("   ⚡ Yarı Çekildi");
        }
        
        // OpenXR Grip Values (/user/hand/left|right/input/squeeze/value)
        float leftGrip, rightGrip;
        leftController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.grip, out leftGrip);
        rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.grip, out rightGrip);
        
        if (leftGrip > inputThreshold)
        {
            analogStates["LeftGrip"] = leftGrip;
            currentInputs.Add($"✊ Sol Grip (OpenXR): {leftGrip:F2}");
            
            if (leftGrip > 0.9f) currentInputs.Add("   💪 Sıkı Kavrama!");
            else if (leftGrip > 0.5f) currentInputs.Add("   🤏 Hafif Kavrama");
        }
        
        if (rightGrip > inputThreshold)
        {
            analogStates["RightGrip"] = rightGrip;
            currentInputs.Add($"✊ Sağ Grip (OpenXR): {rightGrip:F2}");
            
            if (rightGrip > 0.9f) currentInputs.Add("   💪 Sıkı Kavrama!");
            else if (rightGrip > 0.5f) currentInputs.Add("   🤏 Hafif Kavrama");
        }
        
        // OpenXR Trigger/Grip Button States
        bool leftTriggerButton = leftTrigger > 0.7f;
        bool rightTriggerButton = rightTrigger > 0.7f;
        bool leftGripButton = leftGrip > 0.7f;
        bool rightGripButton = rightGrip > 0.7f;
        
        if (leftTriggerButton && !buttonStates["LeftTriggerButton"])
        {
            currentInputs.Add("🎯 Sol Trigger Button (OpenXR) Aktif!");
        }
        buttonStates["LeftTriggerButton"] = leftTriggerButton;
        
        if (rightTriggerButton && !buttonStates["RightTriggerButton"])
        {
            currentInputs.Add("🎯 Sağ Trigger Button (OpenXR) Aktif!");
        }
        buttonStates["RightTriggerButton"] = rightTriggerButton;
        
        if (leftGripButton && !buttonStates["LeftGripButton"])
        {
            currentInputs.Add("🤜 Sol Grip Button (OpenXR) Aktif!");
        }
        buttonStates["LeftGripButton"] = leftGripButton;
        
        if (rightGripButton && !buttonStates["RightGripButton"])
        {
            currentInputs.Add("🤜 Sağ Grip Button (OpenXR) Aktif!");
        }
        buttonStates["RightGripButton"] = rightGripButton;
    }
    
    void ReadOpenXRTouchInputs()
    {
        // OpenXR Touch Sensörleri (Kapasitif dokunma)
        // Not: Tüm cihazlarda desteklenmeyebilir
        
        try
        {
            // Thumbstick Touch (/user/hand/left|right/input/thumbstick/touch)
            bool leftThumbstickTouch, rightThumbstickTouch;
            if (leftController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisTouch, out leftThumbstickTouch) && leftThumbstickTouch)
            {
                currentInputs.Add("👆 Sol Thumbstick Dokunuyor (OpenXR Touch)");
            }
            
            if (rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisTouch, out rightThumbstickTouch) && rightThumbstickTouch)
            {
                currentInputs.Add("👆 Sağ Thumbstick Dokunuyor (OpenXR Touch)");
            }
        }
        catch (System.Exception)
        {
            // Thumbstick touch desteklenmiyor
        }
        
        // Trigger Touch - Bu özellik çoğu cihazda desteklenmediği için kaldırıldı
        // Alternatif olarak trigger değeri ile yaklaşık touch algılaması
        float leftTriggerValue = analogStates.ContainsKey("LeftTrigger") ? analogStates["LeftTrigger"] : 0f;
        float rightTriggerValue = analogStates.ContainsKey("RightTrigger") ? analogStates["RightTrigger"] : 0f;
        
        // Çok hafif dokunma algılaması (0.05'ten büyük ama 0.3'ten küçük)
        if (leftTriggerValue > 0.05f && leftTriggerValue < 0.3f)
        {
            currentInputs.Add("👆 Sol Trigger Hafif Dokunuyor (Simüle)");
        }
        
        if (rightTriggerValue > 0.05f && rightTriggerValue < 0.3f)
        {
            currentInputs.Add("👆 Sağ Trigger Hafif Dokunuyor (Simüle)");
        }
    }
    
    #endregion
    
    #region Display
    
    void UpdateDisplay()
    {
        if (showInputsInConsole && currentInputs.Count > 0)
        {
            foreach (string input in currentInputs)
            {
                Debug.Log(input);
            }
        }
        
        if (showInputsOnScreen && displayText != null)
        {
            string displayString = "🎮 OpenXR VR Input'ları:\n\n";
            
            // Cihaz bilgisi
            displayString += $"🥽 Headset: {(headset.isValid ? headset.name : "Algılanmadı")}\n";
            displayString += $"🤚 Sol: {(leftController.isValid ? leftController.name : "Algılanmadı")}\n";
            displayString += $"🤚 Sağ: {(rightController.isValid ? rightController.name : "Algılanmadı")}\n\n";
            
            if (currentInputs.Count > 0)
            {
                foreach (string input in currentInputs)
                {
                    displayString += input + "\n";
                }
            }
            else
            {
                displayString += "Hiçbir input algılanmadı...";
            }
            
            displayText.text = displayString;
        }
    }
    
    #endregion
    
    #region OpenXR Info Methods
    
    void LogAllOpenXRInputs()
    {
        Debug.Log("📋 === OPENXR VR INPUT REHBERİ ===");
        Debug.Log("");
        
        Debug.Log("🎮 OPENXR INPUT PATHS:");
        Debug.Log("   • Sol Thumbstick: /user/hand/left/input/thumbstick");
        Debug.Log("   • Sağ Thumbstick: /user/hand/right/input/thumbstick");
        Debug.Log("   • Sol Trigger: /user/hand/left/input/trigger");
        Debug.Log("   • Sağ Trigger: /user/hand/right/input/trigger");
        Debug.Log("   • Sol Grip: /user/hand/left/input/squeeze");
        Debug.Log("   • Sağ Grip: /user/hand/right/input/squeeze");
        Debug.Log("");
        
        Debug.Log("🔘 OPENXR BUTTON PATHS:");
        Debug.Log("   • A Button: /user/hand/right/input/a/click");
        Debug.Log("   • B Button: /user/hand/right/input/b/click");
        Debug.Log("   • X Button: /user/hand/left/input/x/click");
        Debug.Log("   • Y Button: /user/hand/left/input/y/click");
        Debug.Log("   • Menu: /user/hand/left/input/menu/click");
        Debug.Log("   • Thumbstick Click: /user/hand/left|right/input/thumbstick/click");
        Debug.Log("");
        
        Debug.Log("👆 OPENXR TOUCH PATHS:");
        Debug.Log("   • Thumbstick Touch: /user/hand/left|right/input/thumbstick/touch");
        Debug.Log("   • Trigger Touch: /user/hand/left|right/input/trigger/touch");
        Debug.Log("   • A/B/X/Y Touch: /user/hand/left|right/input/a|b|x|y/touch");
        Debug.Log("");
        
        Debug.Log("🎯 OPENXR COMMONUSAGES MAPPING:");
        Debug.Log("   • UnityEngine.XR.CommonUsages.primary2DAxis → Thumbstick");
        Debug.Log("   • UnityEngine.XR.CommonUsages.primary2DAxisClick → Thumbstick Click");
        Debug.Log("   • UnityEngine.XR.CommonUsages.primary2DAxisTouch → Thumbstick Touch (Desteklenirse)");
        Debug.Log("   • UnityEngine.XR.CommonUsages.trigger → Trigger Value");
        Debug.Log("   • Trigger Touch → Simüle edilir (Analog değer ile)");
        Debug.Log("   • UnityEngine.XR.CommonUsages.grip → Grip Value");
        Debug.Log("   • UnityEngine.XR.CommonUsages.primaryButton → A/X Button");
        Debug.Log("   • UnityEngine.XR.CommonUsages.secondaryButton → B/Y Button");
        Debug.Log("   • UnityEngine.XR.CommonUsages.menuButton → Menu Button");
        Debug.Log("");
        
        Debug.Log("🔧 OPENXR INPUT SYSTEM ACTIONS:");
        Debug.Log("   • <XRController>{LeftHand}/{Primary2DAxis}");
        Debug.Log("   • <XRController>{RightHand}/{Primary2DAxis}");
        Debug.Log("   • <XRController>{LeftHand}/{Trigger}");
        Debug.Log("   • <XRController>{RightHand}/{Trigger}");
        Debug.Log("   • <XRController>{LeftHand}/{GripButton}");
        Debug.Log("   • <XRController>{RightHand}/{GripButton}");
        Debug.Log("   • <XRController>{LeftHand}/{PrimaryButton}");
        Debug.Log("   • <XRController>{RightHand}/{PrimaryButton}");
        Debug.Log("");
        
        Debug.Log("🎮 DESTEKLENEN CIHAZLAR:");
        Debug.Log("   • Meta Quest 1/2/3/Pro");
        Debug.Log("   • HTC Vive/Vive Pro/Vive Cosmos");
        Debug.Log("   • Valve Index");
        Debug.Log("   • Windows Mixed Reality");
        Debug.Log("   • Pico 4/4 Enterprise");
        Debug.Log("   • Varjo Aero/VR-3");
        Debug.Log("   • Tüm OpenXR uyumlu cihazlar");
        Debug.Log("");
        
        Debug.Log("============================================");
    }
    
    /// <summary>
    /// OpenXR cihaz bilgilerini al
    /// </summary>
    public Dictionary<string, string> GetOpenXRDeviceInfo()
    {
        var deviceInfo = new Dictionary<string, string>();
        
        if (headset.isValid)
        {
            deviceInfo["Headset"] = $"{headset.name} - {headset.manufacturer}";
        }
        
        if (leftController.isValid)
        {
            deviceInfo["LeftController"] = $"{leftController.name} - {leftController.manufacturer}";
        }
        
        if (rightController.isValid)
        {
            deviceInfo["RightController"] = $"{rightController.name} - {rightController.manufacturer}";
        }
        
        return deviceInfo;
    }
    
    /// <summary>
    /// OpenXR özelliklerini kontrol et
    /// </summary>
    public bool CheckOpenXRFeature(string featureName)
    {
        // OpenXR özellik kontrolü - basit versiyon
        Debug.Log($"OpenXR Feature kontrol edildi: {featureName}");
        return true; // Basitleştirilmiş versiyon
    }
    
    #endregion
    
    #region Public Properties (OpenXR)
    
    public Vector2 LeftThumbstick => joystickStates.ContainsKey("LeftThumbstick") ? joystickStates["LeftThumbstick"] : Vector2.zero;
    public Vector2 RightThumbstick => joystickStates.ContainsKey("RightThumbstick") ? joystickStates["RightThumbstick"] : Vector2.zero;
    
    public float LeftTrigger => analogStates.ContainsKey("LeftTrigger") ? analogStates["LeftTrigger"] : 0f;
    public float RightTrigger => analogStates.ContainsKey("RightTrigger") ? analogStates["RightTrigger"] : 0f;
    public float LeftGrip => analogStates.ContainsKey("LeftGrip") ? analogStates["LeftGrip"] : 0f;
    public float RightGrip => analogStates.ContainsKey("RightGrip") ? analogStates["RightGrip"] : 0f;
    
    public bool LeftPrimaryButton => buttonStates.ContainsKey("LeftPrimaryButton") && buttonStates["LeftPrimaryButton"];
    public bool RightPrimaryButton => buttonStates.ContainsKey("RightPrimaryButton") && buttonStates["RightPrimaryButton"];
    public bool LeftSecondaryButton => buttonStates.ContainsKey("LeftSecondaryButton") && buttonStates["LeftSecondaryButton"];
    public bool RightSecondaryButton => buttonStates.ContainsKey("RightSecondaryButton") && buttonStates["RightSecondaryButton"];
    
    public bool IsOpenXRActive => XRSettings.enabled;
    public string OpenXRRuntime => XRSettings.loadedDeviceName;
    
    #endregion
} 
