using System.Collections.Generic;
using UnityEngine;

namespace CustomizationInspector.Runtime
{
    public abstract class SerializableDictionaryBase{}

    public abstract class SerializableDictionary<TK, TV> : SerializableDictionaryBase, ISerializationCallbackReceiver
    {
        [SerializeField] private bool mIsAdd;
        [SerializeField] private TK mToAddKey;
        [SerializeField] private TV mToAddValue;
        
        [SerializeField] protected List<TK> mKeys = new List<TK>();
        [SerializeField] protected List<TV> mValues = new List<TV>();
        protected Dictionary<TK, TV> mDictionary = new Dictionary<TK, TV>();
        public Dictionary<TK, TV> Dictionary => mDictionary;
        //隐式转换
        public static implicit operator Dictionary<TK, TV>(SerializableDictionary<TK, TV> serializableDictionary)
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