using UnityEngine;

public class InteractionTextManager : MonoBehaviour
{
    public InteractionUI interactionUI;
    public InteractableObject[] interactableObjects;

    private void Start()
    {
        foreach (var obj in interactableObjects)
        {
            string showEvent = $"ShowInteraction_{obj.gameObject.name}";
            string textEvent = $"ShowText_{obj.gameObject.name}";

            EventManager.Subscribe(showEvent, () => interactionUI.ShowInteractionPrompt(obj.gameObject.name));
            EventManager.Subscribe(textEvent, () => interactionUI.SetText(obj.interactionText));
        }
    }
}
