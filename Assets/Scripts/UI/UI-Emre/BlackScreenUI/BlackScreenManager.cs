using UnityEngine;
using TMPro;
using System.Collections;

public class BlackScreenManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private AudioSource typeSound;
    [SerializeField] private float typingSpeed = 0.05f;

    private string[] dialogLines;
    private int currentLine = 0;
    private bool isTyping = false;

    // Hangi versiyon kullan�lacak, inspector veya ba�ka script'ten atanabilir
    public enum DialogVersion {ilkSahne,Dus,EvdenAyrılma,MetroAyrılma,SonSahne }
    [SerializeField] private DialogVersion selectedVersion = DialogVersion.ilkSahne;

    [Header("Diyalog bitti�inde ge�ilecek sahne")]
    [SerializeField] private string nextSceneName;  // Inspector'dan sahne ad� girilecek

    private void Start()
    {
        if (BlackScreenState.Instance != null)
        {
            selectedVersion = BlackScreenState.Instance.GetCurrentDialogVersion();
            nextSceneName = BlackScreenState.Instance.GetCurrentScene();
        }
        else
        {
            Debug.LogWarning("BlackScreenState bulunamad�, default ayarlar kullan�l�yor.");
        }

        LoadDialogFromJson(selectedVersion);
        if (dialogLines != null && dialogLines.Length > 0)
            StartCoroutine(TypeLine(dialogLines[currentLine]));
    }



    void LoadDialogFromJson(DialogVersion version)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("addictionFacts");
        if (jsonFile != null)
        {
            GameTextData data = JsonUtility.FromJson<GameTextData>(jsonFile.text);
            switch (version)
            {
                case DialogVersion.ilkSahne:
                    dialogLines = data.LinesV1?.ToArray();
                    break;
                case DialogVersion.EvdenAyrılma:
                    dialogLines = data.LinesV2?.ToArray();
                    break;
                default:
                    dialogLines = new string[] { "Diyalog versiyonu se�ilmedi." };
                    break;
            }
            Debug.Log("Dialog sat�rlar� y�klendi: " + dialogLines.Length);
        }
        else
        {
            Debug.LogError("dialog.json bulunamad�!");
            dialogLines = new string[] { "Diyalog dosyas� y�klenemedi." };
        }
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            if (!isTyping)
            {
                currentLine++;
                if (currentLine < dialogLines.Length)
                {
                    StartCoroutine(TypeLine(dialogLines[currentLine]));
                }
                else
                {
                    // Diyalog bitti�inde:
                    if (!string.IsNullOrEmpty(nextSceneName))
                    {
                        BlackScreenState.Instance.AdvanceToNext();
                        SceneLoader.LoadSceneWithLoading(nextSceneName);
                    }
                }
            }
            else
            {
                // Yaz� yaz�l�rken space bas�l�rsa yaz�n�n tamam� an�nda g�z�ks�n
                StopAllCoroutines();
                // Burada yaz�y� alttaki gibi ekliyoruz, yeni sat�r alt alta
                dialogText.text += dialogLines[currentLine] + "\n";
                isTyping = false;
            }
        }
    }


    IEnumerator TypeLine(string line)
    {
        isTyping = true;

        // Burada silme yapm�yoruz, sadece yeni sat�r� alt alta ekleyece�iz
        string currentText = dialogText.text;

        // �nce bo� yazal�m, sonra harf harf ekleyelim, ekrandaki metnin tamam�n� kaybetmemek i�in:
        // Yani dialogText.text = currentText + (harf harf eklenecek);

        dialogText.text = currentText;  // var olan� koru

        foreach (char c in line)
        {
            dialogText.text += c;
            if (typeSound != null)
                typeSound.PlayOneShot(typeSound.clip);
            yield return new WaitForSeconds(typingSpeed);
        }


        dialogText.text += "\n";  // her sat�rdan sonra alt sat�ra ge�

        isTyping = false;
    }
}

