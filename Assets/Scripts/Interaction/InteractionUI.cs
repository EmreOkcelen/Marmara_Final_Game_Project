using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionUI : MonoBehaviour
{
    public GameObject interactionPrompt; // "E) Interact with [name]" UI
    public TextMeshProUGUI promptText; // UI i�indeki metin
    public GameObject interactionPanel; // A��lacak pencere
    public TextMeshProUGUI interactionText; // Panel i�indeki a��klamalar

    private bool isPanelOpen = false;

    private void OnEnable()
    {
        EventManager.Subscribe("HideInteraction", HideInteractionPrompt);
        EventManager.Subscribe("ToggleUI", TogglePanel);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe("HideInteraction", HideInteractionPrompt);
        EventManager.Unsubscribe("ToggleUI", TogglePanel);
    }

    public void ShowInteractionPrompt(string objectName)
    {
        promptText.text = $"E) Interact with {objectName}";
        interactionPrompt.SetActive(true);
    }

    private void HideInteractionPrompt()
    {
        interactionPrompt.SetActive(false);
    }

    private void TogglePanel()
    {
        isPanelOpen = !isPanelOpen;
        interactionPanel.SetActive(isPanelOpen);
        HideInteractionPrompt();

        // UI a��ld���nda hareketi kilitle, kapand���nda a�
        EventManager.Trigger(isPanelOpen ? "LockPlayerMovement" : "UnlockPlayerMovement");
    }

    public void SetText(string text)
    {
        interactionText.text = text;
    }
}
