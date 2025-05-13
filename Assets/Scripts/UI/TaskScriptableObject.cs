using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// "Task" adında bir ScriptableObject: bir görev tanımı (description) içerir,
/// tamamlanma durumunu takip eder ve tamamlandığında abone olunan Event'i tetikler.
/// </summary>
[CreateAssetMenu(fileName = "NewTask", menuName = "Tasks/Task")]
public class MyTask : ScriptableObject
{
    [TextArea]
    [Tooltip("Bu görev için açıklama metni.")]
    public string description;

    [Tooltip("Görev tamamlandığında true olur.")]
    public bool isCompleted;

    [Tooltip("Görev tamamlandığında tetiklenecek olaylar.")]
    public UnityEvent onCompleted;

    public TMP_Text taskText; // Görev metni UI bileşeni

    /// <summary>
    /// Görevi tamamlanmış olarak işaretler ve onCompleted event'ini çağırır.
    /// </summary>
    /// 

    public void Start()
    {
        
    }
    public void Initiate()
    {
        if (taskText != null)
        {
            Debug.LogWarning("Görev metni bileşeni atanmadı.");
            taskText.text = description; // Görev açıklamasını UI'da göster
        }
    }
    public void Complete()
    {
        if (!isCompleted)
        {
            isCompleted = true;
            onCompleted?.Invoke();
        }
    }

    /// <summary>
    /// Görev durumunu sıfırlar, isCompleted = false olur.
    /// </summary>
    public void ResetTask()
    {
        isCompleted = false;
    }
}
