using UnityEngine;
using TMPro;
using System.Collections;

public class NPCGeneralController : MonoBehaviour
{
    [Header("3D Text Settings")]
    [SerializeField] private TextMeshPro npc3DText;
    [SerializeField] private float textDisplayDuration = 3f;
    [SerializeField] private bool autoFindTextComponent = true;
    
    [Header("NPC Chat Integration")]
    [SerializeField] private bool useLocalChatLLM = true;
    [SerializeField] private float llmResponseWaitTime = 2f;  // LLM response için bekleme süresi
    
    [Header("Backup Message System")]
    [SerializeField] private int backupMessageCount = 10;     // Yedekte tutulacak mesaj sayısı
    [SerializeField] private bool preloadMessagesOnStart = true; // Başlangıçta mesaj yükle
    [SerializeField] private float preloadDelay = 2f;        // Preload için bekleme süresi
    
    // Yedek mesaj sistemi
    private System.Collections.Generic.Queue<string> backupMessages = new System.Collections.Generic.Queue<string>();
    private bool isPreloading = false;
    private int totalPreloadedMessages = 0;
    
    private bool isTextDisplaying = false;
    private float lastCollisionTime = -10f;
    private float collisionCooldown = 2f;

    void Start()
    {
        // LocalChatLLM sistemini kontrol et ve gerekirse oluştur
        InitializeLLMSystem();
        
        // 3D Text component'ini otomatik bul
        if (autoFindTextComponent && npc3DText == null)
        {
            npc3DText = GetComponentInChildren<TextMeshPro>();
            if (npc3DText == null)
            {
                // Eğer yoksa oluştur
                CreateNPC3DText();
            }
        }
        
        // Başlangıçta text'i gizle
        if (npc3DText != null)
        {
            npc3DText.text = "";
            npc3DText.gameObject.SetActive(false);
        }
        
        // Yedek mesajları önceden yükle
        if (preloadMessagesOnStart)
        {
            StartCoroutine(PreloadBackupMessages());
        }
    }

    void InitializeLLMSystem()
    {
        Debug.Log("LLM sistem kontrolü başlatılıyor...");
        
        // Önce mevcut Instance'ı kontrol et
        if (LocalChatLLM.Instance != null)
        {
            Debug.Log("LocalChatLLM Instance zaten mevcut");
            return;
        }
        
        // Sahneyi tara
        LocalChatLLM foundLLM = FindFirstObjectByType<LocalChatLLM>();
        if (foundLLM != null)
        {
            Debug.Log("LocalChatLLM sahneye bulundu, Instance ayarlanıyor...");
            LocalChatLLM.Instance = foundLLM;
            return;
        }
        
        // Hiç bulunamadıysa oluştur
        Debug.Log("LocalChatLLM bulunamadı, yeni bir tane oluşturuluyor...");
        CreateLLMSystem();
    }

    void CreateLLMSystem()
    {
        // Yeni bir GameObject oluştur
        GameObject llmObject = new GameObject("LocalChatLLM_Auto");
        
        // LocalChatLLM component'ini ekle
        LocalChatLLM llmComponent = llmObject.AddComponent<LocalChatLLM>();
        
        // Instance'ı ayarla
        LocalChatLLM.Instance = llmComponent;
        
        // DontDestroyOnLoad yap ki sahne değişimlerinde silinmesin
        DontDestroyOnLoad(llmObject);
        
        Debug.Log("LocalChatLLM otomatik olarak oluşturuldu ve sahneye eklendi");
    }

    // Yedek mesaj sistemi metodları
    IEnumerator PreloadBackupMessages()
    {
        Debug.Log("Yedek mesajlar önceden yükleniyor...");
        isPreloading = true;
        
        // LLM sisteminin hazır olmasını bekle
        yield return new WaitForSeconds(preloadDelay);
        
        if (LocalChatLLM.Instance == null)
        {
            Debug.LogWarning("LLM sistemi bulunamadı, yedek mesajlar yüklenemedi");
            isPreloading = false;
            yield break;
        }
        
        // Belirlenen sayıda mesaj yükle
        for (int i = 0; i < backupMessageCount; i++)
        {
            Debug.Log($"Yedek mesaj yükleniyor: {i + 1}/{backupMessageCount}");
            
            // LLM'den yeni mesaj generate et
            LocalChatLLM.Instance.GenerateNPCReaction();
            
            // Mesajın üretilmesi için bekle
            yield return new WaitForSeconds(2f);
            
            // LLM'den mesaj al ve yedek kuyruğa ekle
            string backupMessage = ExtractMessageFromLLM();
            if (!string.IsNullOrEmpty(backupMessage))
            {
                backupMessages.Enqueue(backupMessage);
                totalPreloadedMessages++;
                Debug.Log($"Yedek mesaj eklendi: '{backupMessage}' (Toplam: {backupMessages.Count})");
            }
            else
            {
                Debug.LogWarning($"Yedek mesaj {i + 1} boş geldi, atlaniyor...");
            }
            
            // Çok hızlı request göndermemek için bekleme
            yield return new WaitForSeconds(1f);
        }
        
        isPreloading = false;
        Debug.Log($"Yedek mesaj yükleme tamamlandı! Toplam: {backupMessages.Count} mesaj hazır");
    }

    string ExtractMessageFromLLM()
    {
        try
        {
            if (LocalChatLLM.Instance == null) return "";

            // Reflection ile dialogueQueue'ya erişim
            System.Reflection.FieldInfo queueField = typeof(LocalChatLLM).GetField("dialogueQueue", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (queueField != null)
            {
                var queue = queueField.GetValue(LocalChatLLM.Instance) as System.Collections.Generic.Queue<string>;
                
                if (queue != null && queue.Count > 0)
                {
                    // Mesajı çıkar
                    string message = queue.Dequeue();
                    Debug.Log($"LLM'den yedek için mesaj çıkarıldı: '{message}'");
                    return message;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("LLM'den yedek mesaj alınırken hata: " + e.Message);
        }
        
        return "";
    }

    string GetBackupMessage()
    {
        if (backupMessages.Count > 0)
        {
            string message = backupMessages.Dequeue();
            Debug.Log($"Yedek mesaj kullanıldı: '{message}' (Kalan: {backupMessages.Count})");
            
            // Yedek mesaj azaldıysa yeni mesajlar yükle
            if (backupMessages.Count < 3 && !isPreloading)
            {
                StartCoroutine(RefillBackupMessages());
            }
            
            return message;
        }
        
        Debug.LogWarning("Yedek mesaj kuyruğu boş!");
        return "";
    }

    IEnumerator RefillBackupMessages()
    {
        if (isPreloading) yield break; // Zaten yükleniyor
        
        Debug.Log("Yedek mesajlar yeniden dolduruluyor...");
        isPreloading = true;
        
        // 3 yeni mesaj ekle
        for (int i = 0; i < 3; i++)
        {
            if (LocalChatLLM.Instance != null)
            {
                LocalChatLLM.Instance.GenerateNPCReaction();
                yield return new WaitForSeconds(2f);
                
                string newMessage = ExtractMessageFromLLM();
                if (!string.IsNullOrEmpty(newMessage))
                {
                    backupMessages.Enqueue(newMessage);
                    Debug.Log($"Yeni yedek mesaj eklendi: '{newMessage}'");
                }
            }
            
            yield return new WaitForSeconds(1f);
        }
        
        isPreloading = false;
        Debug.Log($"Yedek mesaj doldurma tamamlandı. Toplam: {backupMessages.Count}");
    }

    void Update()
    {

    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            float timeSinceLast = Time.time - lastCollisionTime;
            if (timeSinceLast >= collisionCooldown)
            {
                Debug.Log("NPC Collision detected with Player: " + collision.gameObject.name);
                lastCollisionTime = Time.time;
                
                // Chat sistemlerinden yazı al ve 3D text'e gönder
                HandlePlayerCollision();
            }
        }
    }

    void HandlePlayerCollision()
    {
        Debug.Log("HandlePlayerCollision çağrıldı");
        
        // Önce yedek mesajları kontrol et
        if (backupMessages.Count > 0)
        {
            string backupMessage = GetBackupMessage();
            if (!string.IsNullOrEmpty(backupMessage))
            {
                Debug.Log("Yedek mesaj kullanılıyor: " + backupMessage);
                DisplayText3D(backupMessage);
                return;
            }
        }
        
        // Yedek mesaj yoksa normal LLM akışına geç
        Debug.Log("Yedek mesaj yok, normal LLM akışı başlatılıyor...");
        
        // LLM sistemini tekrar kontrol et
        if (LocalChatLLM.Instance == null)
        {
            Debug.LogWarning("LocalChatLLM.Instance null! Tekrar initialize edilmeye çalışılıyor...");
            InitializeLLMSystem();
        }
        
        // Şimdi tekrar kontrol et
        if (LocalChatLLM.Instance == null)
        {
            Debug.LogError("LocalChatLLM hala oluşturulamadı! Fallback mesaj gösteriliyor.");
            DisplayText3D(GetFallbackMessage());
            return;
        }

        if (useLocalChatLLM)
        {
            Debug.Log("LLM sistemi bulundu, reaction generate ediliyor...");
            
            // İlk önce mevcut queue'yu kontrol et
            string existingMessage = GetLatestMessageFromLLM();
            if (!string.IsNullOrEmpty(existingMessage))
            {
                Debug.Log("Mevcut mesaj bulundu, direkt gösteriliyor: " + existingMessage);
                DisplayText3D(existingMessage);
                return;
            }
            
            // LLM'den yeni reaction generate et
            LocalChatLLM.Instance.GenerateNPCReaction();
            
            // Retry sistemi ile LLM'den mesaj almaya çalış
            StartCoroutine(GetLLMMessageWithRetry());
        }
        else
        {
            Debug.LogWarning("LocalChatLLM kullanımı devre dışı!");
            DisplayText3D("AI Disabled");
        }
    }

    string GetLatestMessageFromLLM()
    {
        try
        {
            if (LocalChatLLM.Instance == null)
            {
                Debug.LogError("GetLatestMessageFromLLM: LocalChatLLM Instance null!");
                return "";
            }

            // Reflection ile dialogueQueue'ya erişim
            System.Reflection.FieldInfo queueField = typeof(LocalChatLLM).GetField("dialogueQueue", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (queueField != null)
            {
                var queue = queueField.GetValue(LocalChatLLM.Instance) as System.Collections.Generic.Queue<string>;
                
                if (queue != null && queue.Count > 0)
                {
                    // Queue'nun son elemanını al ve queue'dan çıkar
                    string latestMessage = queue.Dequeue();
                    Debug.Log($"Queue'dan mesaj çıkarıldı: '{latestMessage}' (Kalan: {queue.Count})");
                    return latestMessage;
                }
                else
                {
                    Debug.Log($"LLM Queue boş. Queue null: {queue == null}, Count: {queue?.Count ?? -1}");
                    
                    // Queue boşsa yeni bir mesaj generate etmeye çalış
                    if (queue != null && queue.Count == 0)
                    {
                        Debug.Log("Queue boş, yeni GenerateNPCReaction tetikleniyor...");
                        LocalChatLLM.Instance.GenerateNPCReaction();
                    }
                }
            }
            else
            {
                Debug.LogError("dialogueQueue field'i LocalChatLLM'de bulunamadı!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("LLM'den mesaj alınırken hata: " + e.Message);
        }
        
        return "";
    }

    // LLM sisteminin durumunu kontrol eden metod
    bool IsLLMSystemReady()
    {
        if (LocalChatLLM.Instance == null)
        {
            Debug.LogWarning("IsLLMSystemReady: LocalChatLLM Instance null!");
            return false;
        }

        try
        {
            // isFetchingNewDialogue field'ini kontrol et
            System.Reflection.FieldInfo fetchingField = typeof(LocalChatLLM).GetField("isFetchingNewDialogue", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (fetchingField != null)
            {
                bool isFetching = (bool)fetchingField.GetValue(LocalChatLLM.Instance);
                Debug.Log($"LLM isFetching: {isFetching}");
                return !isFetching; // Fetching değilse ready
            }
            else
            {
                Debug.LogWarning("isFetchingNewDialogue field'i bulunamadı!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("LLM durumu kontrol edilirken hata: " + e.Message);
        }

        return true; // Varsayılan olarak ready kabul et
    }

    // LLM'den direkt mesaj çekmeye çalışan gelişmiş metod
    IEnumerator GetLLMMessageWithRetry()
    {
        int maxRetries = 8; // Retry sayısını artırdık
        int currentRetry = 0;
        
        Debug.Log("LLM'den mesaj almaya başlanıyor...");
        
        // İlk önce LLM'in queue'sunu kontrol et
        string existingMessage = GetLatestMessageFromLLM();
        if (!string.IsNullOrEmpty(existingMessage))
        {
            Debug.Log("Zaten mevcut mesaj bulundu: " + existingMessage);
            DisplayText3D(existingMessage);
            yield break;
        }
        
        while (currentRetry < maxRetries)
        {
            Debug.Log($"LLM mesaj alma denemesi: {currentRetry + 1}/{maxRetries}");
            
            if (LocalChatLLM.Instance == null)
            {
                Debug.LogError("LLM Instance retry sırasında null oldu!");
                DisplayText3D("System Error");
                yield break;
            }
            
            // Queue'yu tekrar kontrol et
            string message = GetLatestMessageFromLLM();
            Debug.Log($"LLM'den alınan mesaj: '{message}'");
            
            if (!string.IsNullOrEmpty(message))
            {
                DisplayText3D(message);
                Debug.Log("Başarıyla mesaj görüntülendi!");
                yield break;
            }
            
            // Eğer ilk birkaç denemede mesaj yoksa, yeni bir generate et
            if (currentRetry == 2)
            {
                Debug.Log("Yeni GenerateNPCReaction tetikleniyor...");
                LocalChatLLM.Instance.GenerateNPCReaction();
            }
            
            // Eğer yarıda mesaj hala yoksa fallback mesaj göster
            if (currentRetry == maxRetries / 2)
            {
                DisplayText3D("Processing...");
            }
            
            currentRetry++;
            
            // İlk denemeler hızlı, sonrakiler daha yavaş
            float waitTime = currentRetry < 3 ? 0.3f : 0.8f;
            yield return new WaitForSeconds(waitTime);
        }
        
        // Tüm denemeler başarısız oldu - fallback mesajlar kullan
        Debug.LogWarning("LLM'den mesaj alınamadı, fallback mesaj kullanılıyor!");
        string fallbackMessage = GetFallbackMessage();
        DisplayText3D(fallbackMessage);
    }

    // Fallback mesajlar - LLM çalışmazsa kullanılacak
    string GetFallbackMessage()
    {
        string[] fallbackMessages = {
            "Hey! Dikkat et!",
            "Gözünü aç!",
            "Nereye bakıyorsun?",
            "Yolumu kesme!",
            "Özür dile!"
        };
        
        return fallbackMessages[Random.Range(0, fallbackMessages.Length)];
    }

    // Acil durum için instant mesaj alma
    public void ForceShowMessage()
    {
        string message = GetLatestMessageFromLLM();
        if (!string.IsNullOrEmpty(message))
        {
            DisplayText3D(message);
        }
        else
        {
            DisplayText3D(GetFallbackMessage());
        }
    }

    // LLM queue'sunu zorla doldurma
    public void ForceGenerateMultipleMessages()
    {
        if (LocalChatLLM.Instance != null)
        {
            Debug.Log("Çoklu mesaj generate ediliyor...");
            for (int i = 0; i < 3; i++)
            {
                LocalChatLLM.Instance.GenerateNPCReaction();
            }
        }
    }

    public void DisplayText3D(string message)
    {
        if (npc3DText == null) return;
        
        // Eğer zaten text gösteriliyorsa, mevcut coroutine'i durdur
        if (isTextDisplaying)
        {
            StopAllCoroutines();
        }
        
        StartCoroutine(ShowText3D(message));
    }

    IEnumerator ShowText3D(string message)
    {
        isTextDisplaying = true;
        
        // Text'i göster
        npc3DText.gameObject.SetActive(true);
        npc3DText.text = message;
        
        Debug.Log("NPC 3D Text: " + message);
        
        // Belirtilen süre kadar bekle
        yield return new WaitForSeconds(textDisplayDuration);
        
        // Text'i gizle
        npc3DText.text = "";
        npc3DText.gameObject.SetActive(false);
        
        isTextDisplaying = false;
    }

    void CreateNPC3DText()
    {
        // Yeni bir 3D Text objesi oluştur
        GameObject textObj = new GameObject("NPC_3DText");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = new Vector3(0, 2f, 0); // NPC'nin üstünde
        
        // TextMeshPro component'i ekle
        npc3DText = textObj.AddComponent<TextMeshPro>();
        npc3DText.text = "";
        npc3DText.fontSize = 3f;
        npc3DText.color = Color.white;
        npc3DText.alignment = TextAlignmentOptions.Center;
        
        // Text'in oyuncuya bakmasını sağla
        textObj.transform.LookAt(Camera.main.transform);
        textObj.transform.Rotate(0, 180, 0);
        
        Debug.Log("NPC 3D Text component'i oluşturuldu");
    }

    // Public metodlar - dışarıdan çağrılabilir
    public void SetTextDisplayDuration(float duration)
    {
        textDisplayDuration = duration;
    }

    public void SetCollisionCooldown(float cooldown)
    {
        collisionCooldown = cooldown;
    }

    public void ShowCustomMessage(string message)
    {
        DisplayText3D(message);
    }

    // Debug ve test metodları
    public void TestLLMSystem()
    {
        Debug.Log("=== LLM Sistem Testi ===");
        Debug.Log($"LocalChatLLM.Instance null mu: {LocalChatLLM.Instance == null}");
        Debug.Log($"Yedek mesaj sayısı: {backupMessages.Count}");
        Debug.Log($"Toplam preload edilmiş mesaj: {totalPreloadedMessages}");
        Debug.Log($"Preload durumu: {(isPreloading ? "Devam ediyor" : "Tamamlandı")}");
        
        if (LocalChatLLM.Instance != null)
        {
            Debug.Log("LLM Instance mevcut, GenerateNPCReaction test ediliyor...");
            LocalChatLLM.Instance.GenerateNPCReaction();
            StartCoroutine(TestGetMessage());
        }
        else
        {
            Debug.LogError("LLM Instance null, sistem çalışmıyor!");
            InitializeLLMSystem();
        }
    }

    IEnumerator TestGetMessage()
    {
        yield return new WaitForSeconds(2f);
        string testMessage = GetLatestMessageFromLLM();
        Debug.Log($"Test mesajı alındı: '{testMessage}'");
        
        if (!string.IsNullOrEmpty(testMessage))
        {
            DisplayText3D("TEST: " + testMessage);
        }
        else
        {
            DisplayText3D("TEST FAILED");
        }
    }

    // Inspector'dan çağrılabilir test butonları için
    [ContextMenu("Test LLM System")]
    public void TestLLMFromInspector()
    {
        TestLLMSystem();
    }

    [ContextMenu("Force Initialize LLM")]
    public void ForceInitializeLLM()
    {
        InitializeLLMSystem();
    }

    [ContextMenu("Show Backup Message")]
    public void TestBackupMessage()
    {
        string backupMsg = GetBackupMessage();
        if (!string.IsNullOrEmpty(backupMsg))
        {
            DisplayText3D("BACKUP: " + backupMsg);
        }
        else
        {
            DisplayText3D("NO BACKUP");
        }
    }

    [ContextMenu("Force Preload Messages")]
    public void ForcePreloadMessages()
    {
        if (!isPreloading)
        {
            StartCoroutine(PreloadBackupMessages());
        }
        else
        {
            Debug.Log("Preload zaten devam ediyor...");
        }
    }

    [ContextMenu("Debug Backup System")]
    public void DebugBackupSystem()
    {
        Debug.Log("=== Yedek Mesaj Sistemi Debug ===");
        Debug.Log($"Yedek mesaj kuyruğu boyutu: {backupMessages.Count}");
        Debug.Log($"Toplam preload edilmiş: {totalPreloadedMessages}");
        Debug.Log($"Preload devam ediyor mu: {isPreloading}");
        Debug.Log($"Preload ayarları - Count: {backupMessageCount}, Delay: {preloadDelay}s");
        
        if (backupMessages.Count > 0)
        {
            Debug.Log("Mevcut yedek mesajlar:");
            string[] msgs = backupMessages.ToArray();
            for (int i = 0; i < msgs.Length && i < 3; i++)
            {
                Debug.Log($"  {i + 1}: {msgs[i]}");
            }
        }
    }
}
