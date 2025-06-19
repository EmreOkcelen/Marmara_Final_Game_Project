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
    private const string OpenAIApiKey = "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"; // Buraya yeni API key
    private const string OpenAIApiUrl = "https://api.openai.com/v1/chat/completions";

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

    public void GenerateNPCReaction(Action<string> onDialogueReady)
    {
        if (dialogueQueue.Count > 0)
        {
            string nextDialogue = dialogueQueue.Dequeue();
            onDialogueReady?.Invoke(nextDialogue);

            if (!isFetchingNewDialogue && dialogueQueue.Count < 2)
            {
                StartCoroutine(GetChatResponse());
            }
        }
        else
        {
            onDialogueReady?.Invoke("Diyalog yükleniyor...");
            if (!isFetchingNewDialogue)
            {
                StartCoroutine(GetChatResponse());
            }
        }
    }



    IEnumerator GetChatResponse()
    {
        isFetchingNewDialogue = true;

        string systemPrompt = "Sen acımasız, sert, kaba ve sinirli bir sokak NPC'sisin. Sana biri çarptığında, kesinlikle kırıcı, iğrenç, küçümseyici ve sinirli bir şekilde 1-2 cümleyle sert bir tepki ver. Kibar olma, lafı geveleme, doğrudan ve net ol. (Küfür etme.)";
        string userPrompt = "Bir alkol bağımlısı sana çarptı. Ona 1-2 cümle ile iğrenç, sert ve sinirli şekilde kırıcı bir cevap ver.";

        string json = @"{
        ""model"": ""gpt-3.5-turbo"",
        ""messages"": [
        {""role"": ""system"", ""content"": """ + systemPrompt + @"""}, 
        {""role"": ""user"", ""content"": """ + userPrompt + @"""}
    ],
    ""max_tokens"": 125,
    ""temperature"": 0.8
}";


        using (UnityWebRequest www = new UnityWebRequest(OpenAIApiUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", "Bearer " + OpenAIApiKey);

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
