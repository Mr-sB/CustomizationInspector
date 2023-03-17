using CustomizationInspector.Runtime;
using UnityEngine;

namespace CustomizationInspector.Example
{
    public class MinMaxExample : MonoBehaviour
    {
        [MinMax(0, 1)]//只能修饰Vector2
        public Vector2 MinMaxAttributeField;
        
        public MinMaxValue MinMaxValueField;
    }
}