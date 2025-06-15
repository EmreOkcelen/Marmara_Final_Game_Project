using UnityEngine;
using System.Collections;

public class Game2 : MonoBehaviour
{

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
        // Git-gel hareketini başlat
        if (pingPongObject != null && pointA != null && pointB != null)
        {
            StartCoroutine(PingPongMovement());
        }

        SpawnGreenArea();

    }

    // Update is called once per frame
    void Update()
    {
        // Space tuşuna basıldığında ışınlanma
        if (Input.GetKeyDown(KeyCode.K))
        {
            TeleportObject();
        }
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

        while (true)
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

        // Alanı yeni pozisyon ve boyutta güncelle
        SpawnGreenArea();

        Debug.Log($"Başarılı! Yeni alan boyutu: {currentAreaSize}");
    }
}
