using UnityEngine;

public class ChairTask : MyTask
{
    private bool isTouched = false;
    public override bool CheckCompletion()
    {
        if (isTouched)
        {
            Debug.Log("Chair task completed.");
            return true; // Görev tamamlandı
        }
        return false; // Görev tamamlanmadı
        
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isTouched = true;
            Debug.Log("Player touched the chair.");
        }
    }
}
