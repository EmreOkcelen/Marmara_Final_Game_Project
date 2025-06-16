using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [Header("Interaction")]
    public string interactionText = "Press E to interact";
    
    public virtual string GetInteractionText()
    {
        return interactionText;
    }
    
    public virtual void Interact()
    {
        // Override this method in derived classes
    }
}
