using UnityEngine;

public class NoteBookTask : MyTask
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

