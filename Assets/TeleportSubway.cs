using UnityEngine;

public class TeleportSubway : MonoBehaviour
{
    [Header("Işınlanma Noktası")]
    public Transform subwayPos;

    [Header("Game3 Objesi")]
    [Tooltip("Sahnede aktif etmek istediğiniz Game3 GameObject'ini buraya atayın")]
    public GameObject game3Object;
    
    [Header("Debug")]
    public bool debugMode = true;

    bool hasTeleported = false;

    void Start()
    {
        // Debug bilgileri
        if (debugMode)
        {
            Debug.Log($"TeleportSubway başlatıldı: {gameObject.name}");
            Debug.Log($"SubwayPos: {(subwayPos != null ? subwayPos.name : "NULL")}");
            Debug.Log($"Game3Object: {(game3Object != null ? game3Object.name : "NULL")}");
            Debug.Log($"TrainController.Instance: {(TrainController.Instance != null ? "FOUND" : "NULL")}");
        }
        
        // Gerekli referansları kontrol et
        ValidateReferences();
    }
    
    void ValidateReferences()
    {
        if (subwayPos == null)
        {
            Debug.LogError("SubwayPos atanmamış! Inspector'dan atayın.");
        }
        
        if (game3Object == null)
        {
            Debug.LogWarning("Game3Object atanmamış! Inspector'dan atayın veya FindObjectOfType kullanılacak.");
            // Otomatik bulma denemesi
            var foundGame3 = FindFirstObjectByType<Game3>();
            if (foundGame3 != null)
            {
                game3Object = foundGame3.gameObject;
                Debug.Log($"Game3 otomatik bulundu: {game3Object.name}");
            }
        }
        
        // TrainController kontrolü
        if (TrainController.Instance == null)
        {
            Debug.LogWarning("TrainController.Instance bulunamadı! TrainController sahneye eklendi mi?");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (debugMode)
        {
            Debug.Log($"OnTriggerEnter tetiklendi: {other.gameObject.name}, Tag: {other.tag}");
        }
        
        if (hasTeleported) 
        {
            if (debugMode) Debug.Log("Zaten ışınlanmış, işlem iptal edildi.");
            return;
        }
        
        if (!other.CompareTag("Player")) 
        {
            if (debugMode) Debug.Log($"Player değil: {other.tag}");
            return;
        }
        
        if (debugMode) Debug.Log("Işınlanma işlemi başlıyor...");
        
        hasTeleported = true;
        PerformTeleport(other);
    }
    
    void PerformTeleport(Collider playerCollider)
    {
        try
        {
            // 1. Tren hızlansın
            if (TrainController.Instance != null)
            {
                TrainController.Instance.AccelerateAway();
                if (debugMode) Debug.Log("Tren hızlanmaya başladı.");
            }
            else
            {
                Debug.LogWarning("TrainController.Instance null, tren hızlanamadı!");
            }

            // 2. Oyuncuyu ışınla
            if (subwayPos != null)
            {
                Vector3 oldPos = playerCollider.transform.position;
                playerCollider.transform.position = subwayPos.position;
                if (debugMode) Debug.Log($"Oyuncu ışınlandı: {oldPos} → {subwayPos.position}");
            }
            else
            {
                Debug.LogError("SubwayPos null, ışınlanma yapılamadı!");
                return;
            }
            
            // 3. Player Controller'ı aktif et
            var playerCtrl = playerCollider.GetComponent<SimplePlayerController>();
            if (playerCtrl != null) 
            {
                playerCtrl.enabled = true;
                if (debugMode) Debug.Log("SimplePlayerController aktif edildi.");
            }
            else
            {
                Debug.LogWarning("SimplePlayerController bulunamadı!");
            }

            // 4. Game3'ü aktif et
            if (game3Object != null)
            {
                var game3Comp = game3Object.GetComponent<Game3>();
                if (game3Comp != null)
                {
                    game3Comp.enabled = true;
                    // Script'i de başlat
                    if (game3Comp.GetType().GetMethod("StartScript") != null)
                    {
                        game3Comp.SendMessage("StartScript", SendMessageOptions.DontRequireReceiver);
                        if (debugMode) Debug.Log("Game3 StartScript çağrıldı.");
                    }
                    if (debugMode) Debug.Log("Game3 component aktif edildi.");
                }
                else
                {
                    Debug.LogError("Game3 component bulunamadı!");
                }
                
                // GameObject'i de aktif et
                game3Object.SetActive(true);
                if (debugMode) Debug.Log("Game3 GameObject aktif edildi.");
            }
            else
            {
                Debug.LogError("Game3Object null, Game3 başlatılamadı!");
            }

            // 5. Bu teleporter objesini yok et (delay ile)
            Invoke(nameof(DestroyTeleporter), 1f);
            
            Debug.Log("✅ Oyuncu başarıyla ışınlandı ve Game3 başlatıldı!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Teleport işlemi sırasında hata: {e.Message}");
            hasTeleported = false; // Hata durumunda tekrar deneme şansı ver
        }
    }
    
    void DestroyTeleporter()
    {
        if (debugMode) Debug.Log("TeleportSubway objesi yok ediliyor...");
        Destroy(gameObject);
    }
    
    // Manual test için
    [ContextMenu("Test Teleport")]
    void TestTeleport()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Collider playerCollider = player.GetComponent<Collider>();
            if (playerCollider != null)
            {
                hasTeleported = false;
                PerformTeleport(playerCollider);
            }
            else
            {
                Debug.LogError("Player'da Collider bulunamadı!");
            }
        }
        else
        {
            Debug.LogError("Player bulunamadı!");
        }
    }
}