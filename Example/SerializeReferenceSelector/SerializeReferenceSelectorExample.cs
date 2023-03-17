using System;
using System.Collections;
using System.Collections.Generic;
using CustomizationInspector.Runtime;
using UnityEngine;

namespace CustomizationInspector.Example
{
    public class SerializeReferenceSelectorExample : MonoBehaviour
    {
        public interface ITest
        {
        }
        
        [Serializable]
        public class TestA : ITest
        {
            public int a;
            public int b;
        }
        
        [Serializable]
        public class TestB : ITest
        {
            public string a;
            public GameObject b;
        }

        [SerializeReference, SerializeReferenceSelector]
        public ITest test1;
        [SerializeReference, SerializeReferenceSelector]
        public List<ITest> test2;
        [SerializeReference, SerializeReferenceSelector]
        public ITest[] test3;
    }
}