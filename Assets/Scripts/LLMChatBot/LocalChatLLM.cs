using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using TMPro;
using System;
using Newtonsoft.Json.Linq;

public class LocalChatLLM : MonoBehaviour
{
   // public TextMeshProUGUI npcReactionText;

    private const string OpenRouterApiKey = "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";
    private const string OpenRouterApiUrl = "https://openrouter.ai/api/v1/chat/completions";

    public static LocalChatLLM Instance;

    private Queue<string> dialogueQueue = new Queue<string>();
    private bool isFetchingNewDialogue = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Başlangıçta 3 diyalog çekip kuyruğu dolduruyoruz
        StartCoroutine(InitializeDialogueQueue(3));
    }
    IEnumerator InitializeDialogueQueue(int count)
    {
        for (int i = 0; i < count; i++)
        {
            yield return StartCoroutine(GetChatResponse());
        }

        Debug.Log("Başlangıç diyaloğu kuyruğu dolduruldu.");
    }

    public void GenerateNPCReaction()
    {
        Debug.Log("GenerateNPCReaction çağrıldı, dialogueQueue.Count = " + dialogueQueue.Count);

        if (dialogueQueue.Count > 0)
        {
            Debug.Log("buraya giriyorha");
            string nextDialogue = dialogueQueue.Dequeue();
           // npcReactionText.text = nextDialogue;
            Debug.Log("John Amca: " + nextDialogue);


            if (!isFetchingNewDialogue && dialogueQueue.Count < 2)
            {
                StartCoroutine(GetChatResponse());
            }
        }
        else
        {
         //   npcReactionText.text = "Diyalog yükleniyor...";
            Debug.Log("John Amca: Diyalog yükleniyor...");
            if (!isFetchingNewDialogue)
            {
                StartCoroutine(GetChatResponse());
            }
        }
    }


    IEnumerator GetChatResponse()
    {
        isFetchingNewDialogue = true;

        string systemPrompt = "Sen acımasız, sert, kaba ve sinirli bir sokak NPC'sisin. Biri sana çarptığında, kesinlikle kırıcı, iğrenç, küçümseyici ve sinirli bir şekilde 1-2 cümleyle sitem et. Kibar olma, lafı geveleme, doğrudan vur.";
        string userPrompt = "Bir alkol bağımlısı sana çarptı. Ona 1-2 cümle ile iğrenç, sert ve sinirli şekilde kırıcı bir cevap ver.";

        string json = @"{
        ""model"": ""openai/gpt-3.5-turbo"",
        ""messages"": [
            {""role"": ""system"", ""content"": """ + systemPrompt + @"""}, 
            {""role"": ""user"", ""content"": """ + userPrompt + @"""}
        ],
        ""max_tokens"": 125,
        ""temperature"": 0.8
    }";

        using (UnityWebRequest www = new UnityWebRequest(OpenRouterApiUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", "Bearer " + OpenRouterApiKey);

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string responseText = www.downloadHandler.text;

                try
                {
                    JObject jsonResponse = JObject.Parse(responseText);
                    string reply = jsonResponse["choices"][0]["message"]["content"].ToString().Trim();

                    dialogueQueue.Enqueue(reply);
                    Debug.Log("Yeni diyalog kuyruğa eklendi: " + reply);
                }
                catch (Exception e)
                {
                    Debug.LogError("JSON Parse Hatası: " + e.Message);
                }
            }
            else
            {
                Debug.LogError("API hatası: " + www.error);
                Debug.LogError("Body: " + www.downloadHandler.text);
            }
        }

        isFetchingNewDialogue = false;
    }
}
