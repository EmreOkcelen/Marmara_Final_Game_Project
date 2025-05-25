using UnityEngine;
using UnityEngine.Events;
using TMPro;

public abstract class MyTask : MonoBehaviour
{
    [Header("Tanım")]
    [TextArea] public string description;

    [Header("UI")]
    public TMP_Text taskText;

    [Header("Olaylar")]
    public UnityEvent onCompleted;

    /// <summary>
    /// Görevin tamamlanma durumu.
    /// </summary>
    public bool IsCompleted;

    protected virtual void Start()
    {
        // Görev açıklamasını UI'a bas
        if (taskText != null)
            taskText.text = description;
        else
            Debug.LogWarning($"[{name}] taskText atanmamış.");
    }

    protected virtual void Update()
    {
        // Eğer henüz tamamlanmadıysa, her frame kontrol et
        if (!IsCompleted && CheckCompletion())
            Complete();
    }

    /// <summary>
    /// Görevin tamamlandığını tespit eden metot. Her alt sınıf burada kendi koşulunu yazar.
    /// </summary>
    protected abstract bool CheckCompletion();

    /// <summary>
    /// Tamamlama işlemleri: durumu işaretle, event tetikle, gerekirse UI güncelle.
    /// </summary>
    protected virtual void Complete()
    {
        IsCompleted = true;
        onCompleted?.Invoke();
        Debug.Log($"Görev tamamlandı: {name}");
    }

    /// <summary>
    /// Görevi sıfırlar (örneğin yeniden başlatma gerektiğinde).
    /// </summary>
    public virtual void ResetTask()
    {
        IsCompleted = false;
        if (taskText != null)
            taskText.text = description;
    }
    public void Initiate()
    {
        // Başlangıçta görevi başlat
        ResetTask();
        Start();
    }
}