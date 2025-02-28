using System;
using UnityEngine;

public class NPCController : MonoBehaviour
{
    [SerializeField] private string message = "Hello, Player!"; // Inspector'dan ayarlanabilir mesaj
    [SerializeField] private GameObject textUI; // Mesaj� g�stermek i�in bir UI eleman�

    private void Start()
    {
        // Ba�lang��ta UI'yi gizle
        if (textUI != null)
        {
            textUI.SetActive(false);
        }

        // Event'e abone ol
        EventManager.Subscribe("ShowNPCMessage", ShowMessage);
        EventManager.Subscribe("HideNPCMessage", HideMessage);
    }

    private void OnDestroy()
    {
        // Abonelikten ��k
        EventManager.Unsubscribe("ShowNPCMessage", ShowMessage);
        EventManager.Unsubscribe("HideNPCMessage", HideMessage);
    }

    private void OnTriggerEnter(Collider other)
    {
        // E�er Player collider'a girerse
        if (other.CompareTag("Player"))
        {
            EventManager.Trigger("ShowNPCMessage");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // E�er Player collider'dan ��karsa
        if (other.CompareTag("Player"))
        {
            EventManager.Trigger("HideNPCMessage");
        }
    }

    private void ShowMessage()
    {
        if (textUI != null)
        {
            textUI.SetActive(true);
            var uiText = textUI.GetComponent<UnityEngine.UI.Text>();
            if (uiText != null)
            {
                uiText.text = message;
            }
        }
    }

    private void HideMessage()
    {
        if (textUI != null)
        {
            textUI.SetActive(false);
        }
    }
}
