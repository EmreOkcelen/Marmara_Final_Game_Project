using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public static string TargetSceneName;

    public static void LoadSceneWithLoading(string sceneName)
    {
        TargetSceneName = sceneName;
        SceneManager.LoadSceneAsync("Loading");
    }
}
