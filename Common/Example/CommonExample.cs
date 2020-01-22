using CustomizationInspector.Runtime;
using UnityEngine;

namespace CustomizationInspector.Example
{
    public class CommonExample : MonoBehaviour
    {
        [Rename("是否隐藏")] public bool Hide;

        [HideIf(nameof(Hide))] //支持值为bool的field、property、method
        public Vector3 HideField;

        [Rename("是否显示")] public bool Show;

        [ShowIf(nameof(Show))] //支持值为bool的field、property、method
        public Vector3 ShowField;

        [ReadOnly] public Vector3 ReadOnlyField;

        [ValueDropdown(nameof(DropdownArray))] //支持值为IList的field、property、method
        public string ValueDropdownField;

        private string[] DropdownArray = {"Value1", "Value2", "Value3"};

        [InfoBox("InfoBox!", MessageType.Info)]
        public Vector3 InfoBoxField;

        [Button]
        [Button("测试按钮")]
        private void TestButton()
        {
            Debug.LogError(nameof(TestButton));
        }
    }
}
