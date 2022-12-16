using System;
using CustomizationInspector.Runtime;
using UnityEngine;

namespace CustomizationInspector.Example
{
    // [CreateAssetMenu]
    public class CommonExampleSO : ScriptableObject
    {
        [Serializable]
        public class TestClass
        {
            public int intValue;
            public GameObject goValue;
            public Rect rectValue;
        }
        
        [Foldout("Foldout A")]
        [Rename("是否隐藏")]
        public bool Hide;

        [Foldout("Foldout A")]
        [HideIf(nameof(Hide))] //支持值为bool的field、property、method
        public Vector3 HideField;
        
        [Foldout("Foldout A")]
        [ReadOnly]
        [FilePath]
        public string filePath;
        
        [Foldout("Foldout A")]
        [FolderPath(PathLocation.AssetsFolder)]
        public string folderPath;

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

        [Button("测试按钮")]
        private void TestButton()
        {
            Debug.LogError(nameof(TestButton));
        }
        
        [Foldout("Foldout A")]
        [Button]
        private void TestButtonWithParameter1([System.ComponentModel.DefaultValue("default")]string str, Color color, int number = 99)
        {
            Debug.LogError(nameof(TestButtonWithParameter1) + ": " + str + ", " + color + ", " + number);
        }
        
        [Foldout("Foldout B")]
        [Button]
        private void TestButtonWithParameter2(Vector3[] array)
        {
            if (array == null)
            {
                Debug.LogError(nameof(TestButtonWithParameter2) + ": " + "null");
                return;
            }
            Debug.LogError(nameof(TestButtonWithParameter2) + ": " + string.Join(",", array));
        }
        
        [Foldout("Foldout A/Sub1/Sub2/Sub3")]
        [Button]
        private void TestButtonWithParameter3(TestClass testClass)
        {
            Debug.LogError(testClass.intValue, testClass.goValue);
        }
        
        [Button]
        private void TestButtonWithParameter4([System.ComponentModel.DefaultValue("test")]object objectValue, Object go)
        {
            Debug.LogError(objectValue, go);
        }
    }
}
