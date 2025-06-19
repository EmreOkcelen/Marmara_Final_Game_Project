using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UIElements;
using System;
using UnityEngine.XR;
using Unity.VisualScripting;

public class TaskManager : MonoBehaviour
{
    [Header("Task Ayarları")]
    public List<MyTask> allTasks = new List<MyTask>();
    public MyTask currentTask;

    private InputDevice leftController;
    private InputDevice rightController;

    [Header("Debug")]
    public bool showDebugInfo = true;

    // Events
    public static event System.Action<MyTask> OnTaskStarted;
    public static event System.Action<MyTask> OnTaskCompleted;
    public static event System.Action OnAllTasksCompleted;

    public bool IsAllTasksCompleted = false;

    [SerializeField] TMP_Text currentTaskText;

    public static TaskManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;// TaskManager'i sahneler arasında taşınabilir yapmak için
        }
        else
        {
            Destroy(gameObject); // Singleton, zaten var ise bu örneği yok et
        }

        OnAllTasksCompleted += () =>
        {
            IsAllTasksCompleted = true;
            Game2 game2 = FindFirstObjectByType<Game2>();
            if (game2 != null)
            {
                UIManager.Instance.gorevText.text = "Kendine gelmen lazım mutfakta kahve yap";

            }
            else
            {
                Debug.LogWarning("game2 script bulunamadı, görev tamamlandığında yapılacak işlemler atlanıyor.");
            }
            // Burada tüm görevler tamamlandığında yapılacak işlemler
        }
            ;
        InputDevices.deviceConnected += OnDeviceConnected;
        InitializeOpenXRControllers();
    }

    private void OnDestroy()
    {
        InputDevices.deviceConnected -= OnDeviceConnected;
    }

    private void OnDeviceConnected(InputDevice device)
    {
        if ((device.characteristics & InputDeviceCharacteristics.Left) != 0 &&
            (device.characteristics & InputDeviceCharacteristics.Controller) != 0)
        {
            leftController = device;
        }
        if ((device.characteristics & InputDeviceCharacteristics.Right) != 0 &&
            (device.characteristics & InputDeviceCharacteristics.Controller) != 0)
        {
            rightController = device;
        }
    }

    private void InitializeOpenXRControllers()
    {
        var allDevices = new List<InputDevice>();
        InputDevices.GetDevices(allDevices);

        foreach (var d in allDevices)
        {
            if ((d.characteristics & InputDeviceCharacteristics.Left) != 0 &&
                (d.characteristics & InputDeviceCharacteristics.Controller) != 0)
            {
                leftController = d;
            }
            if ((d.characteristics & InputDeviceCharacteristics.Right) != 0 &&
                (d.characteristics & InputDeviceCharacteristics.Controller) != 0)
            {
                rightController = d;
            }
        }
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


        if (GetCompletedTaskCount() == (GetTotalTaskCount()-1))
        {
            IsAllTasksCompleted = true;
            Debug.Log("Tüm görevler tamamlandı!");
            OnAllTasksCompleted?.Invoke();
        }
        else
        {
              currentTaskText.text = currentTask.name + " ile etkileşimde bulun" + "(" + GetCompletedTaskCount() + "/" + (GetTotalTaskCount() - 1) + ")";
   
            if (Input.GetKeyDown(KeyCode.R))
            {

                CompleteCurrentTask();
            }
        }
       }
    // Tüm görevleri tamamla (debug amaçlı)
    public void CompleteAllTasks()
    {
        Debug.Log("Tüm görevler manuel olarak tamamlanıyor...");

        foreach (MyTask task in allTasks)
        {
            if (!task.IsCompleted)
            {
                task.Complete();
                OnTaskCompleted?.Invoke(task);
                Debug.Log($"Görev tamamlandı: {task.name}");
            }
        }

        currentTask = null;
        Debug.Log("Tüm görevler tamamlandı!");
        OnAllTasksCompleted?.Invoke();
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
