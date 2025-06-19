using UnityEngine;

public class Game3 : MonoBehaviour
{
    [Header("Player Reference")]
    public Transform player; // Player objesi referansı
    
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 10f;
    public bool CanMove = true;
    
    [Header("Mouse Rotation Settings")]
    public float mouseSensitivity = 2f;
    
    [Header("Jump Settings")]
    public float jumpForce = 5f;
    
    [Header("Drunk Simulation Settings")]
    [Range(0f, 1f)]
    public float drunkLevel = 0f; // 0 = sober, 1 = very drunk
    [Range(0.1f, 3f)]
    public float drunkMovementMultiplierMin = 0.2f; // Minimum movement multiplier when drunk
    [Range(0.1f, 3f)]
    public float drunkMovementMultiplierMax = 2.5f; // Maximum movement multiplier when drunk
    [Range(0f, 1f)]
    public float drunkChance = 0.3f; // Chance for drunk effect to trigger
    [Range(0.1f, 2f)]
    public float drunkEffectDuration = 1f; // How long the drunk effect lasts
    [Range(0f, 180f)]
    public float maxDrunkRotationDeviation = 45f; // Max rotation deviation when drunk
    
    [Header("Auto Start Settings")]
    public bool autoStartDrunkMode = true; // Otomatik olarak sarhoş modu başlat
    public float autoStartDelay = 2f; // Kaç saniye sonra başlasın
    
    [Header("Debug")]
    public KeyCode startDrunkKey = KeyCode.F1;
    public KeyCode stopDrunkKey = KeyCode.F2;
    public KeyCode increaseDrunkKey = KeyCode.F3;
    public KeyCode decreaseDrunkKey = KeyCode.F4;
    
    [Header("Inspector Controls")]
    public ScriptControls inspectorControls;
    
    // Script control variables
    private bool isScriptActive = false;
    private bool isInitialized = false;
    
    // Private variables from SimplePlayerController
    private Rigidbody playerRb; // Player'ın Rigidbody'si
    private bool isGrounded = true;
    private bool isUIOpen = false;
    
    // Private variables for drunk simulation
    private bool isDrunk = false;
    private float currentMovementMultiplier = 1f;
    private float drunkEffectTimer = 0f;
    private Vector3 lastInputDirection = Vector3.zero;
    private bool isEffectActive = false;
    private float drunkRotationOffset = 0f;
    private float drunkRotationTimer = 0f;

    public static Game3 Instance;
    
    void Start()
    {
        // Script'i başlat
        StartScript();
    }
    
    // Ana başlatma fonksiyonu - tüm script için
    public void StartScript()
    {
        if (isScriptActive)
        {
            Debug.LogWarning("Script zaten aktif!");
            return;
        }
        
        Debug.Log("=== GAME3 SCRIPT BAŞLATILIYOR ===");
        
        // Player'ı başlat
        InitializePlayer();
        
        // Mouse kontrolünü ayarla
        SetupMouseControl();
        
        // Script'i aktif et
        isScriptActive = true;
        isInitialized = true;
        
        // Otomatik sarhoş modu başlatma
        if (autoStartDrunkMode)
        {
            Invoke(nameof(StartDrunkMode), autoStartDelay);
            Debug.Log($"Sarhoş modu {autoStartDelay} saniye sonra otomatik başlayacak...");
        }
        
        Debug.Log("Game3 Script Başlatıldı!");
        Debug.Log("Kontroller: WASD=Hareket, Mouse=Döndür, Shift=Koşma, Space=Zıplama");
        Debug.Log($"Debug: F9=ScriptKontrol, {startDrunkKey}=SarhoşBaşlat, {stopDrunkKey}=SarhoşDurdur, T=Debug, Y=HareketAktif");
        
        // Kontrolleri detaylı göster
        Invoke(nameof(ShowControls), 1f);
    }
    
    // Ana bitirme fonksiyonu - tüm script için
    public void StopScript()
    {
        if (!isScriptActive)
        {
            Debug.LogWarning("Script zaten pasif!");
            return;
        }
        
        Debug.Log("=== GAME3 SCRIPT DURDURULUYOR ===");
        
        // Sarhoş modunu durdur
        if (isDrunk)
        {
            StopDrunkMode();
        }
        
        // Hareketi durdur
        CanMove = false;
        
        // Player'ı durdur
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
        }
        
        // Mouse kontrolünü serbest bırak
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Aktif invoke'ları iptal et
        CancelInvoke();
        
        // Script'i pasif et
        isScriptActive = false;
        
        Debug.Log("Game3 Script Durduruldu!");
    }
    
    void SetupMouseControl()
    {
        // Mouse'u kilitle (karakter rotasyonu için)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void InitializePlayer()
    {
        if (player == null)
        {
            // Player yoksa otomatik oluştur
            GameObject playerObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerObj.name = "DrunkPlayer";
            player = playerObj.transform;
            player.position = new Vector3(0, 1, 0);
            
            Debug.Log("Player objesi otomatik oluşturuldu!");
        }
        
        // Player'ın Rigidbody'sini al veya ekle
        playerRb = player.GetComponent<Rigidbody>();
        if (playerRb == null)
        {
            playerRb = player.gameObject.AddComponent<Rigidbody>();
        }
        playerRb.freezeRotation = true;
        
        // Varsa diğer player controller'ları devre dışı bırak
        SimplePlayerController existingController = player.GetComponent<SimplePlayerController>();
        if (existingController != null)
        {
            existingController.enabled = false;
            Debug.Log("Mevcut SimplePlayerController devre dışı bırakıldı");
        }
        
        AddGroundDetectorToPlayer();
        
        Debug.Log($"Player başlatıldı: {player.name}");
    }

    void Update()
    {
        // Debug kontrolleri her zaman çalışır (script control için)
        HandleDebugInput();
        
        // Script aktif değilse diğer işlemleri yapma
        if (!isScriptActive) return;
        
        // T tuşu ile durumu kontrol et
        if (Input.GetKeyDown(KeyCode.T))
        {
            DebugPlayerState();
        }

        // Y tuşu ile hareketi zorla etkinleştir
        if (Input.GetKeyDown(KeyCode.Y))
        {
            ForceEnableMovement();
        }

        // Hareket edemiyor veya UI açıksa kontrolleri yapma
        if (!CanMove || isUIOpen) return;

        // Mouse ile karakter rotasyonu (sarhoş etkisiyle)
        HandleCharacterRotation();
        
        // Zıplama kontrolü
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }
        
        UpdateDrunkEffect();
    }
    
    void FixedUpdate()
    {
        // Script aktif değilse hiçbir şey yapma
        if (!isScriptActive) return;
        
        // Hareket edemiyor veya UI açıksa hiçbir şey yapma
        if (!CanMove || isUIOpen) return;
        
        HandleMovement();
    }
    
    void HandleMovement()
    {
        if (player == null) return;
        
        // WASD girişlerini al
        float horizontal = Input.GetAxis("Horizontal"); // A/D veya Sol/Sağ ok
        float vertical = Input.GetAxis("Vertical");     // W/S veya Yukarı/Aşağı ok
        
        // Shift ile koşma
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? sprintSpeed : walkSpeed;
        
        // Sarhoş etkisi uygula
        if (isDrunk)
        {
            currentSpeed = ApplyDrunkMovementEffect(currentSpeed, horizontal, vertical);
            
            // Hareket yönünü sarhoş etkisiyle değiştir
            Vector2 drunkInput = ApplyDrunkInputEffect(horizontal, vertical);
            horizontal = drunkInput.x;
            vertical = drunkInput.y;
        }
        
        // Hareket yönünü hesapla (player'ın forward ve right vektörlerine göre)
        Vector3 moveDirection = (player.forward * vertical + player.right * horizontal).normalized;
        
        // Hızı uygula
        Vector3 targetVelocity = moveDirection * currentSpeed;
        
        // Sadece yatay hareketi değiştir, dikey hızı koru (zıplama için)
        playerRb.linearVelocity = new Vector3(targetVelocity.x, playerRb.linearVelocity.y, targetVelocity.z);
    }
    
    void HandleCharacterRotation()
    {
        if (player == null) return;
        
        // Mouse X hareketi ile karakteri yatay olarak döndür
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        
        // Sarhoş etkisi uygula
        if (isDrunk)
        {
            mouseX = ApplyDrunkRotationEffect(mouseX);
        }
        
        // Player'ı Y ekseninde döndür (sağa-sola)
        player.Rotate(Vector3.up * mouseX);
    }
    
    void Jump()
    {
        if (player == null || playerRb == null) return;
        
        if (isGrounded)
        {
            // Sarhoş etkisiyle zıplama gücünü değiştir
            float actualJumpForce = jumpForce;
            if (isDrunk)
            {
                actualJumpForce *= Random.Range(0.5f, 1.5f) * (2f - drunkLevel); // Sarhoşken zıplama daha zayıf
            }
            
            playerRb.AddForce(Vector3.up * actualJumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }
    
    // Ground detection - Bu method player objesine eklenecek bir script tarafından çağrılmalı
    // Veya player objesinde OnCollisionEnter event'i dinlenmelidir
    public void OnPlayerGroundCollision(bool grounded)
    {
        isGrounded = grounded;
    }
    
    // Ground detection for this GameObject (eğer bu script player üzerindeyse)
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
    
    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
    
    void HandleDebugInput()
    {
        // Script control keys (çalışır script aktif olmasa bile)
        if (Input.GetKeyDown(KeyCode.F9))
        {
            if (isScriptActive)
                StopScript();
            else
                StartScript();
        }
        
        // Aşağıdaki kontroller sadece script aktifken çalışır
        if (!isScriptActive) return;
        
        // Start drunk mode
        if (Input.GetKeyDown(startDrunkKey))
        {
            StartDrunkMode();
        }
        
        // Stop drunk mode
        if (Input.GetKeyDown(stopDrunkKey))
        {
            StopDrunkMode();
        }
        
        // Increase drunk level
        if (Input.GetKeyDown(increaseDrunkKey))
        {
            drunkLevel = Mathf.Clamp01(drunkLevel + 0.2f);
            Debug.Log($"Drunk Level: {drunkLevel:F1}");
        }
        
        // Decrease drunk level
        if (Input.GetKeyDown(decreaseDrunkKey))
        {
            drunkLevel = Mathf.Clamp01(drunkLevel - 0.2f);
            Debug.Log($"Drunk Level: {drunkLevel:F1}");
        }
    }
    
    void UpdateDrunkEffect()
    {
        if (isEffectActive)
        {
            drunkEffectTimer -= Time.deltaTime;
            
            if (drunkEffectTimer <= 0f)
            {
                // End drunk effect
                isEffectActive = false;
                currentMovementMultiplier = 1f;
            }
        }
    }
    
    // Drunk effect functions
    float ApplyDrunkMovementEffect(float currentSpeed, float horizontal, float vertical)
    {
        // Random chance to trigger drunk movement effect
        if (!isEffectActive && Random.Range(0f, 1f) < drunkChance * drunkLevel)
        {
            // Start drunk effect
            isEffectActive = true;
            drunkEffectTimer = drunkEffectDuration;
            
            // Calculate currentMovementMultiplier
            currentMovementMultiplier = Random.Range(drunkMovementMultiplierMin, drunkMovementMultiplierMax);
            
            Debug.Log($"Drunk Movement Effect! Speed Multiplier: {currentMovementMultiplier:F2}");
        }
        
        return currentSpeed * currentMovementMultiplier;
    }
    
    Vector2 ApplyDrunkInputEffect(float horizontal, float vertical)
    {
        // Add random input deviation when drunk
        if (isDrunk && drunkLevel > 0)
        {
            float deviationStrength = drunkLevel * 0.5f;
            
            // Add random offset to input
            horizontal += Random.Range(-deviationStrength, deviationStrength);
            vertical += Random.Range(-deviationStrength, deviationStrength);
            
            // Clamp to reasonable values
            horizontal = Mathf.Clamp(horizontal, -1.5f, 1.5f);
            vertical = Mathf.Clamp(vertical, -1.5f, 1.5f);
        }
        
        return new Vector2(horizontal, vertical);
    }
    
    float ApplyDrunkRotationEffect(float mouseX)
    {
        // Add random rotation deviation
        if (isDrunk && drunkLevel > 0)
        {
            // Update drunk rotation offset periodically
            drunkRotationTimer -= Time.deltaTime;
            if (drunkRotationTimer <= 0)
            {
                drunkRotationOffset = Random.Range(-maxDrunkRotationDeviation * drunkLevel, maxDrunkRotationDeviation * drunkLevel);
                drunkRotationTimer = Random.Range(0.1f, 0.5f); // Update offset every 0.1-0.5 seconds
            }
            
            // Apply drunk rotation offset
            mouseX += drunkRotationOffset * Time.deltaTime;
        }
        
        return mouseX;
    }
    
    // Public method to start drunk mode
    public void StartDrunkMode()
    {
        isDrunk = true;
        Debug.Log($"Drunk Mode Started! Drunk Level: {drunkLevel:F1}");
        
        // Visual feedback - change player color
        if (player != null)
        {
            Renderer renderer = player.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.Lerp(Color.red, Color.green, drunkLevel);
            }
        }
    }
    
    // Public method to stop drunk mode
    public void StopDrunkMode()
    {
        isDrunk = false;
        isEffectActive = false;
        currentMovementMultiplier = 1f;
        drunkEffectTimer = 0f;
        drunkRotationOffset = 0f;
        drunkRotationTimer = 0f;
        
        Debug.Log("Drunk Mode Stopped!");
        
        // Visual feedback - return to normal color
        if (player != null)
        {
            Renderer renderer = player.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.white;
            }
        }
    }
    
    // Public method to set drunk level
    public void SetDrunkLevel(float level)
    {
        drunkLevel = Mathf.Clamp01(level);
        Debug.Log($"Drunk Level set to: {drunkLevel:F1}");
        
        // Update visual feedback if drunk
        if (isDrunk && player != null)
        {
            Renderer renderer = player.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.Lerp(Color.red, Color.green, drunkLevel);
            }
        }
    }
    
    // Debug method to show current status
    public void ShowStatus()
    {
        Debug.Log("=== DRUNK CONTROLLER STATUS ===");
        Debug.Log($"Is Drunk: {isDrunk}");
        Debug.Log($"Drunk Level: {drunkLevel:F1}");
        Debug.Log($"Current Movement Multiplier: {currentMovementMultiplier:F2}");
        Debug.Log($"Effect Active: {isEffectActive}");
        Debug.Log($"Effect Timer: {drunkEffectTimer:F1}s");
        Debug.Log("==============================");
    }
    
    // Debug fonksiyonları (SimplePlayerController'dan alınan)
    public void DebugPlayerState()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        float mouseX = Input.GetAxis("Mouse X");
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        
        Debug.Log($"Game3 DrunkPlayerController Debug:" +
                  $"\n- Script Aktif: {isScriptActive}" +
                  $"\n- Başlatılmış: {isInitialized}" +
                  $"\n- Player: {(player != null ? player.name : "NULL")}" +
                  $"\n- CanMove: {CanMove}" +
                  $"\n- isUIOpen: {isUIOpen}" +
                  $"\n- isDrunk: {isDrunk}" +
                  $"\n- drunkLevel: {drunkLevel}" +
                  $"\n- currentMovementMultiplier: {currentMovementMultiplier}" +
                  $"\n- Horizontal Input: {horizontal}" +
                  $"\n- Vertical Input: {vertical}" +
                  $"\n- Mouse X Input: {mouseX}" +
                  $"\n- isRunning: {isRunning}" +
                  $"\n- isGrounded: {isGrounded}" +
                  $"\n- playerRb.velocity: {(playerRb != null ? playerRb.linearVelocity.ToString() : "NULL")}" +
                  $"\n- Player Position: {(player != null ? player.position.ToString() : "NULL")}" +
                  $"\n- Player Rotation Y: {(player != null ? player.rotation.eulerAngles.y.ToString() : "NULL")}");
    }
    
    public void ForceEnableMovement()
    {
        CanMove = true;
        isUIOpen = false;
        if (playerRb != null)
        {
            playerRb.isKinematic = false;
        }
        Debug.Log("Game3 DrunkPlayerController: Hareket zorla etkinleştirildi!");
    }
    
    // UI kontrolü için public metodlar
    public void SetUIOpen(bool uiOpen)
    {
        isUIOpen = uiOpen;
        Debug.Log($"Game3 DrunkPlayerController: UI durumu = {uiOpen}");
    }
    
    public void SetCanMove(bool canMove)
    {
        CanMove = canMove;
        Debug.Log($"Game3 DrunkPlayerController: CanMove = {canMove}");
    }

    // Public method to assign a player object at runtime
    public void AssignPlayer(Transform newPlayer)
    {
        if (newPlayer == null)
        {
            Debug.LogWarning("Assigned player is null!");
            return;
        }
        
        player = newPlayer;
        InitializePlayer();
        Debug.Log($"New player assigned: {player.name}");
    }
    
    // Public method to get current player
    public Transform GetPlayer()
    {
        return player;
    }
    
    // Public method to check if player is assigned
    public bool HasPlayer()
    {
        return player != null;
    }

    // Additional helper method for UI integration
    public string GetDrunkStatusText()
    {
        if (!isDrunk)
            return "Sober";
        
        if (drunkLevel < 0.3f)
            return "Slightly Drunk";
        else if (drunkLevel < 0.6f)
            return "Drunk";
        else
            return "Very Drunk";
    }
    
    public Color GetDrunkStatusColor()
    {
        if (!isDrunk)
            return Color.white;
        
        return Color.Lerp(Color.yellow, Color.red, drunkLevel);
    }
    
    // Player objesine eklenecek Ground Detection Component
    [System.Serializable]
    public class PlayerGroundDetector : MonoBehaviour
    {
        private Game3 controller;
        
        void Start()
        {
            // Game3 controller'ı bul
            controller = FindFirstObjectByType<Game3>();
        }
        
        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Ground") && controller != null)
            {
                controller.OnPlayerGroundCollision(true);
            }
        }
        
        void OnCollisionExit(Collision collision)
        {
            if (collision.gameObject.CompareTag("Ground") && controller != null)
            {
                controller.OnPlayerGroundCollision(false);
            }
        }
    }
    
    // InitializePlayer'da ground detector ekle
    void AddGroundDetectorToPlayer()
    {
        if (player != null)
        {
            PlayerGroundDetector detector = player.GetComponent<PlayerGroundDetector>();
            if (detector == null)
            {
                player.gameObject.AddComponent<PlayerGroundDetector>();
                Debug.Log("Ground detector eklendi: " + player.name);
            }
        }
    }
    
    // Script durumu kontrol metodları
    public bool IsScriptActive()
    {
        return isScriptActive;
    }
    
    public bool IsInitialized()
    {
        return isInitialized;
    }
    
    // Script'i yeniden başlat
    public void RestartScript()
    {
        Debug.Log("Script yeniden başlatılıyor...");
        StopScript();
        StartScript();
    }
    
    // Script durumunu göster
    public void ShowScriptStatus()
    {
        Debug.Log("=== GAME3 SCRIPT DURUMU ===");
        Debug.Log($"Script Aktif: {isScriptActive}");
        Debug.Log($"Başlatılmış: {isInitialized}");
        Debug.Log($"Player Var: {(player != null ? player.name : "YOK")}");
        Debug.Log($"Hareket Edebilir: {CanMove}");
        Debug.Log($"Sarhoş: {isDrunk}");
        Debug.Log($"Sarhoş Seviyesi: {drunkLevel:F1}");
        Debug.Log($"Otomatik Başlatma: {autoStartDrunkMode}");
        Debug.Log("========================");
    }
    
    // Tüm kontrolleri göster
    public void ShowControls()
    {
        Debug.Log("=== GAME3 KONTROLLERI ===");
        Debug.Log("HAREKET:");
        Debug.Log("- WASD: Hareket");
        Debug.Log("- Mouse: Döndürme");
        Debug.Log("- Shift: Koşma");
        Debug.Log("- Space: Zıplama");
        Debug.Log("");
        Debug.Log("DEBUG KONTROLLERI:");
        Debug.Log("- F9: Script Başlat/Durdur");
        Debug.Log($"- {startDrunkKey}: Sarhoş Modu Başlat");
        Debug.Log($"- {stopDrunkKey}: Sarhoş Modu Durdur");
        Debug.Log($"- {increaseDrunkKey}: Sarhoş Seviyesi Artır");
        Debug.Log($"- {decreaseDrunkKey}: Sarhoş Seviyesi Azalt");
        Debug.Log("- T: Debug Bilgileri");
        Debug.Log("- Y: Hareketi Zorla Aktif Et");
        Debug.Log("=========================");
    }
    
    // Unity Editor'da Inspector'a buton eklemek için
    [System.Serializable]
    public class ScriptControls
    {
        [Header("Script Kontrolleri")]
        [Space]
        [Button("Script Başlat", "StartScript")]
        public bool startScriptButton;
        
        [Button("Script Durdur", "StopScript")]
        public bool stopScriptButton;
        
        [Button("Script Yeniden Başlat", "RestartScript")]
        public bool restartScriptButton;
        
        [Button("Durumu Göster", "ShowScriptStatus")]
        public bool showStatusButton;
        
        [Button("Kontrolleri Göster", "ShowControls")]
        public bool showControlsButton;
    }
    
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class ButtonAttribute : PropertyAttribute
    {
        public string MethodName { get; }
        public string ButtonName { get; }
        
        public ButtonAttribute(string buttonName, string methodName)
        {
            ButtonName = buttonName;
            MethodName = methodName;
        }
    }
}
