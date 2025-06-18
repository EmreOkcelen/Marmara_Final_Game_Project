using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System;

/// <summary>
/// Basit async Scene Manager - Sahne geçişlerini async olarak yönetir
/// </summary>
public class SimpleSceneManager : MonoBehaviour
{
    private static SimpleSceneManager _instance;
    public static SimpleSceneManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("SimpleSceneManager");
                _instance = go.AddComponent<SimpleSceneManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    [Header("Loading Settings")]
    [SerializeField] private bool showLoadingScreen = true;
    [SerializeField] private string loadingSceneName = "Loading";

    // Events
    public static event Action<string> OnSceneLoadStarted;
    public static event Action<string, float> OnSceneLoadProgress;
    public static event Action<string> OnSceneLoadCompleted;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Sahneyi async olarak yükler
    /// </summary>
    /// <param name="sceneName">Yüklenecek sahne adı</param>
    /// <returns>Task</returns>
    public async Task LoadSceneAsync(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Sahne adı boş olamaz!");
            return;
        }

        Debug.Log($"Sahne yükleniyor: {sceneName}");
        OnSceneLoadStarted?.Invoke(sceneName);

        try
        {
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
            asyncOperation.allowSceneActivation = false;

            // Yükleme ilerlemesini takip et
            while (!asyncOperation.isDone)
            {
                float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);
                OnSceneLoadProgress?.Invoke(sceneName, progress);

                // %90'a ulaştığında sahneyi aktif et
                if (asyncOperation.progress >= 0.9f)
                {
                    asyncOperation.allowSceneActivation = true;
                }

                await Task.Yield();
            }

            Debug.Log($"Sahne yüklendi: {sceneName}");
            OnSceneLoadCompleted?.Invoke(sceneName);
        }
        catch (Exception e)
        {
            Debug.LogError($"Sahne yüklenirken hata oluştu: {e.Message}");
        }
    }

    /// <summary>
    /// Loading screen ile sahne yükler
    /// </summary>
    /// <param name="sceneName">Yüklenecek sahne adı</param>
    /// <returns>Task</returns>
    public async Task LoadSceneWithLoadingAsync(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Sahne adı boş olamaz!");
            return;
        }

        if (showLoadingScreen && !string.IsNullOrEmpty(loadingSceneName))
        {
            // Önce loading sahnesini yükle
            await LoadSceneAsync(loadingSceneName);
            
            // Kısa bir bekleme
            await Task.Delay(500);
            
            // Sonra hedef sahneyi yükle
            await LoadSceneAsync(sceneName);
        }
        else
        {
            // Direkt sahneyi yükle
            await LoadSceneAsync(sceneName);
        }
    }

    /// <summary>
    /// Mevcut sahneyi yeniden yükler
    /// </summary>
    /// <returns>Task</returns>
    public async Task ReloadCurrentSceneAsync()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        await LoadSceneAsync(currentSceneName);
    }

    /// <summary>
    /// Sahneyi additive olarak yükler (mevcut sahneye ekler)
    /// </summary>
    /// <param name="sceneName">Yüklenecek sahne adı</param>
    /// <returns>Task</returns>
    public async Task LoadSceneAdditiveAsync(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Sahne adı boş olamaz!");
            return;
        }

        Debug.Log($"Sahne additive yükleniyor: {sceneName}");
        OnSceneLoadStarted?.Invoke(sceneName);

        try
        {
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            while (!asyncOperation.isDone)
            {
                float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);
                OnSceneLoadProgress?.Invoke(sceneName, progress);
                await Task.Yield();
            }

            Debug.Log($"Sahne additive yüklendi: {sceneName}");
            OnSceneLoadCompleted?.Invoke(sceneName);
        }
        catch (Exception e)
        {
            Debug.LogError($"Additive sahne yüklenirken hata oluştu: {e.Message}");
        }
    }

    /// <summary>
    /// Sahneyi unload eder
    /// </summary>
    /// <param name="sceneName">Unload edilecek sahne adı</param>
    /// <returns>Task</returns>
    public async Task UnloadSceneAsync(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Sahne adı boş olamaz!");
            return;
        }

        Debug.Log($"Sahne unload ediliyor: {sceneName}");

        try
        {
            AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync(sceneName);

            while (!asyncOperation.isDone)
            {
                await Task.Yield();
            }

            Debug.Log($"Sahne unload edildi: {sceneName}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Sahne unload edilirken hata oluştu: {e.Message}");
        }
    }

    /// <summary>
    /// Belirli bir süre bekleyip sahne yükler
    /// </summary>
    /// <param name="sceneName">Yüklenecek sahne adı</param>
    /// <param name="delayInSeconds">Bekleme süresi (saniye)</param>
    /// <returns>Task</returns>
    public async Task LoadSceneWithDelayAsync(string sceneName, float delayInSeconds)
    {
        await Task.Delay((int)(delayInSeconds * 1000));
        await LoadSceneAsync(sceneName);
    }

    // Static metodlar - kolay kullanım için
    public static async Task LoadScene(string sceneName)
    {
        await Instance.LoadSceneAsync(sceneName);
    }

    public static async Task LoadSceneWithLoading(string sceneName)
    {
        await Instance.LoadSceneWithLoadingAsync(sceneName);
    }

    public static async Task ReloadCurrentScene()
    {
        await Instance.ReloadCurrentSceneAsync();
    }

    public static async Task LoadSceneAdditive(string sceneName)
    {
        await Instance.LoadSceneAdditiveAsync(sceneName);
    }

    public static async Task UnloadScene(string sceneName)
    {
        await Instance.UnloadSceneAsync(sceneName);
    }

    public static async Task LoadSceneWithDelay(string sceneName, float delayInSeconds)
    {
        await Instance.LoadSceneWithDelayAsync(sceneName, delayInSeconds);
    }
}
