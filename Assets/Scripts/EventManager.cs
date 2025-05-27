using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : MonoBehaviour
{
    private Dictionary<string, UnityEvent> eventDictionary = new Dictionary<string, UnityEvent>();
    public static EventManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void Subscribe(string eventName, UnityAction listener)
    {
        if (Instance == null) return;
        
        if (!Instance.eventDictionary.ContainsKey(eventName))
            Instance.eventDictionary[eventName] = new UnityEvent();
        
        Instance.eventDictionary[eventName].AddListener(listener);
    }

    public static void Unsubscribe(string eventName, UnityAction listener)
    {
        if (Instance?.eventDictionary.ContainsKey(eventName) == true)
            Instance.eventDictionary[eventName].RemoveListener(listener);
    }

    public static void Trigger(string eventName)
    {
        if (Instance?.eventDictionary.ContainsKey(eventName) == true)
            Instance.eventDictionary[eventName].Invoke();
    }
}