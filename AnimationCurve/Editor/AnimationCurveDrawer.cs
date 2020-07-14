using GameUtil.Extensions;
using UnityEditor;
using UnityEngine;

namespace CustomizationInspector.Editor
{
    [CustomPropertyDrawer(typeof(UnityEngine.AnimationCurve))]
    public class AnimationCurveDrawer : PropertyDrawer
    {
        private const float BUTTON_WIDTH = 20;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.width -= (BUTTON_WIDTH + 2) * 2;
            EditorGUI.PropertyField(position, property, label, true);
            position.x = position.xMax + 2;
            position.width = BUTTON_WIDTH;
            if (GUI.Button(position,  "C"))
            {
                GUIUtility.systemCopyBuffer = property.animationCurveValue.AnimationCurveToJson();
            }
            position.x = position.xMax + 2;
            if (GUI.Button(position, "P"))
            {
                var curve = UnityExtensions.AnimationCurveFromJson(GUIUtility.systemCopyBuffer);
                if(curve == null) return;
                Undo.RecordObject(property.serializedObject.targetObject, "PasteCurve");
                property.animationCurveValue = curve;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, true);
        }
    }
}