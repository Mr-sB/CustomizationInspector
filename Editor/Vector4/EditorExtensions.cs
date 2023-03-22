using UnityEngine;
using UnityEngine.Extension;

namespace UnityEditor.Extension
{
    public static class EditorExtensions
    {
        private static readonly int s_FoldoutHash = "Foldout".GetHashCode();
        private static readonly float[] s_Vector4s = new float[4];
        private static readonly int[] s_Vector4Ints = new int[4];

        private static readonly GUIContent[] s_XYZWLabels = new GUIContent[4]
        {
            new GUIContent("X"),
            new GUIContent("Y"),
            new GUIContent("Z"),
            new GUIContent("W")
        };

        public static Vector4 Vector4Field(
            Rect position,
            GUIContent label,
            Vector4 value)
        {
            int controlId = GUIUtility.GetControlID(s_FoldoutHash, FocusType.Keyboard, position);
            position = MultiFieldPrefixLabel(position, controlId, label, 4);
            position.height = EditorGUIUtility.singleLineHeight;
            return Vector4Field(position, value);
        }
        
        public static Vector4Int Vector4IntField(
            Rect position,
            GUIContent label,
            Vector4Int value)
        {
            int controlId = GUIUtility.GetControlID(s_FoldoutHash, FocusType.Keyboard, position);
            position = MultiFieldPrefixLabel(position, controlId, label, 4);
            position.height = EditorGUIUtility.singleLineHeight;
            return Vector4IntField(position, value);
        }
        
        private static Vector4 Vector4Field(Rect position, Vector4 value)
        {
            s_Vector4s[0] = value.x;
            s_Vector4s[1] = value.y;
            s_Vector4s[2] = value.z;
            s_Vector4s[3] = value.w;
            position.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.BeginChangeCheck();
            EditorGUI.MultiFloatField(position, s_XYZWLabels, s_Vector4s);
            if (EditorGUI.EndChangeCheck())
            {
                value.x = s_Vector4s[0];
                value.y = s_Vector4s[1];
                value.z = s_Vector4s[2];
                value.w = s_Vector4s[3];
            }

            return value;
        }

        private static Vector4Int Vector4IntField(Rect position, Vector4Int value)
        {
            s_Vector4Ints[0] = value.x;
            s_Vector4Ints[1] = value.y;
            s_Vector4Ints[2] = value.z;
            s_Vector4Ints[3] = value.w;
            position.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.BeginChangeCheck();
            EditorGUI.MultiIntField(position, s_XYZWLabels, s_Vector4Ints);
            if (EditorGUI.EndChangeCheck())
            {
                value.x = s_Vector4Ints[0];
                value.y = s_Vector4Ints[1];
                value.z = s_Vector4Ints[2];
                value.w = s_Vector4Ints[3];
            }

            return value;
        }

        public static Rect MultiFieldPrefixLabel(Rect totalPosition, int id, GUIContent label, int columns)
        {
            if (!LabelHasContent(label))
                return EditorGUI.IndentedRect(totalPosition);
            if (EditorGUIUtility.wideMode)
            {
                Rect labelPosition = EditorGUI.IndentedRect(new Rect(totalPosition.x, totalPosition.y,
                    totalPosition.width, EditorGUIUtility.singleLineHeight));
                Rect rect = totalPosition;
                rect.xMin += EditorGUIUtility.labelWidth + 2f;
                // if (columns > 1)
                // {
                //     --labelPosition.width;
                //     --rect.xMin;
                // }

                if (columns == 2)
                {
                    float num = (float) ((rect.width - 8.0) / 3.0);
                    rect.xMax -= num + 4f;
                }

                EditorGUI.HandlePrefixLabel(totalPosition, labelPosition, label, id);
                return rect;
            }

            Rect labelPosition1 = EditorGUI.IndentedRect(new Rect(totalPosition.x, totalPosition.y, totalPosition.width,
                EditorGUIUtility.singleLineHeight));
            Rect rect1 = totalPosition;
            rect1.xMin += (EditorGUI.indentLevel + 1) * 15f;
            rect1.yMin += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.HandlePrefixLabel(totalPosition, labelPosition1, label, id);
            return rect1;
        }

        public static float GetMultiFieldHeight(GUIContent label)
        {
            if (!LabelHasContent(label))
                return EditorGUIUtility.singleLineHeight;
            if (EditorGUIUtility.wideMode)
                return EditorGUIUtility.singleLineHeight;
            return EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
        }

        public static bool LabelHasContent(GUIContent label)
        {
            return label == null || (label.text != string.Empty || label.image != null);
        }
        
        public static Vector4Int GetVector4Int(this SerializedProperty property)
        {
            return new Vector4Int(property.FindPropertyRelative("m_X").intValue,
                property.FindPropertyRelative("m_Y").intValue,
                property.FindPropertyRelative("m_Z").intValue, property.FindPropertyRelative("m_W").intValue);
        }
        
        public static void SetVector4Int(this SerializedProperty property, Vector4Int vector4Int)
        {
            property.FindPropertyRelative("m_X").intValue = vector4Int.x;
            property.FindPropertyRelative("m_Y").intValue = vector4Int.y;
            property.FindPropertyRelative("m_Z").intValue = vector4Int.z;
            property.FindPropertyRelative("m_W").intValue = vector4Int.w;
        }
    }
}