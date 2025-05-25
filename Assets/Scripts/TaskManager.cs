using UnityEngine;
using System.Linq;

public class TaskManager : MonoBehaviour
{
    public MyTask[] allTasks;

    private void Update()
    {
        if (allTasks.All(t => t.IsCompleted))
        {
            Debug.Log("Tüm görevler tamamlandı!");
            enabled = false; // bir kez bildirdikten sonra durdur
        }
    }
}