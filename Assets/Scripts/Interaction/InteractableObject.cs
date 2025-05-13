using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public string interactionText; // Objeye özel metin
    private bool isPlayerInRange = false; // Oyuncu objeye yaklaþtý mý?

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;  // Oyuncu objeye girdi
            EventManager.Trigger($"ShowInteraction_{gameObject.name}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false; // Oyuncu objeden çýktý
            EventManager.Trigger("HideInteraction");
        }
    }

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            EventManager.Trigger("ToggleUI");
            EventManager.Trigger($"ShowText_{gameObject.name}");
        }
    }
}
