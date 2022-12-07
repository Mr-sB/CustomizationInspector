using CustomizationInspector.Runtime;
using UnityEngine;

namespace CustomizationInspector.Example
{
    public class CommonExample : MonoBehaviour
    {
        [Foldout("Foldout A")]
        [Rename("是否隐藏")]
        public bool Hide;

        [Foldout("Foldout A")]
        [HideIf(nameof(Hide))] //支持值为bool的field、property、method
        public Vector3 HideField;

        [Foldout("Foldout A")]
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

        [Foldout("Foldout A")]
        [InfoBox("InfoBox!", MessageType.Info)]
        public Vector3 InfoBoxField;

        [Button]
        [Button("测试按钮")]
        private void TestButton()
        {
            Debug.LogError(nameof(TestButton));
        }
        
        [Button(null, 2, "str")]
        [Button("TestButtonWithParameter1Error")]
        [Button("TestButtonWithParameter1UseDefault", 1)]
        private void TestButtonWithParameter1(int a, string b = "default")
        {
            Debug.LogError(nameof(TestButtonWithParameter1) + ": " + a + ", " + b);
        }
        
#if UNITY_EDITOR
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
