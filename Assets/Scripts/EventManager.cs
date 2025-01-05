using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager
{
    private static Dictionary<string, Action> eventDictionary = new Dictionary<string, Action>();

    // Event'e abone olma
    public static void Subscribe(string eventName, Action listener)
    {
        if (eventDictionary.TryGetValue(eventName, out Action action))
        {
            eventDictionary[eventName] = action + listener;
        }
        else
        {
            eventDictionary[eventName] = listener;
        }
    }

    // Event'ten aboneliği kaldırma
    public static void Unsubscribe(string eventName, Action listener)
    {
        if (eventDictionary.TryGetValue(eventName, out Action action))
        {
            action -= listener;
            if (action == null)
            {
                eventDictionary.Remove(eventName);
            }
            else
            {
                eventDictionary[eventName] = action;
            }
        }
    }

    // Event'i tetikleme
    public static void Trigger(string eventName)
    {
        if (eventDictionary.TryGetValue(eventName, out Action action))
        {
            action?.Invoke();
        }
    }

    // Bütün event'leri temizleme
    public static void ClearAllEvents()
    {
        foreach (var key in eventDictionary.Keys)
        {
            eventDictionary[key] = null;  // Bellek sızıntısını engellemek için
        }
        eventDictionary.Clear();
    }
}