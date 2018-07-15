using System;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSmith.Utility
{
    [Serializable]
    public abstract class BaseSerializableDictionary<K, V> : ISerializationCallbackReceiver
    {
        public List<K> _keys = new List<K>();
        public List<V> _values = new List<V>();
        public Dictionary<K, V> data = new Dictionary<K, V>();
        public int Count { get { return data.Count; } }

        public BaseSerializableDictionary(Dictionary<K, V> dict)
        {
            foreach (KeyValuePair<K, V> item in dict)
                this.data.Add(item.Key, item.Value);
        }

        public BaseSerializableDictionary()
        {
        }

        public void Add(K key, V value)
        {
            data.Add(key, value);
        }

        public bool Has(K key)
        {
            return data.ContainsKey(key);
        }

        public V GetValue(K key)
        {
            return data[key];
        }

        public V Value(K key)
        {
            return data[key];
        }

        public void OnBeforeSerialize()
        {
            _keys.Clear();
            _values.Clear();

            foreach (var kvp in data) {
                _keys.Add(kvp.Key);
                _values.Add(kvp.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            data = new Dictionary<K, V>();

            for (int i = 0; i != Math.Min(_keys.Count, _values.Count); i++)
                data.Add(_keys[i], _values[i]);
        }
    }
}
