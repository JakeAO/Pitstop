using System.Collections.Generic;
using UnityEngine;

namespace SadPumpkin.Game.Pitstop.Core.Code.Util
{
    [CreateAssetMenu]
    public class InterimDataHolder : ScriptableObject
    {
        private readonly Dictionary<string, object> _holder = new Dictionary<string, object>();

        public void Push<T>(T obj, string fieldName)
        {
            string key = typeof(T).FullName + fieldName;
            _holder[key] = obj;
        }

        public T Pop<T>(string fieldName)
        {
            string key = typeof(T).FullName + fieldName;
            if (_holder.TryGetValue(key, out object tObj))
            {
                _holder.Remove(key);
                return (T)tObj;
            }

            return default;
        }

        public T Peek<T>(string fieldName)
        {
            string key = typeof(T).FullName + fieldName;
            if (_holder.TryGetValue(key, out object tObj))
            {
                return (T)tObj;
            }

            return default;
        }

        public void Clear()
        {
            _holder.Clear();
        }
    }
}