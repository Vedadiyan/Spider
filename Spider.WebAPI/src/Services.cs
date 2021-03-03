using System;
using System.Collections.Generic;

namespace Spider.WebAPI
{
    public class Services
    {
        private static Services instance = new Services();
        public static Services GetServices()
        {
            return instance;
        }
        private Dictionary<String, Func<Object>> serviceRepository;
        private Services()
        {
            serviceRepository = new Dictionary<string, Func<Object>>();
        }
        public void AddTransient<T>(Func<T> instanceGenerator, String name = null)
        {
            serviceRepository.Add(name ?? typeof(T).Name, () => { return instanceGenerator(); });
        }
        public void AddSingleton<T>(Func<T> instanceGenerator, String name = null)
        {
            Object instance = instanceGenerator();
            serviceRepository.Add(name ?? typeof(T).Name, () => { return instance; });
        }
        public T GetService<T>()
        {
            if (serviceRepository.TryGetValue(typeof(T).Name, out Func<Object> value))
            {
                return (T)value();
            }
            else
            {
                return default;
            }
        }
        public Object GetService(string name)
        {
            if (serviceRepository.TryGetValue(name, out Func<Object> value))
            {
                return value();
            }
            else
            {
                return null;
            }
        }
    }
    public enum ServiceLifeCycle
    {
        Transient = 0,
        Singleton = 1
    }
}