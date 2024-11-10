using System;
using System.Collections.Generic;
using QFramework;

namespace MOYV
{
    public class EventBus
    {
        private TypeEventSystem mEventSystem;

        public void RegisterEvent<T>(Action<T> action)
        {
            mEventSystem.Register(action);
        }

        public void UnRegisterEvent<T>(Action<T> action)
        {
            mEventSystem.UnRegister(action);
        }

        public void SendEvent<T>(T t)
        {
            mEventSystem.Send(t);
        }

        public void SendEvent<T>() where T : new()
        {
            mEventSystem.Send<T>();
        }

        private EventBus()
        {
            mEventSystem = new TypeEventSystem();
        }

        
        
        private static Dictionary<string, EventBus> mEventBusMap = new Dictionary<string, EventBus>();

        public static EventBus GetOrCreate(string key = "Global")
        {
            if (mEventBusMap.TryGetValue(key, out EventBus eventBus))
            {
                return eventBus;
            }

            eventBus = mEventBusMap[key] = new EventBus();

            return eventBus;
        }
    }
}