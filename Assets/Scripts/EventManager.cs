using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager
{
    // Event'leri Dictionary ile saklıyoruz
    public static event Action JumpEvent;
    public static event Action HelloEvent;
    private static Dictionary<string, Action> eventDictionary = new Dictionary<string, Action>();

    // Event'e abone olma (Subscribe)
    public static void Subscribe(string eventName, Action listener)
    {
        if (eventDictionary.ContainsKey(eventName))
        {
            eventDictionary[eventName] += listener;
        }
        else
        {
            eventDictionary[eventName] = listener;
        }
    }

    // Event'ten aboneliği kaldırma (Unsubscribe)
    public static void Unsubscribe(string eventName, Action listener)
    {
        if (eventDictionary.ContainsKey(eventName))
        {
            eventDictionary[eventName] -= listener;

            if (eventDictionary[eventName] == null)
            {
                eventDictionary.Remove(eventName);
            }
        }
    }

    // Event'i tetikleme (Trigger)
    public static void Trigger(string eventName)
    {
        if (eventDictionary.ContainsKey(eventName))
        {
            eventDictionary[eventName]?.Invoke();
        }
    }

    // Bütün event'leri temizleme (Clear)
    public static void ClearAllEvents()
    {
        eventDictionary.Clear();
    }
}