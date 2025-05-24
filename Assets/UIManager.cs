using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("Prompt UI")]
    [SerializeField] private TMP_Text promptText;

    [Header("Interaction Panel UI")]
    [SerializeField] private GameObject interactionPanel;
    [SerializeField] private TMP_Text panelText;

    private void Awake()
    {
        // Singleton setup
        if (instance == null) instance = this;
        else Destroy(gameObject);
        // Ensure UI is hidden at start
        interactionPanel.SetActive(false);
    }

    /// <summary>
    /// Shows the “Press E to interact” prompt with the given text.
    /// </summary>


    /// <summary>
    /// Opens the interaction panel and sets its content.
    /// </summary>
    public void ShowInteractionPanel(string content)
    {
        panelText.text = content;
        interactionPanel.SetActive(true);
    }

    /// <summary>
    /// Closes the interaction panel.
    /// </summary>
    public void HideInteractionPanel()
    {
        interactionPanel.SetActive(false);
    }

}