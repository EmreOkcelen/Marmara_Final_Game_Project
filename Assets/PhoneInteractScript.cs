using UnityEngine;
using TMPro;
using System.Linq;

public class PhoneInteractScript : MonoBehaviour
{
    [Header("Settings")]
    [TextArea] public string interactionText;
    public Vector3 boxSize = Vector3.one * 5f;
    public Vector3 promptOffset = Vector3.up * 2f;
    public GameObject promptObject;

    private TextMeshPro prompt3DText;
    private bool isInRange;
    private bool isPanelOpen;


    void Start()
    {
        if (promptObject != null)
        {
            prompt3DText = promptObject.GetComponent<TextMeshPro>();
            promptObject.SetActive(false);
        }

    }

    void Update()
    {
        UpdateRange();
        
        // Panel açıkken veya mevcut görevse etkileşim kontrolü yap
        if (isInRange)
            CheckForInteraction();
    }

    void LateUpdate()
    {
        if (isInRange && promptObject.activeSelf)
            UpdatePromptTransform();
    }

    // --- Range Detection & Prompt ---
    void UpdateRange()
    {
        bool playerNear = Physics.OverlapBox(transform.position, boxSize * 0.5f, transform.rotation)
                               .Any(c => c.CompareTag("Player"));

        if (playerNear && !isInRange)
            ShowPrompt();
        else if (!playerNear && isInRange)
            HidePrompt();

        isInRange = playerNear;
    }

    void ShowPrompt()
    {
        if (promptObject == null) return;
        prompt3DText.text = "Telefonu kapatmak için E'ye basın";
        promptObject.SetActive(true);
    }

    void HidePrompt()
    {
        if (promptObject != null)
            promptObject.SetActive(false);

        if (isPanelOpen)
            ClosePanel();
    }

    void UpdatePromptTransform()
    {
        Vector3 worldPos = transform.position + promptOffset;
        promptObject.transform.position = worldPos;
        promptObject.transform.rotation = Quaternion.LookRotation(worldPos - Camera.main.transform.position);
    }

    // --- Input Handling ---
    void CheckForInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Game1.Instance.FinishGame1();
            
        }
    }

    // --- Panel Control ---

    void ClosePanel()
    {
        isPanelOpen = false;
        UIManager.Instance.HideInteractionPanel();

    }

    // --- Editor Gizmo ---
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        Gizmos.DrawWireCube(Vector3.zero, boxSize);
        Gizmos.matrix = Matrix4x4.identity;
    }
}