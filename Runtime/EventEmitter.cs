using System;
using System.Collections.Generic;

namespace Sand
{
    public class EventEmitter
    {
        public Dictionary<string, Action<string>> EventDictionary { get; private set; }

        public EventEmitter()
        {
            this.EventDictionary = new Dictionary<string, Action<string>>();
        }

        public void On(string eventName, Action<string> action)
        {
            if (EventDictionary.ContainsKey(eventName))
            {
                EventDictionary[eventName] += action;
            }
            else
            {
                EventDictionary.Add(eventName, action);
            }
        }

        public void Emit(string eventName, string payload)
        {
            if (EventDictionary.ContainsKey(eventName) && EventDictionary[eventName] != null)
            {
                EventDictionary[eventName].Invoke(payload);
            }
        }

        public void RemoveListener(string eventName, Action<string> action)
        {
            if (EventDictionary.ContainsKey(eventName))
            {
                EventDictionary[eventName] -= action;
            }
        }
    }
}
