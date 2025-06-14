using UnityEngine;

public class LambTask : MyTask
{

    public override bool CheckCompletion()
    {
        if (isInteracted)
        {
            Debug.Log($"[{name}] görev tamamlandý!");
        }
        return false; // Görev tamamlanmadý

    }


}
