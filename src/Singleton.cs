using UnityEditor;
using UnityEngine;

namespace DialogueSmith
{
    public class Singleton<T> where T : class, new()
    {
        public static T Instance
        {
            get {
                if (instance == null)
                    instance = new T();

                return instance;
            }
        }

        protected static T instance;
    }
}
