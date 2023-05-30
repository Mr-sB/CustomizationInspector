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
        [Obsolete("Use XMinPadding instead.")]
        public float XOffset
        {
            set => XMinPadding = value;
            get => XMinPadding;
        }
        /// <summary>
        /// Negtive means use EditorGUIUtility.labelWidth.
        /// </summary>
        public float XMinPadding = -1;
        /// <summary>
        /// Can not be negtive. Negtive means zero.
        /// </summary>
        public float XMaxPadding = 0;

        public SerializeReferenceSelectorAttribute()
        {
            
        }
        
        public SerializeReferenceSelectorAttribute(int minLine)
        {
            MinLine = minLine;
        }
    }
}
