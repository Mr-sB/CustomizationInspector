using GameUtil.Extensions;
using UnityEngine;

namespace UnityEditor
{
    [CustomPropertyDrawer(typeof(AnimationCurve))]
    public class AnimationCurveDrawer : PropertyDrawer
    {
        private static readonly float mButtonWidth = 20;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var pos = position;
            pos.width -= (mButtonWidth + 2) * 2;
            EditorGUI.PropertyField(pos, property, label, true);
            pos.x = pos.xMax + 2;
            pos.width = mButtonWidth;
            if (GUI.Button(pos,  "C"))
            {
                GUIUtility.systemCopyBuffer = property.animationCurveValue.keys.KeyframeArrayToJson();
            }
            pos.x = pos.xMax + 2;
            if (GUI.Button(pos, "P"))
            {
                var keyframes = UnityExtensions.KeyframeArrayFromJson(GUIUtility.systemCopyBuffer);
                if(keyframes == null) return;
                Undo.RecordObject(property.serializedObject.targetObject, "PasteCurve");
                property.animationCurveValue = new AnimationCurve(keyframes);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, true);
        }
    }
}