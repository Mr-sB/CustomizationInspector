using CustomizationInspector.Runtime;
using UnityEngine;

namespace CustomizationInspector.Example
{
    // [CreateAssetMenu]
    public class CommonExampleSO : ScriptableObject
    {
        [Foldout("Foldout A")]
        [Rename("是否隐藏")]
        public bool Hide;

        [Foldout("Foldout A")]
        [HideIf(nameof(Hide))] //支持值为bool的field、property、method
        public Vector3 HideField;

        [Foldout("Foldout A/Sub1")]
        [Rename("是否显示")]
        public bool Show;

        [Foldout("Foldout B")]
        [ShowIf(nameof(Show))] //支持值为bool的field、property、method
        public Vector3 ShowField;

        [ReadOnly]
        public Vector3 ReadOnlyField;

        [Foldout("Foldout B")]
        [ValueDropdown(nameof(DropdownArray))] //支持值为IList的field、property、method
        public string ValueDropdownField;

        private string[] DropdownArray = {"Value1", "Value2", "Value3"};

        [Foldout("Foldout A/Sub1/Sub2")]
        [InfoBox("InfoBox!", MessageType.Info)]
        public Vector3 InfoBoxField;

        [Button]
        [Button("测试按钮")]
        private void TestButton()
        {
            Debug.LogError(nameof(TestButton));
        }
        
        [Foldout("Foldout A")]
        [Button(null, 2, "str", 1)]
        [Button("TestButtonWithParameter1Error")]
        [Button("TestButtonWithParameter1UseDefault", 1)]
        private void TestButtonWithParameter1(int a, [System.ComponentModel.DefaultValue("default")]string b, int c = 99)
        {
            Debug.LogError(nameof(TestButtonWithParameter1) + ": " + a + ", " + b + ", " + c);
        }
        
#if UNITY_EDITOR
        [Foldout("Foldout B")]
        //IL2CPP does not support attributes with object arguments that are array types. But Mono support.
        [Button(null, new []{1,2,3})]
        private void TestButtonWithParameter2(int[] array)
        {
            if (array == null)
            {
                Debug.LogError(nameof(TestButtonWithParameter2) + ": " + "null");
                return;
            }
            Debug.LogError(nameof(TestButtonWithParameter2) + ": " + string.Join(",", array));
        }
        
        [Foldout("Foldout C")]
        [Button(null, new object[]{null})]
        private void TestButtonWithParameter3(int[] array)
        {
            if (array == null)
            {
                Debug.LogError(nameof(TestButtonWithParameter3) + ": " + "null");
                return;
            }
            Debug.LogError(nameof(TestButtonWithParameter3) + ": " + string.Join(",", array));
        }
        
        [Foldout("Foldout C")]
        [Button(null, new []{4,5,6})]
        private void TestButtonWithParameter4(params int[] array)
        {
            if (array == null)
            {
                Debug.LogError(nameof(TestButtonWithParameter4) + ": " + "null");
                return;
            }
            Debug.LogError(nameof(TestButtonWithParameter4) + ": " + string.Join(",", array));
        }
#endif
    }
}
