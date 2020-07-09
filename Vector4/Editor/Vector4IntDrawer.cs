using UnityEngine;
using UnityEngine.Extension;

namespace UnityEditor.Extension
{
    [CustomPropertyDrawer(typeof(Vector4Int))]
    public class Vector4IntDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            Vector4Int value = EditorExtensions.Vector4IntField(position, label, property.GetVector4Int());
            if(EditorGUI.EndChangeCheck())
                property.SetVector4Int(value);
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorExtensions.GetMultiFieldHeight(label);
        }
    }
}