using UnityEngine;
using UnityEngine.UI;

public class PlayerLookInteraction : MonoBehaviour
{
    [Header("Raycast Settings")]
    public float interactionRange = 5f;
    public LayerMask interactableLayer = -1;
    
    [Header("UI")]
    public Text interactionText;
    public GameObject interactionUI;
    
    private Camera playerCamera;
    private InteractableObject currentLookedObject;
    
    void Start()
    {
        playerCamera = Camera.main;
        if (interactionUI != null)
            interactionUI.SetActive(false);
    }
    
    void Update()
    {
        CheckForInteractableObject();
    }
    
    void CheckForInteractableObject()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, interactionRange, interactableLayer))
        {
            InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();
            
            if (interactable != null && interactable != currentLookedObject)
            {
                currentLookedObject = interactable;
                ShowInteractionText(interactable.GetInteractionText());
            }
        }
        else
        {
            if (currentLookedObject != null)
            {
                currentLookedObject = null;
                HideInteractionText();
            }
        }
    }
    
    void ShowInteractionText(string text)
    {
        if (interactionText != null)
            interactionText.text = text;
        
        if (interactionUI != null)
            interactionUI.SetActive(true);
    }
    
    void HideInteractionText()
    {
        if (interactionUI != null)
            interactionUI.SetActive(false);
    }
}
