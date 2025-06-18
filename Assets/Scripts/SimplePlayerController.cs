using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
public class SimplePlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 10f;
    public bool CanMove = true;
    
    [Header("Mouse Rotation Settings")]
    public float mouseSensitivity = 2f;
    
    [Header("Jump Settings")]
    public float jumpForce = 5f;
    
    private Rigidbody rb;
    private bool isGrounded = true;
    private bool isUIOpen = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        
        // Mouse'u kilitle (karakter rotasyonu için)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        Debug.Log("SimplePlayerController: Başlatıldı - WASD: Hareket, Mouse: Döndür, Shift: Koşma, Space: Zıplama");
    }

    void Update()
    {
        // Debug: T tuşu ile durumu kontrol et
        if (Input.GetKeyDown(KeyCode.T))
        {
            DebugPlayerState();
        }

        // Debug: Y tuşu ile hareketi zorla etkinleştir
        if (Input.GetKeyDown(KeyCode.Y))
        {
            ForceEnableMovement();
        }

        // Hareket edemiyor veya UI açıksa kontrolleri yapma
        if (!CanMove || isUIOpen) return;

        // Mouse ile karakter rotasyonu (sadece yatay)
        HandleCharacterRotation();

        if (Input.GetKeyDown(KeyCode.N))
        {
            PlayerPrefs.SetString("MyNextScene", "Bathroom");
            PlayerPrefs.Save();
            SceneManager.LoadScene("BlackScreen");
        }
        
        // Jump kontr
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
        
        // Karakteri Y ekseninde döndür (sağa-sola)
        transform.Rotate(Vector3.up * mouseX);
    }
    
    // Ground detection
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
    
    // Debug fonksiyonları
    public void DebugPlayerState()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        float mouseX = Input.GetAxis("Mouse X");
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        
        Debug.Log($"SimplePlayerController Debug:" +
                  $"\n- CanMove: {CanMove}" +
                  $"\n- isUIOpen: {isUIOpen}" +
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
        Debug.Log("SimplePlayerController: Hareket zorla etkinleştirildi!");
    }
    
    // UI kontrolü için public metodlar
    public void SetUIOpen(bool uiOpen)
    {
        isUIOpen = uiOpen;
        Debug.Log($"SimplePlayerController: UI durumu = {uiOpen}");
    }
    
    public void SetCanMove(bool canMove)
    {
        CanMove = canMove;
        Debug.Log($"SimplePlayerController: CanMove = {canMove}");
    }
}
