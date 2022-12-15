using System;
using System.Diagnostics;
using UnityEngine;

namespace CustomizationInspector.Runtime
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    [Conditional("UNITY_EDITOR")]
    public class MinMaxAttribute : PropertyAttribute
    {
        public readonly float MinLimit;
        public readonly float MaxLimit;

        public MinMaxAttribute(float minLimit, float maxLimit)
        {
            MinLimit = minLimit <= maxLimit ? minLimit : maxLimit;
            MaxLimit = maxLimit;
        }
    }
}
