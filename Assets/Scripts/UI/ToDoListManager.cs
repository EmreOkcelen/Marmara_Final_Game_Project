using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Yönetici sınıf: Sağ üst köşede görünen To-Do List (Yapılacaklar Listesi) oluşturur,
/// inspector üzerinden başlangıç görevleri ayarlanabilir ve oyun sırasında yalnızca ekleme işlevini sağlar.
/// Görev silme butonu veya kullanıcı etkileşimi yoktur; temizleme yalnızca koddan ClearAll() ile yapılabilir.
/// </summary>
public class ToDoListManager : MonoBehaviour
{
    public static ToDoListManager Instance;

    [Header("Prefab & UI Container")]
    [Tooltip("Görev öğesi prefab'ı (içinde Text barındıran)")]
    public GameObject taskItemPrefab;
    [Tooltip("Görevlerin listeleneceği VerticalLayoutGroup içeren transform")]
    public Transform tasksParent;

    [Header("Başlangıç Görevleri (Inspector)")]
    [Tooltip("Oyun başında otomatik eklenecek görevler")]
    public List<MyTask> initialTasks = new List<MyTask>();
    public List<MyTask> doneTasks = new List<MyTask>();


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // Inspector'da tanımlanan başlangıç görevlerini ekle
        for (int i = initialTasks.Count - 1; i >= 0; i--)
        {
            MyTask t = initialTasks[i];
            if (t != null && !string.IsNullOrWhiteSpace(t.description))
                AddTask(t);
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Escape tuşuna basıldığında tüm görevleri temizle
            TaskDone(initialTasks[0]);
        }
    }

    /// <summary>
    /// Yeni bir görev ekler.
    /// </summary>
    public void AddTask(MyTask tempTask)
    {
        if (taskItemPrefab == null || tasksParent == null)
        {
            Debug.LogWarning("TaskItem prefab veya tasksParent atanmamış.");
            return;
        }

        // Prefab'dan yeni öğe oluştur
        GameObject task = Instantiate(taskItemPrefab, tasksParent);
        if (task == null)
        {
            Debug.LogWarning("Prefab üzerinde TaskItem bileşeni bulunamadı.");
            return;
        }
        tempTask.taskText = task.GetComponentInChildren<TMP_Text>();
        tempTask.Initiate();
         
    }

    /// <summary>
    /// Tüm görevleri temizler. Kod tarafından çağrılabilir.
    /// </summary>
    public void ClearAll()
    {
        foreach (var t in initialTasks)
            Destroy(t);
        initialTasks.Clear();
    }
    public void TaskDone(MyTask task)
    {
        if (task == null)
        {
            Debug.LogWarning("Görev öğesi null.");
            return;
        }

        // Görev tamamlandı olarak işaretle
        task.taskText.alpha = 0.5f; // Görev metnini soluklaştır
    }

}



