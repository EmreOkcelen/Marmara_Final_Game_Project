using UnityEngine;

public class NPCChatTrigger : MonoBehaviour
{
    private float lastTriggerTime = -10f;
    private float triggerCooldown = 5f;  // 5 saniye bekleme süresi

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            float timeSinceLast = Time.time - lastTriggerTime;
            if (timeSinceLast >= triggerCooldown)
            {
                Debug.Log("NPC'ye çarpýldý!");
                lastTriggerTime = Time.time;

                if (LocalChatLLM.Instance != null)
                {
                    LocalChatLLM.Instance.GenerateNPCReaction();
                }
                else
                {
                    Debug.LogError("LocalChatLLM Instance null!");
                }
            }
            else
            {
                Debug.Log("Trigger cooldown aktif, bekleniyor.");
            }
        }
    }
}
