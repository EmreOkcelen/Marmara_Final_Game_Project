using UnityEngine;

public class ChairTask : MyTask
{

    public override bool CheckCompletion()
    {
        if (isInteracted)
        {
            Debug.Log($"[{name}] görev tamamlandı!");
        }
        return false; // Görev tamamlanmadı

    }


}
