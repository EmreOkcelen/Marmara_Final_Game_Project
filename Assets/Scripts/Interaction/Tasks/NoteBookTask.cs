using UnityEngine;

public class NoteBookTask : MyTask
{

    public override bool CheckCompletion()
    {
        if (isInteracted)
        {
            Debug.Log($"[{name}] g�rev tamamland�!");
        }
        return false; // G�rev tamamlanmad�

    }


}

