using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CustomizationInspector.Runtime
{
    [Serializable]
    public struct MinMaxValue
    {
        [SerializeField, HideInInspector] private bool mEditMode;
        public float MinLimit, MaxLimit, Min, Max;

        public float RandomValue => Random.Range(Min, Max);

        public MinMaxValue(float minLimit, float maxLimit)
        {
            mEditMode = false;
            MinLimit = minLimit;
            MaxLimit = maxLimit;
            Min = minLimit;
            Max = maxLimit;
        }
        
        public MinMaxValue(float minLimit, float maxLimit, float min, float max)
        {
            mEditMode = false;
            MinLimit = minLimit;
            MaxLimit = maxLimit;
            Min = min;
            Max = max;
        }
    }
}