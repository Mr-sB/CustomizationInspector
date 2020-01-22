using System;
using UnityEngine;

namespace CustomizationInspector.Runtime
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
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
