using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : MonoBehaviour
{
    // Inspector'da her olay için birer satır gözükecek
    [System.Serializable]
    public class EventEntry
    {
        public string eventName;
        public UnityEvent response;
    }

    [SerializeField]
    private List<EventEntry> events = new List<EventEntry>();

    // Hızlı lookup için runtime-dictionary
    private Dictionary<string, UnityEvent> eventDictionary;

    // Singleton instance (isteğe bağlı)
    public static EventManager Instance { get; private set; }

    private void Awake()
    {
        // Singleton ataması
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Dictionary’i oluştur ve inspector’daki listeyi ekle
        eventDictionary = new Dictionary<string, UnityEvent>();
        foreach (var entry in events)
        {
            if (!string.IsNullOrEmpty(entry.eventName))
            {
                eventDictionary[entry.eventName] = entry.response;
            }
        }
    }

    // Event’e abone olma
    public static void Subscribe(string eventName, UnityAction listener)
    {
        if (Instance.eventDictionary.TryGetValue(eventName, out UnityEvent thisEvent))
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            Debug.LogWarning($"EventManager: '{eventName}' adında bir event yok.");
        }
    }

    // Event’ten aboneliği kaldırma
    public static void Unsubscribe(string eventName, UnityAction listener)
    {
        if (Instance.eventDictionary.TryGetValue(eventName, out UnityEvent thisEvent))
        {
            thisEvent.RemoveListener(listener);
        }
    }

    // Event’i tetikleme
    public static void Trigger(string eventName)
    {
        if (Instance.eventDictionary.TryGetValue(eventName, out UnityEvent thisEvent))
        {
            thisEvent.Invoke();
        }
    }
    
}