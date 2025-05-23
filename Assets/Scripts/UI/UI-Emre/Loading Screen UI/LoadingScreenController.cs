using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class LoadingScreenController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI factText;

    private void Start()
    {
        // JSON dosyasýný oku
        TextAsset jsonFile = Resources.Load<TextAsset>("addictionFacts");

        if (jsonFile != null)
        {
            // JSON verisini parse et
            FactData data = JsonUtility.FromJson<FactData>(jsonFile.text);

            // Rastgele bilgi seç
            if (data.facts.Count > 0)
            {
                int randomIndex = Random.Range(0, data.facts.Count);
                factText.text = data.facts[randomIndex];
            }
            else
            {
                factText.text = "Þu anda istatistik yüklenemiyor.";
            }
        }
        else
        {
            factText.text = "addictionFacts.json bulunamadý!";
        }
    }
}
