using UnityEngine;

public class BlackScreenState : MonoBehaviour
{
    public static BlackScreenState Instance;

    public int currentDialogIndex = 0;
    public int currentSceneIndex = 0;

    public string[] sceneNames = { "BedRoom", "Home", "BathRoom" };
    public BlackScreenManager.DialogVersion[] dialogVersions = {
        BlackScreenManager.DialogVersion.LinesV1,
        BlackScreenManager.DialogVersion.LinesV2
    };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public string GetCurrentScene()
    {
        if (currentSceneIndex >= 0 && currentSceneIndex < sceneNames.Length)
            return sceneNames[currentSceneIndex];
        return null;
    }

    public BlackScreenManager.DialogVersion GetCurrentDialogVersion()
    {
        if (currentDialogIndex >= 0 && currentDialogIndex < dialogVersions.Length)
            return dialogVersions[currentDialogIndex];
        return BlackScreenManager.DialogVersion.LinesV1;
    }

    public void AdvanceToNext()
    {
        currentDialogIndex++;
        currentSceneIndex++;
    }
}
