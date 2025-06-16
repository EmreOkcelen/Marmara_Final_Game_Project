using UnityEngine;
using UnityEngine.Events;
using TMPro;

public abstract class MyTask : MonoBehaviour
{
    [Header("Tanım")]
    [TextArea] public string description;
    
    [Header("Görev Ayarları")]
    public int priority = 1; // Görev önceliği (1 = en yüksek)
    public float completionPercentage = 0f; // 0-100 arası tamamlanma yüzdesi
    public bool isActive = false; // Şu anda aktif olup olmadığı
    public TaskType taskLocation = TaskType.Home; // Görevin hangi lokasyonda olduğu

    [Header("UI")]
    public TMP_Text taskText;

    [Header("Olaylar")]
    public UnityEvent onCompleted;

    public bool IsCompleted;
    public bool isInteracted;
    public bool canInteract = false; // Şu anda etkileşime girebilir mi
    public enum TaskType
    {
        Home,
        Subway,
        Office,
    }
    public virtual void Start()
    {
        if (taskText != null)
            taskText.text = description;
        else
            Debug.LogWarning($"[{name}] taskText atanmamış.");
        
        onCompleted.AddListener(() => { Debug.Log($"[{name}] görev tamamlandı!"); });
    }

    public virtual void Update()
    {
        if (!IsCompleted && CheckCompletion())
            Complete();

    }


    public abstract bool CheckCompletion();


    public virtual void Complete()
    {
        IsCompleted = true;// Görev tamamlandığında etkileşim bir kez gerçekleşti olarak işaretlenir
        completionPercentage = 100f;
        isActive = false;
        onCompleted?.Invoke();
        Debug.Log($"Görev tamamlandı: {name}");
    }

    // İlerleme güncelleme metodu
    public virtual void UpdateProgress(float progress)
    {
        completionPercentage = Mathf.Clamp(progress, 0f, 100f);
        
        if (completionPercentage >= 100f && !IsCompleted)
        {
            Complete();
        }
    }
    
    // Görevi aktif yapma
    public virtual void SetActive(bool active)
    {
        isActive = active;
        canInteract = active; // Sadece aktif görevlerle etkileşim kurulabilir
        
        if (isActive)
        {
            Debug.Log($"Görev aktif: {name}");
        }
        else
        {
            Debug.Log($"Görev pasif: {name}");
        }
    }
    
    // Etkileşim kontrolü
    public virtual bool CanInteract()
    {
        return canInteract && !IsCompleted;
    }
    
    // Etkileşime girme
    public virtual void Interact()
    {
        if (!CanInteract())
        {
            Debug.Log($"Bu görev ile şu anda etkileşime geçemezsiniz: {name}");
            return;
        }
        
        isInteracted = true;
        Debug.Log($"Görev ile etkileşime geçildi: {name}");
        
        // Görevi tamamla
        Complete();
    }

    public virtual void ResetTask()
    {
        IsCompleted = false;
        completionPercentage = 0f;
        isActive = false;
        if (taskText != null)
            taskText.text = description;
    }
    public void Initiate()
    {
        ResetTask();
        Start();
    }
}