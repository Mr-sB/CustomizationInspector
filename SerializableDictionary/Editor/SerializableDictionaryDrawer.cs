using System;
using System.Collections;
using System.Reflection;
using CustomizationInspector.Runtime;
using UnityEditor;
using UnityEngine;

namespace CustomizationInspector.Editor
{
    [CustomPropertyDrawer(typeof(SerializableDictionaryBase), true)]
    public class SerializableDictionaryDrawer : PropertyDrawer
    {
        private Color mToggleOn = Color.green;
        private Color mToggleOff = Color.white;
        private BindingFlags mBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic |
                                               BindingFlags.Public | BindingFlags.Static;

        private GUIContent mToAddContent;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (mToAddContent == null)
                mToAddContent = new GUIContent("To Add");
            SerializedProperty isAddProperty = property.FindPropertyRelative("mIsAdd");
            SerializedProperty keysProperty = property.FindPropertyRelative("mKeys");
            SerializedProperty valuesProperty = property.FindPropertyRelative("mValues");
            float propertyHeight = EditorGUIUtility.singleLineHeight;
            float lineWidth = position.width;
            var labelRect = new Rect(position.x, position.y, lineWidth, EditorGUIUtility.singleLineHeight);
            if (EditorGUI.PropertyField(labelRect, property, false))
            {
                propertyHeight += 2;
                EditorGUI.indentLevel++;
                float elementNameWidth = 80;
                float btnWidth = 20;
                var keyLabelRect = new Rect(labelRect.x + elementNameWidth - 30, position.y + propertyHeight, (lineWidth - btnWidth - elementNameWidth + 30) / 2,
                    EditorGUIUtility.singleLineHeight);
                var valueLabelRect = new Rect(keyLabelRect.xMax, position.y + propertyHeight, (lineWidth - btnWidth - elementNameWidth + 30) / 2,
                    EditorGUIUtility.singleLineHeight);
                var addToggleRect = new Rect(valueLabelRect.xMax, position.y + propertyHeight, btnWidth, EditorGUIUtility.singleLineHeight);
                propertyHeight += EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(keyLabelRect, "Key");
                EditorGUI.LabelField(valueLabelRect, "Value");
                //设置按钮背景颜色
                var backgroundColor = GUI.backgroundColor;
                GUI.backgroundColor = isAddProperty.boolValue ? mToggleOn : mToggleOff;
                if (GUI.Button(addToggleRect, "+", EditorStyles.miniButtonMid))
                {
                    isAddProperty.boolValue = !isAddProperty.boolValue;
                    GUI.changed = true;
                }

                GUI.backgroundColor = backgroundColor;
                if (isAddProperty.boolValue)
                {
                    propertyHeight += 2;
                    SerializedProperty toAddKeyProperty = property.FindPropertyRelative("mToAddKey");
                    SerializedProperty toAddValueProperty = property.FindPropertyRelative("mToAddValue");
                    //显示添加的GUI
                    var itemElementNameRect = new Rect(labelRect.x, position.y + propertyHeight, elementNameWidth, EditorGUIUtility.singleLineHeight);
                    //计算属性高度
                    float height = EditorGUIUtility.singleLineHeight;

                    var keyPropertyHeight = EditorGUI.GetPropertyHeight(toAddKeyProperty, true);
                    if (keyPropertyHeight > height)
                        height = keyPropertyHeight;
                    var keyRect = new Rect(keyLabelRect.x, position.y + propertyHeight, keyLabelRect.width, keyPropertyHeight);
                    if (!toAddKeyProperty.isExpanded)
                    {
                        toAddKeyProperty.isExpanded = true;
                        keyPropertyHeight = EditorGUI.GetPropertyHeight(toAddKeyProperty, true);
                        toAddKeyProperty.isExpanded = false;
                    }

                    var valuePropertyHeight = EditorGUI.GetPropertyHeight(toAddValueProperty, true);
                    if (valuePropertyHeight > height)
                        height = valuePropertyHeight;
                    var valueRect = new Rect(valueLabelRect.x, position.y + propertyHeight, valueLabelRect.width, valuePropertyHeight);
                    if (!toAddValueProperty.isExpanded)
                    {
                        toAddValueProperty.isExpanded = true;
                        valuePropertyHeight = EditorGUI.GetPropertyHeight(toAddValueProperty, true);
                        toAddValueProperty.isExpanded = false;
                    }

                    propertyHeight += height + 2;
                    EditorGUI.LabelField(itemElementNameRect, "AddItem");
                    var labelWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = elementNameWidth;
                    EditorGUI.PropertyField(keyRect, toAddKeyProperty,
                        toAddKeyProperty.hasVisibleChildren && keyPropertyHeight > EditorGUIUtility.singleLineHeight + 2 ? mToAddContent : GUIContent.none,
                        true);
                    EditorGUI.PropertyField(valueRect, toAddValueProperty,
                        toAddValueProperty.hasVisibleChildren && valuePropertyHeight > EditorGUIUtility.singleLineHeight + 2 ? mToAddContent : GUIContent.none,
                        true);
                    EditorGUIUtility.labelWidth = labelWidth;
                    var addRect = EditorGUI.IndentedRect(new Rect(labelRect.x, position.y + propertyHeight, lineWidth, EditorGUIUtility.singleLineHeight));
                    propertyHeight += EditorGUIUtility.singleLineHeight;
                    if (GUI.Button(addRect, "Add"))
                    {
                        Undo.RecordObject(property.serializedObject.targetObject, "Add");
                        object targetObject = property.GetObject();
                        Type type = targetObject.GetType();
                        FieldInfo toAddKeyFieldInfo = type.GetFieldInfoIncludeBase("mToAddKey", mBindingFlags);
                        FieldInfo dictFieldInfo = type.GetField("mDictionary", mBindingFlags);

                        var dict = dictFieldInfo.GetValue(targetObject) as IDictionary;
                        object key = toAddKeyFieldInfo.GetValue(targetObject);
                        if (key == null || key is UnityEngine.Object uObject && !uObject)
                            Debug.LogError("ArgumentNullException: Key cannot be null.");
                        else if (dict.Contains(key))
                            Debug.LogErrorFormat("An item with the same key has already been added. Key: {0}", key);
                        else
                        {
                            FieldInfo toAddValueFieldInfo = type.GetFieldInfoIncludeBase("mToAddValue", mBindingFlags);
                            object value = toAddValueFieldInfo.GetValue(targetObject);
                            FieldInfo keysFieldInfo = type.GetField("mKeys", mBindingFlags);
                            FieldInfo valuesFieldInfo = type.GetField("mValues", mBindingFlags);
                            var keys = keysFieldInfo.GetValue(targetObject) as IList;
                            var values = valuesFieldInfo.GetValue(targetObject) as IList;

                            keys.Add(key);
                            values.Add(value);
                            dict.Add(key, value);
                            toAddKeyFieldInfo.SetValue(targetObject, null);
                            toAddValueFieldInfo.SetValue(targetObject, null);
                        }
                    }
                }

                //序列化Key Value
                for (int i = 0, size = keysProperty.arraySize; i < size; i++)
                {
                    propertyHeight += 2;
                    var delRect = new Rect(valueLabelRect.xMax, position.y + propertyHeight, btnWidth, EditorGUIUtility.singleLineHeight);
                    //删除
                    bool delete = GUI.Button(delRect, "X", EditorStyles.miniButtonMid); //先显示Button，避免按钮点击被覆盖

                    var itemElementNameRect = new Rect(labelRect.x, position.y + propertyHeight, elementNameWidth, EditorGUIUtility.singleLineHeight);
                    //计算属性高度
                    float height = EditorGUIUtility.singleLineHeight;

                    var keyProperty = keysProperty.GetArrayElementAtIndex(i);
                    var keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty, true);
                    if (keyPropertyHeight > height)
                        height = keyPropertyHeight;
                    var keyRect = new Rect(keyLabelRect.x, position.y + propertyHeight, keyLabelRect.width, keyPropertyHeight);
                    if (!keyProperty.isExpanded)
                    {
                        keyProperty.isExpanded = true;
                        keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty, true);
                        keyProperty.isExpanded = false;
                    }

                    var valueProperty = valuesProperty.GetArrayElementAtIndex(i);
                    var valuePropertyHeight = EditorGUI.GetPropertyHeight(valueProperty, true);
                    if (valuePropertyHeight > height)
                        height = valuePropertyHeight;
                    var valueRect = new Rect(valueLabelRect.x, position.y + propertyHeight, valueLabelRect.width, valuePropertyHeight);
                    if (!valueProperty.isExpanded)
                    {
                        valueProperty.isExpanded = true;
                        valuePropertyHeight = EditorGUI.GetPropertyHeight(valueProperty, true);
                        valueProperty.isExpanded = false;
                    }

                    EditorGUI.LabelField(itemElementNameRect, "Item" + i);
                    var labelWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = elementNameWidth;
                    EditorGUI.PropertyField(keyRect, keyProperty,
                        keyProperty.hasVisibleChildren && keyPropertyHeight > EditorGUIUtility.singleLineHeight + 2 ? null : GUIContent.none, true);
                    EditorGUI.PropertyField(valueRect, valueProperty,
                        valueProperty.hasVisibleChildren && valuePropertyHeight > EditorGUIUtility.singleLineHeight + 2 ? null : GUIContent.none, true);
                    EditorGUIUtility.labelWidth = labelWidth;
                    propertyHeight += height;
                    if (delete)
                    {
                        Undo.RecordObject(property.serializedObject.targetObject, "Delete");
                        object targetObject = property.GetObject();
                        Type type = targetObject.GetType();
                        FieldInfo keysFieldInfo = type.GetField("mKeys", mBindingFlags);
                        FieldInfo valuesFieldInfo = type.GetField("mValues", mBindingFlags);
                        FieldInfo dictFieldInfo = type.GetField("mDictionary", mBindingFlags);

                        var keys = keysFieldInfo.GetValue(targetObject) as IList;
                        var values = valuesFieldInfo.GetValue(targetObject) as IList;
                        var dict = dictFieldInfo.GetValue(targetObject) as IDictionary;
                        dict.Remove(keys[i]);
                        keys.RemoveAt(i);
                        values.RemoveAt(i);
                        i--;
                        size--;
                    }
                }

                EditorGUI.indentLevel--;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float propertyHeight;
            SerializedProperty isAddProperty = property.FindPropertyRelative("mIsAdd");
            SerializedProperty keysProperty = property.FindPropertyRelative("mKeys");
            SerializedProperty valuesProperty = property.FindPropertyRelative("mValues");
            propertyHeight = EditorGUIUtility.singleLineHeight;//property
            if (property.isExpanded)
            {
                propertyHeight += 2;
                EditorGUI.indentLevel++;
                propertyHeight += EditorGUIUtility.singleLineHeight;//Key Value Label
                if (isAddProperty.boolValue)
                {
                    propertyHeight += 2;
                    SerializedProperty toAddKeyProperty = property.FindPropertyRelative("mToAddKey");
                    SerializedProperty toAddValueProperty = property.FindPropertyRelative("mToAddValue");
                    //计算属性高度
                    float height = EditorGUIUtility.singleLineHeight;

                    var keyPropertyHeight = EditorGUI.GetPropertyHeight(toAddKeyProperty, true);
                    if (keyPropertyHeight > height)
                        height = keyPropertyHeight;

                    var valuePropertyHeight = EditorGUI.GetPropertyHeight(toAddValueProperty, true);
                    if (valuePropertyHeight > height)
                        height = valuePropertyHeight;

                    propertyHeight += height + 2;//Item
                    propertyHeight += EditorGUIUtility.singleLineHeight;//Add Button
                }

                //序列化Key Value
                for (int i = 0, size = keysProperty.arraySize; i < size; i++)
                {
                    propertyHeight += 2;
                    //计算属性高度
                    float height = EditorGUIUtility.singleLineHeight;

                    var keyProperty = keysProperty.GetArrayElementAtIndex(i);
                    var keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty, true);
                    if (keyPropertyHeight > height)
                        height = keyPropertyHeight;

                    var valueProperty = valuesProperty.GetArrayElementAtIndex(i);
                    var valuePropertyHeight = EditorGUI.GetPropertyHeight(valueProperty, true);
                    if (valuePropertyHeight > height)
                        height = valuePropertyHeight;
                    
                    propertyHeight += height;//Item
                }
                EditorGUI.indentLevel--;
            }
            return propertyHeight;
        }
    }
}
