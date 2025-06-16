using UnityEngine;

public class LambTask : MyTask
{
    public override bool CheckCompletion()
    {
        if (isInteracted)
        {
            Debug.Log($"[{name}] görev tamamlandı!");
            return true; // Görev tamamlandı
        }
        return false; // Görev tamamlanmadı
    }
}
