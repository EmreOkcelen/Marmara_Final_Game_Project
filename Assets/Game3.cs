using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
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
    
    [Header("Debug")]
    public KeyCode startDrunkKey = KeyCode.F1;
    public KeyCode stopDrunkKey = KeyCode.F2;
    public KeyCode increaseDrunkKey = KeyCode.F3;
    public KeyCode decreaseDrunkKey = KeyCode.F4;
    
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
    
    void Start()
    {
        InitializePlayer();
        
        // Mouse'u kilitle (karakter rotasyonu için)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        Debug.Log("Game3 DrunkPlayerController: Başlatıldı - WASD: Hareket, Mouse: Döndür, Shift: Koşma, Space: Zıplama");
        Debug.Log($"Sarhoş Kontrolü: {startDrunkKey}=Başlat, {stopDrunkKey}=Durdur, {increaseDrunkKey}/{decreaseDrunkKey}=Seviye");
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
        
        Debug.Log($"Player başlatıldı: {player.name}");
    }

    void Update()
    {
        // Debug kontrolleri
        HandleDebugInput();
        
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
        // Hareket edemiyor veya UI açıksa hiçbir şey yapma
        if (!CanMove || isUIOpen) return;
        
        HandleMovement();
    }
    
    void HandleMovement()
    {
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
        Vector3 moveDirection = (transform.forward * vertical + transform.right * horizontal).normalized;
        
        // Hızı uygula
        Vector3 targetVelocity = moveDirection * currentSpeed;
        
        // Sadece yatay hareketi değiştir, dikey hızı koru (zıplama için)
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
    }
    
    void HandleCharacterRotation()
    {
        // Mouse X hareketi ile karakteri yatay olarak döndür
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        
        // Sarhoş etkisi uygula
        if (isDrunk)
        {
            mouseX = ApplyDrunkRotationEffect(mouseX);
        }
        
        // Karakteri Y ekseninde döndür (sağa-sola)
        transform.Rotate(Vector3.up * mouseX);
    }
    
    void Jump()
    {
        if (isGrounded)
        {
            // Sarhoş etkisiyle zıplama gücünü değiştir
            float actualJumpForce = jumpForce;
            if (isDrunk)
            {
                actualJumpForce *= Random.Range(0.5f, 1.5f) * (2f - drunkLevel); // Sarhoşken zıplama daha zayıf
            }
            
            rb.AddForce(Vector3.up * actualJumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }
    
    // Ground detection
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
    
    void HandleDebugInput()
    {
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
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.Lerp(Color.red, Color.green, drunkLevel);
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
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.white;
        }
    }
    
    // Public method to set drunk level
    public void SetDrunkLevel(float level)
    {
        drunkLevel = Mathf.Clamp01(level);
        Debug.Log($"Drunk Level set to: {drunkLevel:F1}");
        
        // Update visual feedback if drunk
        if (isDrunk)
        {
            Renderer renderer = GetComponent<Renderer>();
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
                  $"\n- rb.velocity: {rb.linearVelocity}" +
                  $"\n- Position: {transform.position}" +
                  $"\n- Rotation Y: {transform.rotation.eulerAngles.y}");
    }
    
    public void ForceEnableMovement()
    {
        CanMove = true;
        isUIOpen = false;
        rb.isKinematic = false;
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
}
