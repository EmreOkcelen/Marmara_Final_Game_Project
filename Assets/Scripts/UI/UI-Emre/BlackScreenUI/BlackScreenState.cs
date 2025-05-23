using UnityEngine;
using System.Collections.Generic;

public class BlackScreenState : MonoBehaviour
{
    public static BlackScreenState Instance { get; private set; }

    private int currentIndex = 0;

    public List<BlackScreenManager.DialogVersion> dialogVersions = new List<BlackScreenManager.DialogVersion>();
    public List<string> sceneNames = new List<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public BlackScreenManager.DialogVersion GetCurrentDialogVersion()
    {
        return (currentIndex < dialogVersions.Count) ? dialogVersions[currentIndex] : BlackScreenManager.DialogVersion.LinesV1;
    }

    public string GetCurrentScene()
    {
        return (currentIndex < sceneNames.Count) ? sceneNames[currentIndex] : "";
    }

    public void AdvanceToNext()
    {
        currentIndex++;
    }

    public void ResetState()
    {
        currentIndex = 0;
    }
}
