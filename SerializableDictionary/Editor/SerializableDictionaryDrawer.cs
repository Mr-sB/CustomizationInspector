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
        private float mHeight = 0;
        private Color mToggleOn = Color.green;
        private Color mToggleOff = Color.white;
        private BindingFlags mBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic |
                                               BindingFlags.Public | BindingFlags.Static;        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty isAddProperty = property.FindPropertyRelative("mIsAdd");
            SerializedProperty keysProperty = property.FindPropertyRelative("mKeys");
            SerializedProperty valuesProperty = property.FindPropertyRelative("mValues");
            SerializedProperty toAddKeyProperty = property.FindPropertyRelative("mToAddKey");
            SerializedProperty toAddValueProperty = property.FindPropertyRelative("mToAddValue");
            mHeight = EditorGUIUtility.singleLineHeight;
            float lineWidth = position.width;
            var labelRect = new Rect(position.x, position.y, lineWidth, EditorGUIUtility.singleLineHeight);
            if(EditorGUI.PropertyField(labelRect, property, false))
            {
                EditorGUI.indentLevel++;
                float elementNameWidth = 80;
                float btnWidth = 20;
                var keyLabelRect = new Rect(labelRect.x + elementNameWidth - 30, position.y + mHeight, (lineWidth - btnWidth - elementNameWidth + 30) / 2, EditorGUIUtility.singleLineHeight);
                var valueLabelRect = new Rect(keyLabelRect.xMax, position.y + mHeight, (lineWidth - btnWidth - elementNameWidth + 30) / 2, EditorGUIUtility.singleLineHeight);
                var addToggleRect = new Rect(valueLabelRect.xMax, position.y + mHeight, btnWidth, EditorGUIUtility.singleLineHeight);
                mHeight += EditorGUIUtility.singleLineHeight;
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
                if(isAddProperty.boolValue)
                {
                    //显示添加的GUI
                    var itemElementNameRect = new Rect(labelRect.x, position.y + mHeight, elementNameWidth, EditorGUIUtility.singleLineHeight);
                    //计算属性高度
                    float height = EditorGUIUtility.singleLineHeight;
                    
                    var propertyHeight = EditorGUIUtility.singleLineHeight;
                    if (toAddKeyProperty.isExpanded)
                    {
                        propertyHeight = EditorGUI.GetPropertyHeight(toAddKeyProperty, true);
                        if (propertyHeight > height)
                            height = propertyHeight;
                    }
                    var keyRect = new Rect(keyLabelRect.x, position.y + mHeight, keyLabelRect.width, propertyHeight);
                    
                    propertyHeight = EditorGUIUtility.singleLineHeight;
                    if (toAddValueProperty.isExpanded)
                    {
                        propertyHeight = EditorGUI.GetPropertyHeight(toAddValueProperty, true);
                        if (propertyHeight > height)
                            height = propertyHeight;
                    }
                    
                    var valueRect = new Rect(valueLabelRect.x, position.y + mHeight, valueLabelRect.width, propertyHeight);
                    mHeight += height;
                    var addRect = new Rect(labelRect.x, position.y + mHeight, lineWidth, EditorGUIUtility.singleLineHeight);
                    mHeight += EditorGUIUtility.singleLineHeight;
                    EditorGUI.LabelField(itemElementNameRect, "AddItem");
                    //修改label宽度，避免太长
                    var lastLabelWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = elementNameWidth;
                    if (toAddKeyProperty.hasVisibleChildren)
                    {
                        keyRect.width = 500;//width太小的话显示不了，手动放大
                        EditorGUI.PropertyField(keyRect, toAddKeyProperty, new GUIContent("To Add Key"), true);
                    }
                    else
                        EditorGUI.PropertyField(keyRect, toAddKeyProperty, GUIContent.none, true);

                    if (toAddValueProperty.hasVisibleChildren)
                    {
                        valueRect.width = 500;
                        EditorGUI.PropertyField(valueRect, toAddValueProperty, new GUIContent("To Add Value"), true);
                    }
                    else
                        EditorGUI.PropertyField(valueRect, toAddValueProperty, GUIContent.none, true);
                    EditorGUIUtility.labelWidth = lastLabelWidth;

                    if (GUI.Button(addRect, "Add"))
                    {
                        Undo.RecordObject(property.serializedObject.targetObject, "Add");
                        object targetObject = property.GetObject();
                        Type type = targetObject.GetType();
                        FieldInfo toAddKeyFieldInfo = type.GetFieldInfoIncludeBase("mToAddKey", mBindingFlags);
                        FieldInfo toAddValueFieldInfo = type.GetFieldInfoIncludeBase("mToAddValue", mBindingFlags);
                        FieldInfo dictFieldInfo = type.GetField("mDictionary", mBindingFlags);
                        
                        var dict = dictFieldInfo.GetValue(targetObject) as IDictionary;
                        object key = toAddKeyFieldInfo.GetValue(targetObject);
                        object value = toAddValueFieldInfo.GetValue(targetObject);
                        if (dict.Contains(key))
                        {
                            Debug.LogErrorFormat("An item with the same key has already been added. Key: {0}", key);
                        }
                        else
                        {
                            FieldInfo keysFieldInfo = type.GetField("mKeys", mBindingFlags);
                            FieldInfo valuesFieldInfo = type.GetField("mValues", mBindingFlags);
                            var keys = keysFieldInfo.GetValue(targetObject) as IList;
                            var values = valuesFieldInfo.GetValue(targetObject) as IList;
                            
                            keys.Add(key);
                            values.Add(value);
                            dict.Add(key, value);
                        }
                        toAddKeyFieldInfo.SetValue(targetObject, null);
                        toAddValueFieldInfo.SetValue(targetObject, null);
                    }
                }
                //序列化Key Value
                for (int i = 0, size = keysProperty.arraySize; i < size; i++)
                {
                    var delRect = new Rect(valueLabelRect.xMax, position.y + mHeight, btnWidth, EditorGUIUtility.singleLineHeight);
                    //删除
                    bool delete = GUI.Button(delRect, "X", EditorStyles.miniButtonMid);//先显示Button，避免按钮点击被覆盖

                    var itemElementNameRect = new Rect(labelRect.x, position.y + mHeight, elementNameWidth, EditorGUIUtility.singleLineHeight);
                    //计算属性高度
                    float height = EditorGUIUtility.singleLineHeight;
                    
                    var propertyHeight = EditorGUIUtility.singleLineHeight;
                    if (keysProperty.GetArrayElementAtIndex(i).isExpanded)
                    {
                        propertyHeight = EditorGUI.GetPropertyHeight(keysProperty.GetArrayElementAtIndex(i), true);
                        if (propertyHeight > height)
                            height = propertyHeight;
                    }
                    var keyRect = new Rect(keyLabelRect.x, position.y + mHeight, keyLabelRect.width, propertyHeight);
                    
                    propertyHeight = EditorGUIUtility.singleLineHeight;
                    if (valuesProperty.GetArrayElementAtIndex(i).isExpanded)
                    {
                        propertyHeight = EditorGUI.GetPropertyHeight(valuesProperty.GetArrayElementAtIndex(i), true);
                        if (propertyHeight > height)
                            height = propertyHeight;
                    }
                    var valueRect = new Rect(valueLabelRect.x, position.y + mHeight, valueLabelRect.width, propertyHeight);
                    
                    EditorGUI.LabelField(itemElementNameRect, "Item" + i);
                    //修改label宽度，避免太长
                    var lastLabelWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = elementNameWidth;
                    if (keysProperty.GetArrayElementAtIndex(i).hasVisibleChildren)
                    {
                        keyRect.width = 500;//width太小的话显示不了，手动放大
                        EditorGUI.PropertyField(keyRect, keysProperty.GetArrayElementAtIndex(i), true);
                    }
                    else
                        EditorGUI.PropertyField(keyRect, keysProperty.GetArrayElementAtIndex(i), GUIContent.none, true);

                    if (valuesProperty.GetArrayElementAtIndex(i).hasVisibleChildren)
                    {
                        valueRect.width = 500;
                        EditorGUI.PropertyField(valueRect, valuesProperty.GetArrayElementAtIndex(i), true);
                    }
                    else
                        EditorGUI.PropertyField(valueRect, valuesProperty.GetArrayElementAtIndex(i), GUIContent.none, true);
                    EditorGUIUtility.labelWidth = lastLabelWidth;

                    mHeight += height;
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
            return mHeight;
        }
    }
}
