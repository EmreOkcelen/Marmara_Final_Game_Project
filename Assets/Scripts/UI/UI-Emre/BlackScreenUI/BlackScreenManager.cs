using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR;

public class BlackScreenManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private AudioSource typeSound;
    [SerializeField] private float typingSpeed = 0.05f;

    private InputDevice leftController;
    private InputDevice rightController;

    private string[] dialogLines;
    private int currentLine = 0;
    private bool isTyping = false;

    // Hangi versiyon kullan�lacak, inspector veya ba�ka script'ten atanabilir
    public enum DialogVersion {ilkSahne,Dus,EvdenAyrilma,MetroAyrilma,SonSahne }
    public DialogVersion selectedVersion ;

    [Header("Diyalog bitti�inde ge�ilecek sahne")]
    public static string nextSceneName;
    public static string mySceneNext;  // Inspector'dan sahne ad� girilecek

    private void Awake()
    {
        InputDevices.deviceConnected += OnDeviceConnected;
        InitializeOpenXRControllers();
    }
    private void OnDestroy()
    {
        InputDevices.deviceConnected -= OnDeviceConnected;
    }
    private void OnDeviceConnected(InputDevice device)
    {
        InitializeOpenXRControllers();
    }

    private void InitializeOpenXRControllers()
    {
        var allDevices = new List<InputDevice>();
        InputDevices.GetDevices(allDevices);

        foreach (var d in allDevices)
        {
            if ((d.characteristics & InputDeviceCharacteristics.Left) != 0 &&
                (d.characteristics & InputDeviceCharacteristics.Controller) != 0)
            {
                leftController = d;
                Debug.Log($"🤚 Sol kontrolcü: {d.name}");
            }
            if ((d.characteristics & InputDeviceCharacteristics.Right) != 0 &&
                (d.characteristics & InputDeviceCharacteristics.Controller) != 0)
            {
                rightController = d;
                Debug.Log($"🤚 Sağ kontrolcü: {d.name}");
            }
        }
    }
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
                    dialogLines = data.ilkSahne?.ToArray();
                    break;
                case DialogVersion.EvdenAyrilma:
                    dialogLines = data.EvdenAyrilma?.ToArray();
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
        if (Input.GetKeyDown(KeyCode.Space) || leftController.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryButtonPressed) && primaryButtonPressed ||
            rightController.TryGetFeatureValue(CommonUsages.primaryButton, out bool secondaryButtonPressed) && secondaryButtonPressed)
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
                    if (!string.IsNullOrEmpty(mySceneNext))
                    {
                        BlackScreenState.Instance.AdvanceToNext();
                        SceneLoader.LoadSceneWithLoading(mySceneNext);
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

