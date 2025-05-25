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

    public bool IsCompleted;
    public enum TaskType
    {
        Home,
        Subway,
        Office,
    }
    protected virtual void Start()
    {
        if (taskText != null)
            taskText.text = description;
        else
            Debug.LogWarning($"[{name}] taskText atanmamış.");
    }

    protected virtual void Update()
    {
        if (!IsCompleted && CheckCompletion())
            Complete();
    }


    public abstract bool CheckCompletion();


    public virtual void Complete()
    {
        IsCompleted = true;
        onCompleted?.Invoke();
        Debug.Log($"Görev tamamlandı: {name}");
    }

    public virtual void ResetTask()
    {
        IsCompleted = false;
        if (taskText != null)
            taskText.text = description;
    }
    public void Initiate()
    {
        ResetTask();
        Start();
    }
}