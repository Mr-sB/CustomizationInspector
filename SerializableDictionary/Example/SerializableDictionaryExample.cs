using CustomizationInspector.Runtime;
using System;
using UnityEngine;

namespace CustomizationInspector.Example
{
    public class SerializableDictionaryExample : MonoBehaviour
    {
        public SerializableDictionaryIntFloat IntFloatDict;
        public SerializableDictionaryStringGameObject StringGameObjectDict;
        public SerializableDictionaryTransformCustumClass TransformCustumClassDict;

        [Serializable]
        public class SerializableDictionaryIntFloat : SerializableDictionary<int, float>
        {
        }

        [Serializable]
        public class SerializableDictionaryStringGameObject : SerializableDictionary<string, GameObject>
        {
        }

        [Serializable]
        public class SerializableDictionaryTransformCustumClass : SerializableDictionary<Transform, CustomClass>
        {
        }

        [Serializable]
        public class CustomClass
        {
            public int A;
            public string B;
        }
    }
}
