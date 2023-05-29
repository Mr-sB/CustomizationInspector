using System;
using System.Diagnostics;
using UnityEngine;

namespace CustomizationInspector.Runtime
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    [Conditional("UNITY_EDITOR")]
    public class SerializeReferenceSelectorAttribute : PropertyAttribute
    {
        public int MinLine = 16;
        public float XOffset = -1;
        public float XMaxOffset = 0;

        public SerializeReferenceSelectorAttribute()
        {
            
        }
        
        public SerializeReferenceSelectorAttribute(int minLine)
        {
            MinLine = minLine;
        }
    }
}
