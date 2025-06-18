using System.Collections;
using TMPro;
using UnityEngine;

public class NPCChatTrigger : MonoBehaviour
{
    private float lastTriggerTime = -10f;
    private float triggerCooldown = 5f;

    public TextMeshPro textMeshPro; // Inspector'da atanacak
    private Coroutine hideTextCoroutine;

    private void Start()
    {
        if (textMeshPro != null)
        {
            textMeshPro.gameObject.SetActive(false); // Baþta kapalý olsun
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            float timeSinceLast = Time.time - lastTriggerTime;
            if (timeSinceLast >= triggerCooldown)
            {
                lastTriggerTime = Time.time;

                if (LocalChatLLM.Instance != null)
                {
                    LocalChatLLM.Instance.GenerateNPCReaction(DisplayDialogue);
                }
                else
                {
                    Debug.LogError("LocalChatLLM Instance null!");
                }
            }
        }
    }

    public void DisplayDialogue(string message)
    {
        if (textMeshPro != null)
        {
            textMeshPro.text = message;
            textMeshPro.gameObject.SetActive(true);

            if (hideTextCoroutine != null)
                StopCoroutine(hideTextCoroutine);

            hideTextCoroutine = StartCoroutine(HideTextAfterSeconds(5f));
        }
    }

    private IEnumerator HideTextAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        textMeshPro.gameObject.SetActive(false);
    }
}
