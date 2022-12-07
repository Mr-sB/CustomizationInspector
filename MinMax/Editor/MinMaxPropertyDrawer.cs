using CustomizationInspector.Runtime;
using UnityEditor;
using UnityEngine;

namespace CustomizationInspector.Editor
{
    [CustomPropertyDrawer(typeof(MinMaxAttribute))]
	internal class MinMaxDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (!property.type.Equals(nameof(Vector2)))
			{
				EditorGUI.PropertyField(position, property, label, true);
				return;
			}
			var minMaxLimit = attribute as MinMaxAttribute;
            SerializedProperty minProperty = property.FindPropertyRelative("x");
            SerializedProperty maxProperty = property.FindPropertyRelative("y");
            float limitRectWidth = 40;
            float space = 2;
            
            var labelRect = position;
            labelRect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(labelRect, label);
            //缩进
            EditorGUI.indentLevel++;
            labelRect = EditorGUI.IndentedRect(labelRect);
            EditorGUI.indentLevel--;
            float y = position.y + labelRect.height;
            var minLimitRect = new Rect(labelRect.x, y, limitRectWidth, EditorGUIUtility.singleLineHeight);
            var maxLimitRect = new Rect(position.xMax - limitRectWidth, y, limitRectWidth, EditorGUIUtility.singleLineHeight);
            var sliderRect = new Rect(minLimitRect.xMax + space, y, maxLimitRect.xMin - minLimitRect.xMax - space, EditorGUIUtility.singleLineHeight);
            float minLimit = minMaxLimit.MinLimit;
            float maxLimit = minMaxLimit.MaxLimit;
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(minLimitRect, minProperty, GUIContent.none);
            EditorGUI.PropertyField(maxLimitRect, maxProperty, GUIContent.none);
            float min = minProperty.floatValue;
            float max = maxProperty.floatValue;
            EditorGUI.MinMaxSlider(sliderRect, ref min, ref max, minLimit, maxLimit);
            if (EditorGUI.EndChangeCheck())
            {
                //保证数据有效性
                min = Mathf.Clamp(min, minLimit, maxLimit);
                max = Mathf.Clamp(max, minLimit, maxLimit);
                min = Mathf.Clamp(min, minLimit, max);
                max = Mathf.Clamp(max, min, maxLimit);
                minProperty.floatValue = min;
                maxProperty.floatValue = max;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
	        if (!property.type.Equals(nameof(Vector2)))
		        return EditorGUI.GetPropertyHeight(property);
            return EditorGUIUtility.singleLineHeight * 2;
        }
	}
}