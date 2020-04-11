using System;
using System.Collections.Generic;

namespace Sand
{
    public class EventEmitter
    {
        private Dictionary<string, Action<string>> eventDictionary;

        public EventEmitter()
        {
            this.eventDictionary = new Dictionary<string, Action<string>>();
        }

        public void On(string eventName, Action<string> action)
        {
            if (eventDictionary.ContainsKey(eventName))
            {
                eventDictionary[eventName] += action;
            }
            else
            {
                eventDictionary.Add(eventName, action);
            }
        }

        public void Emit(string eventName, string payload)
        {
            if (eventDictionary.ContainsKey(eventName))
            {
                eventDictionary[eventName].Invoke(payload);
            }
        }

        public void RemoveListener(string eventName, Action<string> action)
        {
            if (eventDictionary.ContainsKey(eventName))
            {
                eventDictionary[eventName] -= action;
            }
        }
    }
}
