using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

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
            BlackScreenManager.mySceneNext = nextSceneName; // Geçilecek sahne adını ayarla
            SceneManager.LoadScene("BlackScreen"); // BlackScreen sahnesine geç


        }
    }
}
