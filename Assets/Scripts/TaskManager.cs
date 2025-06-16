using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UIElements;

public class TaskManager : MonoBehaviour
{
    [Header("Task Ayarları")]
    public List<MyTask> allTasks = new List<MyTask>();
    public MyTask currentTask;

    [Header("Debug")]
    public bool showDebugInfo = true;

    // Events
    public static event System.Action<MyTask> OnTaskStarted;
    public static event System.Action<MyTask> OnTaskCompleted;
    public static event System.Action OnAllTasksCompleted;

    [SerializeField] TMP_Text currentTaskText;

    public static TaskManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // TaskManager'i sahneler arasında taşınabilir yapmak için
        }
        else
        {
            Destroy(gameObject); // Singleton, zaten var ise bu örneği yok et
        }

        OnAllTasksCompleted += () =>
        {
            Game1 game1 = FindFirstObjectByType<Game1>();
            if (game1 != null)
            {
                game1.StartGame1();
            }
            else
            {
                Debug.LogWarning("Game1 script bulunamadı, görev tamamlandığında yapılacak işlemler atlanıyor.");
            }
            // Burada tüm görevler tamamlandığında yapılacak işlemler
            }
            ;
    }

    private void Start()
    {
        InitializeTasks();
        StartNextTask();
    }

    private void Update()
    {
        // Mevcut görev tamamlandıysa otomatik olarak sıradakine geç
        if (currentTask != null && currentTask.IsCompleted)
        {
            Debug.Log($"Görev tamamlandı: {currentTask.name}, sıradakine geçiliyor...");
            currentTask = null; // Tamamlanan görevi temizle
            StartNextTask();
        }

        // Debug
        if (showDebugInfo && Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log($"Mevcut görev: {(currentTask ? currentTask.name : "Yok")}");
            Debug.Log($"Tamamlanan/Toplam: {GetCompletedTaskCount()}/{GetTotalTaskCount()}");
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("mevcut görev:" + (currentTask ? currentTask.name : "Yok"));
        }
        currentTaskText.text = currentTask.name + " ile etkileşimde bulun" + "(" + GetCompletedTaskCount() + "/" + (GetTotalTaskCount()-1)  + ")";
    }

    // Görevleri başlat
    private void InitializeTasks()
    {
        if (allTasks.Count == 0)
        {
            // Sahnedeki MyTask scriptlerini bul
            MyTask[] foundTasks = FindObjectsByType<MyTask>(FindObjectsSortMode.None);
            for (int i = 0; i < foundTasks.Length; i++)
            {
                foundTasks[i].priority = i + 1;
                allTasks.Add(foundTasks[i]);
            }
        }
        // Öncelik sırasına göre sırala
        allTasks = allTasks.OrderBy(t => t.priority).ToList();

    }

    // Sıradaki görevi başlat
    public void StartNextTask()
    {
        // Mevcut görevi deaktif et
        if (currentTask != null)
        {
            currentTask.SetActive(false);
        }

        // Tamamlanmamış ilk görevi bul
        currentTask = allTasks.FirstOrDefault(t => !t.IsCompleted);
        

        if (currentTask != null)
        {
            currentTask.SetActive(true);
            OnTaskStarted?.Invoke(currentTask);
            Debug.Log($"Yeni görev başlatıldı: {currentTask.name}");
        }
        else
        {
            Debug.Log("Tüm görevler tamamlandı!");
            OnAllTasksCompleted?.Invoke();
        }
    }

    // Mevcut görevi tamamla
    public void CompleteCurrentTask()
    {
        if (currentTask != null && !currentTask.IsCompleted)
        {
            currentTask.Complete();
            OnTaskCompleted?.Invoke(currentTask);
            Debug.Log($"Görev manuel tamamlandı: {currentTask.name}");
        }
    }

    // Görev sayıları
    public int GetCompletedTaskCount()
    {
        return allTasks.Count(t => t.IsCompleted);
    }

    public int GetTotalTaskCount()
    {
        return allTasks.Count;
    }

    // Mevcut görevi al
    public MyTask GetCurrentTask()
    {
        return currentTask;
    }

    // Sonraki görevi al
    public MyTask GetNextTask()
    {
        var currentIndex = allTasks.FindIndex(t => t == currentTask);
        if (currentIndex >= 0)
        {
            for (int i = currentIndex + 1; i < allTasks.Count; i++)
            {
                if (!allTasks[i].IsCompleted)
                    return allTasks[i];
            }
        }
        return null;
    }
}
