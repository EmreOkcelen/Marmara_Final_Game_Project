using UnityEngine;
using System.Collections;
using System.Linq;

public class Game2 : MonoBehaviour
{
    [Header("Game Control")]
    [Tooltip("Game2 başlatılsın mı?")]
    public bool isGameStarted = false;
    public bool isGameFinished = false; // Game2 bitince true olacak

    [Header("Movement Settings")]
    public Transform targetTransform; // Hedef pozisyon
    public GameObject objectToMove;   // Taşınacak obje

    [Header("Ping Pong Movement")]
    public GameObject pingPongObject;  // Git-gel yapacak obje
    public Transform pointA;           // İlk nokta
    public Transform pointB;           // İkinci nokta
    public float moveSpeed = 2f;       // Hareket hızı

    [Header("Player Controller Reference")]
    public PlayerController playerController; // PlayerController referansı

    [Header("Timing Area Settings")]
    public GameObject greenAreaObject;     // Yeşil alan GameObject referansı
    public float initialAreaSize = 2f;     // Başlangıç alan boyutu
    public float sizeReduction = 0.2f;     // Her başarıda küçülme miktarı
    public float minAreaSize = 0.5f;       // Minimum alan boyutu

    [Header("Player Detection Settings")]
    public Vector3 boxSize = new Vector3(2f, 2f, 2f); // Trigger alanı boyutu
    public string playerTag = "Player";     // Player tag'ı
    
    private bool isInRange = false;         // Player alan içinde mi?

    private float currentAreaSize;         // Güncel alan boyutu
    private Vector3 greenAreaPosition;     // Yeşil alanın pozisyonu
    public bool isPlayerInGreenArea = false; // Oyuncu yeşil alanda mı?

    public static Game2 Instance { get; private set; } // Singleton örneği
    private void Awake()
    {
        // Singleton kontrolü
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentAreaSize = 1f;
        // Game2 başlangıçta başlatılmaz, player alana girdiğinde başlar
        Debug.Log("Game2 hazır. Player alana girdiğinde başlayacak.");
    }

    // Update is called once per frame
    void Update()
    {
        // Player alan kontrolü (Game başlamamışsa da çalışsın)
        UpdateRange();
        
        // Debug: J tuşu ile Game2'yi tamamen bitir (test için)
        
        // Game başlatılmadıysa hiçbir şey yapma
        if (!isGameStarted) return;
        
        if(isGameFinished) return; // Eğer oyun bitmişse hiçbir şey yapma
        // Space tuşuna basıldığında ışınlanma
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isPlayerInGreenArea)
            {
                OnSuccessfulTiming();
            }
        }

        // Oyuncunun yeşil alanda olup olmadığını kontrol et
    }

    // Işınlanma fonksiyonu
    void TeleportObject()
    {
        if (objectToMove != null && targetTransform != null)
        {
            objectToMove.GetComponent<Rigidbody>().isKinematic = true; // Fizik etkilerini devre dışı bırak
            objectToMove.transform.position = targetTransform.position;
            if (playerController != null)
            {
                playerController.CanMove = false; // Hareket etmeyi kısıtla
            }
            Debug.Log("Obje ışınlandı! Hareket kısıtlandı.");
        }
    }

    IEnumerator PingPongMovement()
    {
        Vector3 targetPoint = pointB.position;

        while (isGameStarted && !isGameFinished)
        {
            // Hedefe doğru hareket et
            while (Vector3.Distance(pingPongObject.transform.position, targetPoint) > 0.001f)
            {
                pingPongObject.transform.position = Vector3.MoveTowards(
                    pingPongObject.transform.position,
                    targetPoint,
                    moveSpeed * Time.deltaTime
                );
                yield return null;
            }

            // Hedefi değiştir (A -> B veya B -> A)
            targetPoint = targetPoint == pointA.position ? pointB.position : pointA.position;
        }
    }

    // Yeşil alan oluştur
    void SpawnGreenArea()
    {
        // Rastgele pozisyon belirle (A ve B noktaları arasında)
        float randomT = Random.Range(pointB.position.z, pointA.position.z); // %20-80 arası
        greenAreaPosition = new Vector3(
            pointA.position.x,
            pointA.position.y, // Y ekseni sıfır
            randomT
        );
        greenAreaObject.transform.position = greenAreaPosition;

        greenAreaObject.transform.localScale = new Vector3(
            greenAreaObject.transform.localScale.x * currentAreaSize,
            greenAreaObject.transform.localScale.y,
            greenAreaObject.transform.localScale.z 
        );

    }

    // Oyuncunun yeşil alanda olup olmadığını kontrol et


    // Başarılı zamanlama
    void OnSuccessfulTiming()
    {
        // Alan boyutunu küçült
        currentAreaSize = currentAreaSize - sizeReduction;

        // Minimum boyut kontrolü - eğer çok küçükse Game2'yi bitir
        if (currentAreaSize <= minAreaSize)
        {
            Debug.Log("Minimum alan boyutuna ulaşıldı! Game2 tamamlandı!");
            FinishGame2();
            return;
        }

        // Alanı yeni pozisyon ve boyutta güncelle
        SpawnGreenArea();

        Debug.Log($"Başarılı! Yeni alan boyutu: {currentAreaSize}");
    }

    /// <summary>
    /// Game2'yi başlatan fonksiyon - Bu çağrılana kadar game çalışmaz
    /// </summary>
    public void StartGame2()
    {
        isGameStarted = true;
        TeleportObject(); // Başlangıçta objeyi ışınla
        // Git-gel hareketini başlat
        if (pingPongObject != null && pointA != null && pointB != null)
        {
            StartCoroutine(PingPongMovement());
        }

        SpawnGreenArea();

        
        
    }
    void UpdateRange()
    {
        bool playerNear = Physics.OverlapBox(transform.position, boxSize * 0.5f, transform.rotation)
                               .Any(c => c.CompareTag("Player"));

        if (playerNear && !isInRange)
            ShowPrompt();
        else if (!playerNear && isInRange)
            HidePrompt();

        isInRange = playerNear;
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        Gizmos.DrawWireCube(Vector3.zero, boxSize);
        Gizmos.matrix = Matrix4x4.identity;
    }

    /// <summary>
    /// Game2'yi durduran fonksiyon
    /// </summary>
    public void StopGame2()
    {
        isGameStarted = false;
        StopAllCoroutines(); // Tüm coroutine'ları durdur
        Debug.Log("Game2 durduruldu!");
    }

    /// <summary>
    /// Game2'yi tamamen bitiren ve tüm aktiviteleri durduran fonksiyon
    /// Bu çağrıldıktan sonra Game2 ile ilgili hiçbir şey çalışmaz
    /// </summary>
    public void FinishGame2()
    {
        // Oyunu durdur
        isGameStarted = false;
        isGameFinished = true; // Game2 bitince true olacak
        
        
        // Tüm coroutine'ları durdur
        StopAllCoroutines();
        
        // Player hareket kısıtlamasını kaldır
        if (playerController != null)
        {
            playerController.CanMove = true;
        }
        
        // Yeşil alanı gizle
        if (greenAreaObject != null)
        {
            greenAreaObject.SetActive(false);
        }
        
        // Durumu sıfırla
        currentAreaSize = initialAreaSize;
        isPlayerInGreenArea = false;
        objectToMove.GetComponent<Rigidbody>().isKinematic = false; // Fizik etkilerini geri al
        Game1.Instance.StartGame1(); // Game1'i yeniden başlat
        
        Debug.Log("Game2 tamamen bitirildi! Yeniden başlatmak için StartGame2() çağrılmalı.");
    }

    /// <summary>
    /// Player alan içine girdiğinde çağrılan fonksiyon
    /// </summary>
    void ShowPrompt()
    {
        Debug.Log("Player Game2 alanına girdi! Game2 başlatılıyor...");
        
        // Game2'yi başlat (eğer henüz başlamamışsa)
        if (!isGameStarted && !isGameFinished && TaskManager.Instance.IsAllTasksCompleted)
        {
            StartGame2();
        }
    }

    /// <summary>
    /// Player alan dışına çıktığında çağrılan fonksiyon
    /// </summary>
    void HidePrompt()
    {
        Debug.Log("Player Game2 alanından çıktı!");
    }
}
