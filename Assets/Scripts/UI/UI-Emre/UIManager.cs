using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Main Panels")]
    public GameObject mainMenu;
    public GameObject settingsMenu;
    public GameObject pauseMenu;
    public GameObject inGameUI;

    [SerializeField] private string[] gameplayScenes;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "MainMenu")
        {
            ShowMainMenu();
        }
        else
        {
            mainMenu.SetActive(false); // Sahne MainMenu de�ilse UI'yi kapat
        }
    }

    void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (!System.Array.Exists(gameplayScenes, scene => scene == currentScene))
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!pauseMenu.activeSelf)
                PauseGame();
            else
                ResumeGame();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (settingsMenu.activeSelf)
                CloseSettings();
            else
                OpenSettings();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            MainMenuSceneLoader();
        }
    }



    public void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        settingsMenu.SetActive(false);
        pauseMenu.SetActive(false);
        inGameUI.SetActive(false);
        Time.timeScale = 0f;
    }

    public void MainMenuSceneLoader()
    {
        SceneLoader.LoadSceneWithLoading("BlackScreen");
    }

    public void StartGame()
    {
        mainMenu.SetActive(false);
        settingsMenu.SetActive(false);
        pauseMenu.SetActive(false);
        inGameUI.SetActive(true);
        Time.timeScale = 1f;
        SceneLoader.LoadSceneWithLoading("BedRoom");
    }

    public void OpenSettings()
    {
        settingsMenu.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsMenu.SetActive(false);
    }

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    public void ShowInteractionPanel(string interactionText)
    {
        if (inGameUI == null)
        {
            Debug.LogError("In-Game UI GameObject is not assigned in the UIManager.");
            return;
        }

        // Burada etkileşim panelini açmak için gerekli kodları ekleyebilirsiniz
        // Örneğin, bir metin bileşeni varsa, metni ayarlayabilirsiniz
        // interactionTextComponent.text = interactionText;

        inGameUI.SetActive(true);
        inGameUI.GetComponentInChildren<TMP_Text>().text = interactionText;
    }

    public void HideInteractionPanel()
    {
        inGameUI.SetActive(false);
        // Burada etkileşim panelini kapatmak için gerekli kodları ekleyebilirsiniz
    }
}
