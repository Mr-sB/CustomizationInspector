using CustomizationInspector.Runtime;
using UnityEditor;
using UnityEngine;

namespace CustomizationInspector.Editor
{
    [CustomPropertyDrawer(typeof(MinMaxValue))]
    public class MinMaxValueDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty editModeProperty = property.FindPropertyRelative("mEditMode");
            SerializedProperty minLimitProperty = property.FindPropertyRelative("MinLimit");
            SerializedProperty maxLimitProperty = property.FindPropertyRelative("MaxLimit");
            SerializedProperty minProperty = property.FindPropertyRelative("Min");
            SerializedProperty maxProperty = property.FindPropertyRelative("Max");
            float editBtnWith = 100;
            float limitRectWidth = 40;
            float space = 2;
            
            var labelRect = new Rect(position.x, position.y, position.width -  editBtnWith, EditorGUIUtility.singleLineHeight);
            var editBtnRect = new Rect(labelRect.xMax, position.y, editBtnWith, EditorGUIUtility.singleLineHeight);
            EditorGUI.BeginChangeCheck();
            EditorGUI.LabelField(labelRect, label);
            //设置按钮背景颜色
            if (GUI.Button(editBtnRect, editModeProperty.boolValue ? "EditingLimit" : "EditingValue"))
            {
                editModeProperty.boolValue = !editModeProperty.boolValue;
                GUI.changed = true;
            }
            //缩进
            EditorGUI.indentLevel++;
            labelRect = EditorGUI.IndentedRect(labelRect);
            EditorGUI.indentLevel--;
            float y = position.y + labelRect.height;
            var minLimitRect = new Rect(labelRect.x, y, limitRectWidth, EditorGUIUtility.singleLineHeight);
            var maxLimitRect = new Rect(position.xMax - limitRectWidth, y, limitRectWidth, EditorGUIUtility.singleLineHeight);
            var sliderRect = new Rect(minLimitRect.xMax + space, y, maxLimitRect.xMin - minLimitRect.xMax - space, EditorGUIUtility.singleLineHeight);
            
            if (editModeProperty.boolValue)
            {
                EditorGUI.PropertyField(minLimitRect, minLimitProperty, GUIContent.none);
                EditorGUI.PropertyField(maxLimitRect, maxLimitProperty, GUIContent.none);
            }
            else
            {
                EditorGUI.PropertyField(minLimitRect, minProperty, GUIContent.none);
                EditorGUI.PropertyField(maxLimitRect, maxProperty, GUIContent.none);
            }

            float min = minProperty.floatValue;
            float max = maxProperty.floatValue;
            EditorGUI.MinMaxSlider(sliderRect, ref min, ref max, minLimitProperty.floatValue, maxLimitProperty.floatValue);
            if (EditorGUI.EndChangeCheck())
            {
                //保证数据有效性
                float minLimit = minLimitProperty.floatValue;
                float maxLimit = maxLimitProperty.floatValue;
                if (minLimit > maxLimit) minLimit = maxLimit;
                min = Mathf.Clamp(min, minLimit, maxLimit);
                max = Mathf.Clamp(max, minLimit, maxLimit);
                min = Mathf.Clamp(min, minLimit, max);
                max = Mathf.Clamp(max, min, maxLimit);
                minLimitProperty.floatValue = minLimit;
                maxLimitProperty.floatValue = maxLimit;
                minProperty.floatValue = min;
                maxProperty.floatValue = max;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2;
        }
    }
}