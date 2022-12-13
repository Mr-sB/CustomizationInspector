#if UNITY_2019_3_OR_NEWER
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomizationInspector.Runtime
{
    public abstract class SerializableReferenceDictionaryBase{}

    [Serializable]
    public class SerializableReferenceDictionary<TK, TV> : SerializableReferenceDictionaryBase, ISerializationCallbackReceiver, IDictionary<TK, TV>
    {
        [SerializeField] protected List<TK> mKeys = new List<TK>();
        [SerializeField, SerializeReference] protected List<TV> mValues = new List<TV>();
        protected Dictionary<TK, TV> mDictionary = new Dictionary<TK, TV>();
        public Dictionary<TK, TV> Dictionary => mDictionary;
        //隐式转换
        public static implicit operator Dictionary<TK, TV>(SerializableReferenceDictionary<TK, TV> serializableDictionary)
        {
            return serializableDictionary.mDictionary;
        }

        //序列化
        public void OnBeforeSerialize()
        {
            mKeys.Clear();
            mValues.Clear();
            int count = mDictionary.Count;
            if (mKeys.Capacity < count)
                mKeys.Capacity = count;
            if (mValues.Capacity < count)
                mValues.Capacity = count;
            foreach (KeyValuePair<TK, TV> item in mDictionary)
            {
                mKeys.Add(item.Key);
                mValues.Add(item.Value);
            }
        }
        
        //反序列化
        public void OnAfterDeserialize()
        {
            mDictionary.Clear();
            for (int i = 0, count = mKeys.Count; i < count; i++)
                mDictionary.Add(mKeys[i], mValues[i]);
        }
        
        public IEnumerator<KeyValuePair<TK, TV>> GetEnumerator()
        {
            return mDictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<TK, TV> item)
        {
            ((IDictionary<TK, TV>)mDictionary).Add(item);
        }

        public void Clear()
        {
            mDictionary.Clear();
        }

        public bool Contains(KeyValuePair<TK, TV> item)
        {
            return ((IDictionary<TK, TV>) mDictionary).Contains(item);
        }

        public void CopyTo(KeyValuePair<TK, TV>[] array, int arrayIndex)
        {
            ((IDictionary<TK, TV>)mDictionary).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TK, TV> item)
        {
            return ((IDictionary<TK, TV>) mDictionary).Remove(item);
        }

        public int Count => mDictionary.Count;
        public bool IsReadOnly => ((IDictionary<TK, TV>) mDictionary).IsReadOnly;
        public void Add(TK key, TV value)
        {
            mDictionary.Add(key, value);
        }

        public bool ContainsKey(TK key)
        {
            return mDictionary.ContainsKey(key);
        }

        public bool Remove(TK key)
        {
            return mDictionary.Remove(key);
        }

        public bool TryGetValue(TK key, out TV value)
        {
            return mDictionary.TryGetValue(key, out value);
        }

        public TV this[TK key]
        {
            get => mDictionary[key];
            set => mDictionary[key] = value;
        }

        public ICollection<TK> Keys => mDictionary.Keys;
        public ICollection<TV> Values => mDictionary.Values;
    }
}
#endif
