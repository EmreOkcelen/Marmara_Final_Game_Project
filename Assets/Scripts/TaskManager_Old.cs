using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class TaskManagerOld : MonoBehaviour
{
    [Header("Task Ayarları")]
    public List<MyTask> allTasks = new List<MyTask>();
    public List<MyTask> remainingTasks = new List<MyTask>(); // Henüz etkileşime girilmemiş görevler
    public MyTask currentTask;
    
    [Header("UI ve Debug")]
    public bool showDebugInfo = true;

    public static TaskManager Instance { get; private set; }
    
    // Events
    public static event Action<MyTask> OnTaskStarted;
    public static event Action<MyTask> OnTaskCompleted;
    public static event Action<MyTask> OnTaskProgressUpdated;
    public static event Action<MyTask> OnTaskInteractedEvent;
    public static event Action OnAllTasksCompleted;


    private void Start()
    {
        InitializeTasks();
        InitializeRemainingTasks();
        StartNextTask();
    }

    private void Update()
    {
        if (showDebugInfo)
        {
            DisplayTaskInfo();
        }

        CheckTaskCompletion();
        
        // Mevcut görev tamamlandıysa otomatik olarak sıradakine geç
        if (currentTask != null && currentTask.IsCompleted)
        {
            Debug.Log($"Görev otomatik geçiş: {currentTask.name} tamamlandı, sıradakine geçiliyor...");
            currentTask = null;
            StartNextTask();
        }
        
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log(allTasks);
            Debug.Log("şuanki görev: " + currentTask);
            Debug.Log("sonraki görev: " + GetNextTask());
        }
    }

    // Görevleri başlatma
    private void InitializeTasks()
    {
        if (allTasks.Count == 0)
        {
            // Sahne içindeki MyTask scriptlerini otomatik olarak bul ve ekle
            FindAndAddMyTasksInScene();
        }
        
        // Görevleri öncelik sırasına göre sırala
        allTasks = allTasks.OrderBy(t => t.priority).ToList();
    }
    
    // Yapılması gereken görevler listesini başlat
    private void InitializeRemainingTasks()
    {
        remainingTasks.Clear();
        remainingTasks.AddRange(allTasks);
        Debug.Log($"Toplam {remainingTasks.Count} görev yapılması gerekiyor.");
    }

    // Sahnedeki MyTask scriptlerini bulup ekleme
    private void FindAndAddMyTasksInScene()
    {
        MyTask[] myTasks = FindObjectsByType<MyTask>(FindObjectsSortMode.None);
        
        for (int i = 0; i < myTasks.Length; i++)
        {
            if (!allTasks.Contains(myTasks[i]))
            {
                myTasks[i].priority = i + 1; // Otomatik öncelik ata
                allTasks.Add(myTasks[i]);
                Debug.Log($"MyTask bulundu ve eklendi: {myTasks[i].name}");
            }
        }
    }

    // Örnek görevler ekleme - artık kullanılmıyor, MyTask scriptleri kullanıyor
    private void AddSampleTasks()
    {
        // Bu metod artık MyTask scriptleri kullandığımız için boş
        Debug.Log("Sahnedeki MyTask scriptleri kullanılıyor...");
    }

    // Yeni MyTask ekleme
    public void AddMyTask(MyTask myTask, int priority, MyTask.TaskType location = MyTask.TaskType.Home)
    {
        if (!allTasks.Contains(myTask))
        {
            myTask.priority = priority;
            myTask.taskLocation = location;
            allTasks.Add(myTask);
            
            // Yeniden sırala
            allTasks = allTasks.OrderBy(t => t.priority).ToList();
            
            Debug.Log($"Yeni MyTask eklendi: {myTask.name}");
        }
    }

    // Bir sonraki görevi başlatma
    public void StartNextTask()
    {


        Debug.Log($"StartNextTask çağrıldı. Toplam görev sayısı: {remainingTasks.Count}");
        
        // Tamamlanmamış ilk görevi bul
        currentTask = remainingTasks.FirstOrDefault(t => !t.IsCompleted);
        
        if (currentTask != null)
        {
            OnTaskStarted?.Invoke(currentTask);
            Debug.Log($"Yeni görev başlatıldı: {currentTask.name}");
        }
        else
        {
            Debug.Log("Tüm görevler tamamlandı!");
            OnAllTasksCompleted?.Invoke();
        }
    }
    
    // Görev ile etkileşime geçildiğinde çağrılır
    public void OnTaskInteracted(MyTask task)
    {
        if (task == currentTask && remainingTasks.Contains(task))
        {
            // Görev yapılması gereken listeden çıkar
            remainingTasks.Remove(task);
            Debug.Log($"Görev yapılması gereken listeden çıkarıldı: {task.name}");
            Debug.Log($"Kalan görev sayısı: {remainingTasks.Count}");
            
            OnTaskInteractedEvent?.Invoke(task);
            
            // Bir sonraki göreve geç
            StartNextTask();
        }
        else if (task != currentTask)
        {
            Debug.Log($"Bu görev şu anda sırada değil: {task.name}");
        }
    }

    // Görev ilerlemesini güncelleme


    // Mevcut görevi tamamlama
    public void CompleteCurrentTask()
    {
        if (currentTask != null && !currentTask.IsCompleted)
        {
            currentTask.Complete();
            
            OnTaskCompleted?.Invoke(currentTask);
            Debug.Log($"Görev tamamlandı: {currentTask.name}");
            
            // Tamamlanan görevi null yap, sonra yeni görev bul
            currentTask = null;
            StartNextTask();
        }
    }

    // Belirli bir görevi tamamlama
    public void CompleteTask(string taskName)
    {
        MyTask task = allTasks.FirstOrDefault(t => t.name == taskName);
        if (task != null && !task.IsCompleted)
        {
            if (task == currentTask)
            {
                CompleteCurrentTask();
            }
            else
            {
                task.Complete();
                OnTaskCompleted?.Invoke(task);
                Debug.Log($"Görev tamamlandı: {taskName}");
            }
        }
    }

    // Görev tamamlanma kontrolü
    private void CheckTaskCompletion()
    {
        if (allTasks.All(t => t.IsCompleted) && currentTask == null)
        {
            OnAllTasksCompleted?.Invoke();
            enabled = false;
        }
    }

    // Genel tamamlanma oranını hesaplama
    public float GetOverallCompletionPercentage()
    {
        if (allTasks.Count == 0) return 0f;
        
        float totalCompletion = allTasks.Sum(t => t.completionPercentage);
        return totalCompletion / allTasks.Count;
    }

    // Tamamlanan görev sayısını alma
    public int GetCompletedTaskCount()
    {
        return allTasks.Count(t => t.IsCompleted);
    }

    // Toplam görev sayısını alma
    public int GetTotalTaskCount()
    {
        return allTasks.Count;
    }
    
    // Kalan görev sayısını alma
    public int GetRemainingTaskCount()
    {
        return remainingTasks.Count;
    }
    
    // Yapılması gereken görevleri alma
    public List<MyTask> GetRemainingTasks()
    {
        return remainingTasks;
    }

    // Mevcut görevi alma
    public MyTask GetCurrentTask()
    {
        return currentTask;
    }

    // Bir sonraki görevi alma
    public MyTask GetNextTask()
    {
        // Mevcut görevden sonraki tamamlanmamış ilk görevi bul
        var currentIndex = allTasks.FindIndex(t => t == currentTask);
        if (currentIndex >= 0)
        {
            for (int i = currentIndex + 1; i < allTasks.Count; i++)
            {
                if (!allTasks[i].IsCompleted)
                {
                    return allTasks[i];
                }
            }
        }
        return null;
    }

    // Tüm görevleri alma
    public List<MyTask> GetAllTasks()
    {
        return allTasks;
    }

    // Belirli bir görevi alma
    public MyTask GetTask(string taskName)
    {
        return allTasks.FirstOrDefault(t => t.name == taskName);
    }

    // Debug bilgilerini gösterme
    private void DisplayTaskInfo()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("=== GÖREV BİLGİLERİ ===");
            Debug.Log($"Genel Tamamlanma: {GetOverallCompletionPercentage():F1}%");
            Debug.Log($"Tamamlanan Görevler: {GetCompletedTaskCount()}/{GetTotalTaskCount()}");
            Debug.Log($"Kalan Görevler: {GetRemainingTaskCount()}");
            
            if (currentTask != null)
            {
                Debug.Log($"Mevcut Görev: {currentTask.name} (%{currentTask.completionPercentage:F1})");
                Debug.Log($"Etkileşime Geçilebilir: {currentTask.CanInteract()}");
            }
            
            var nextTask = GetNextTask();
            if (nextTask != null)
            {
                Debug.Log($"Sonraki Görev: {nextTask.name}");
            }
        }
    }

    // İlerlemeli görev tamamlama (test için)
    private void OnGUI()
    {
        if (!showDebugInfo) return;
        
        // 1080x1920 ekran için ölçeklendirilmiş UI
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        
        // Responsive boyutlandırma
        float uiWidth = screenWidth * 0.25f; // Ekranın %25'i
        float uiHeight = screenHeight * 0.3f; // Ekranın %30'u
        float margin = screenWidth * 0.02f; // Ekranın %2'si margin
        
        GUILayout.BeginArea(new Rect(margin, margin, uiWidth, uiHeight));
        
        // Font boyutunu ekran boyutuna göre ayarla
        int fontSize = Mathf.RoundToInt(screenHeight * 0.02f);
        GUI.skin.label.fontSize = fontSize;
        GUI.skin.button.fontSize = fontSize;
        
        GUILayout.Label($"Genel İlerleme: {GetOverallCompletionPercentage():F1}%");
        GUILayout.Label($"Görevler: {GetCompletedTaskCount()}/{GetTotalTaskCount()}");
        GUILayout.Label($"Kalan: {GetRemainingTaskCount()}");
        
        if (currentTask != null)
        {
            GUILayout.Label($"Mevcut: {currentTask.name}");
            GUILayout.Label($"İlerleme: {currentTask.completionPercentage:F1}%");
            GUILayout.Label($"Etkileşim: {(currentTask.CanInteract() ? "Mümkün" : "Mümkün değil")}");

            
            if (GUILayout.Button("Görevi Tamamla", GUILayout.Height(screenHeight * 0.04f)))
            {
                CompleteCurrentTask();
            }
            
            if (GUILayout.Button("Etkileşime Geç", GUILayout.Height(screenHeight * 0.04f)) && currentTask.CanInteract())
            {
                currentTask.Interact();
            }
        }
        
        GUILayout.Label("T tuşu = Detaylı bilgi");
        GUILayout.EndArea();
    }
}