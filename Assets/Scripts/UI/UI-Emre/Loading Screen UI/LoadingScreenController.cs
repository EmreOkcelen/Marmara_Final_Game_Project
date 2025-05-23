using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class LoadingScreenController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI factText;

    private void Start()
    {
        // JSON dosyas�n� oku
        TextAsset jsonFile = Resources.Load<TextAsset>("addictionFacts");

        if (jsonFile != null)
        {
            // JSON verisini parse et
            FactData data = JsonUtility.FromJson<FactData>(jsonFile.text);

            // Rastgele bilgi se�
            if (data.facts.Count > 0)
            {
                int randomIndex = Random.Range(0, data.facts.Count);
                factText.text = data.facts[randomIndex];
            }
            else
            {
                factText.text = "�u anda istatistik y�klenemiyor.";
            }
        }
        else
        {
            factText.text = "addictionFacts.json bulunamad�!";
        }
    }
}
