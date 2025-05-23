using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class LoadingScreenController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI factText;
    [SerializeField] private Slider progressBar;
    [SerializeField] private float typingSpeed = 0.007f;
    [SerializeField] private AudioSource typeSound;

    private void Start()
    {
        string selectedFact = GetRandomFact();
        StartCoroutine(ShowFactThenLoadScene(selectedFact));
    }

    string GetRandomFact()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("addictionFacts");

        if (jsonFile != null)
        {
            GameTextData data = JsonUtility.FromJson<GameTextData>(jsonFile.text);
            if (data.facts != null && data.facts.Count > 0)
            {
                int randomIndex = Random.Range(0, data.facts.Count);
                return data.facts[randomIndex];
            }
            else
            {
                return "Þu anda istatistik yüklenemiyor.";
            }
        }
        else
        {
            return "addictionFacts.json bulunamadý!";
        }
    }

    IEnumerator LoadTargetSceneAsync()
    {
        float loadDuration = 8f; // toplam bekleme süresi
        float elapsed = 0f;

        while (elapsed < loadDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / loadDuration);
            progressBar.value = progress;
            yield return null;
        }

        AsyncOperation operation = SceneManager.LoadSceneAsync(SceneLoader.TargetSceneName);
        yield return operation;
    }
    IEnumerator TypeFactText(string fact)
    {
        factText.text = "";
        foreach (char c in fact)
        {
            factText.text += c;

            if (typeSound != null && c != ' ')
            {
                typeSound.PlayOneShot(typeSound.clip);
            }

            yield return new WaitForSeconds(typingSpeed);
        }
    }

    IEnumerator ShowFactThenLoadScene(string fact)
    {
        yield return StartCoroutine(TypeFactText(fact));
        yield return StartCoroutine(LoadTargetSceneAsync());
    }

}
