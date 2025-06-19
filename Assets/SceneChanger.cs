using UnityEngine;
using UnityEngine.SceneManagement;
using static BlackScreenManager;

public class SceneChanger : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public BlackScreenManager.DialogVersion dialogVersion;

    public string nextSceneName;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (dialogVersion == BlackScreenManager.DialogVersion.None)
            {
                SceneLoader.LoadSceneWithLoading(nextSceneName);
            }
            else
            {
                BlackScreenManager.mySceneNext = nextSceneName; // Geçilecek sahne adını ayarla
                BlackScreenState.Instance.SetState(dialogVersion, nextSceneName); // Geçiş durumu belirle
                SceneManager.LoadScene("BlackScreen"); // BlackScreen sahnesine geç
            }
        }
    }
    public static void LoadSceneWithOptionalBlackScreen(string nextScene, BlackScreenManager.DialogVersion version)
    {
        if (version == BlackScreenManager.DialogVersion.None)
        {
            SceneLoader.LoadSceneWithLoading(nextScene);
        }
        else
        {
            BlackScreenManager.mySceneNext = nextScene;
            BlackScreenState.Instance.SetState(version, nextScene);
            SceneManager.LoadScene("BlackScreen");
        }
    }
    public void OnPlayButtonPressed()
    {
        // Örneğin Bedroom sahnesine BlackScreen ile geçiş
        SceneChanger.LoadSceneWithOptionalBlackScreen("Bedroom", BlackScreenManager.DialogVersion.ilkSahne);
    }


}
