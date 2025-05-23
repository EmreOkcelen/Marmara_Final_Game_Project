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

    // Hangi versiyon kullanýlacak, inspector veya baþka script'ten atanabilir
    public enum DialogVersion { LinesV1, LinesV2 }
    [SerializeField] private DialogVersion selectedVersion = DialogVersion.LinesV1;

    [Header("Diyalog bittiðinde geçilecek sahne")]
    [SerializeField] private string nextSceneName;  // Inspector'dan sahne adý girilecek

    private void Start()
    {
        selectedVersion = BlackScreenState.Instance.GetCurrentDialogVersion();
        nextSceneName = BlackScreenState.Instance.GetCurrentScene();

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
                case DialogVersion.LinesV1:
                    dialogLines = data.LinesV1?.ToArray();
                    break;
                case DialogVersion.LinesV2:
                    dialogLines = data.LinesV2?.ToArray();
                    break;
                default:
                    dialogLines = new string[] { "Diyalog versiyonu seçilmedi." };
                    break;
            }
            Debug.Log("Dialog satýrlarý yüklendi: " + dialogLines.Length);
        }
        else
        {
            Debug.LogError("dialog.json bulunamadý!");
            dialogLines = new string[] { "Diyalog dosyasý yüklenemedi." };
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
                    // Diyalog bittiðinde:
                    if (!string.IsNullOrEmpty(nextSceneName))
                    {
                        BlackScreenState.Instance.AdvanceToNext();
                        SceneLoader.LoadSceneWithLoading(nextSceneName);
                    }
                }
            }
            else
            {
                // Yazý yazýlýrken space basýlýrsa yazýnýn tamamý anýnda gözüksün
                StopAllCoroutines();
                // Burada yazýyý alttaki gibi ekliyoruz, yeni satýr alt alta
                dialogText.text += dialogLines[currentLine] + "\n";
                isTyping = false;
            }
        }
    }


    IEnumerator TypeLine(string line)
    {
        isTyping = true;

        // Burada silme yapmýyoruz, sadece yeni satýrý alt alta ekleyeceðiz
        string currentText = dialogText.text;

        // Önce boþ yazalým, sonra harf harf ekleyelim, ekrandaki metnin tamamýný kaybetmemek için:
        // Yani dialogText.text = currentText + (harf harf eklenecek);

        dialogText.text = currentText;  // var olaný koru

        foreach (char c in line)
        {
            dialogText.text += c;
            if (typeSound != null)
                typeSound.PlayOneShot(typeSound.clip);
            yield return new WaitForSeconds(typingSpeed);
        }


        dialogText.text += "\n";  // her satýrdan sonra alt satýra geç

        isTyping = false;
    }
}

