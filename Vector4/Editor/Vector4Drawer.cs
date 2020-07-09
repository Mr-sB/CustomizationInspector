using UnityEngine;

namespace UnityEditor.Extension
{
    [CustomPropertyDrawer(typeof(Vector4))]
    public class Vector4Drawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.vector4Value = EditorExtensions.Vector4Field(position, label, property.vector4Value);
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorExtensions.GetMultiFieldHeight(label);
        }
    }
}