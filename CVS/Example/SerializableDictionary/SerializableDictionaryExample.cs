using CustomizationInspector.Runtime;
using System;
using UnityEngine;

namespace CustomizationInspector.Example
{
    public class SerializableDictionaryExample : MonoBehaviour
    {
        public SerializableDictionaryIntFloat IntFloatDict;
        public SerializableDictionaryStringGameObject StringGameObjectDict;
        public SerializableDictionaryTransformCustomClass TransformCustomClassDict;
#if UNITY_2019_3_OR_NEWER
        public SerializableReferenceDictionaryIntCustomClass IntCustomClassReferenceDict =
            new SerializableReferenceDictionaryIntCustomClass
            {
                Dictionary =
                {
                    {1, new CustomClass {A = 1, B = "Test1"}},
                    {2, new CustomClassChild {A = 2, B = "Test2", C = 1.5f}}
                }
            };
#endif

        [Serializable]
        public class SerializableDictionaryIntFloat : SerializableDictionary<int, float>
        {
        }

        [Serializable]
        public class SerializableDictionaryStringGameObject : SerializableDictionary<string, GameObject>
        {
        }

        [Serializable]
        public class SerializableDictionaryTransformCustomClass : SerializableDictionary<Transform, CustomClass>
        {
        }

        [Serializable]
        public class CustomClass
        {
            public int A;
            public string B;
        }
        
#if UNITY_2019_3_OR_NEWER
        [Serializable]
        public class SerializableReferenceDictionaryIntCustomClass : SerializableReferenceDictionary<int, CustomClass>
        {
        }
        
        [Serializable]
        public class CustomClassChild : CustomClass
        {
            public float C;
        }
#endif
    }
}
