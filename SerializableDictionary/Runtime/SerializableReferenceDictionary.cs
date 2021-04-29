#if UNITY_2019_3_OR_NEWER
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomizationInspector.Runtime
{
    public abstract class SerializableReferenceDictionaryBase{}

    [Serializable]
    public class SerializableReferenceDictionary<TK, TV> : SerializableReferenceDictionaryBase, ISerializationCallbackReceiver
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
    }
}
#endif
