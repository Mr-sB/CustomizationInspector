using System;
using System.Collections;
using System.Reflection;
using CustomizationInspector.Runtime;
using UnityEditor;
using UnityEngine;

namespace CustomizationInspector.Editor
{
	//EditorGUILayout.PropertyField能正确计算出位置和高度
	//需要精细控制的再用EditorGUI.PropertyField
	
	[CustomPropertyDrawer(typeof(HideIfAttribute))]
	internal class HideIfDrawer : PropertyDrawer
	{
		protected BindingFlags mBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic |
		                                       BindingFlags.Public | BindingFlags.Static;
		protected static MemberTypes mMemberTypes = MemberTypes.Field | MemberTypes.Property | MemberTypes.Method;
		UnityEngine.Object target;
		Type targetType;
		bool isShow;
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			isShow = true;
			//获取对应的脚本
			if (target == null)
			{
				target = property.serializedObject.targetObject;
			}

			//获取对应脚本的类型
			if (targetType == null)
				targetType = target.GetType();
			//获取判断的名称
			var hideIfAttribute = attribute as HideIfAttribute;
			MemberInfo[] memberInfos = targetType.GetMember(hideIfAttribute.MemberName, mMemberTypes, mBindingFlags);
			for (int i = 0, len = memberInfos.Length; i < len; i++)
			{
				try
				{
					switch (memberInfos[i].MemberType)
					{
						case MemberTypes.Field:
							isShow = !(bool)((memberInfos[i] as FieldInfo)?.GetValue(target) ?? !isShow);
							break;
						case MemberTypes.Property:
							isShow = !(bool)((memberInfos[i] as PropertyInfo)?.GetValue(target) ?? !isShow);
							break;
						case MemberTypes.Method:
							isShow = !(bool)((memberInfos[i] as MethodInfo)?.Invoke(target, hideIfAttribute.Objs) ?? !isShow);
							break;
					}
				}
				catch (Exception e)
				{
					Debug.LogError(e);
				}
			}
			if (isShow)
			{
				EditorGUILayout.PropertyField(property, label, true);
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return 0;
		}
	}

	[CustomPropertyDrawer(typeof(ShowIfAttribute))]
	internal class ShowIfDrawer : PropertyDrawer
	{
		protected BindingFlags mBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.Public | BindingFlags.Static;
		protected static MemberTypes mMemberTypes = MemberTypes.Field | MemberTypes.Property | MemberTypes.Method;
		UnityEngine.Object target;
		Type targetType;
		bool isShow;
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			isShow = true;
			//获取对应的脚本
			if (target == null)
				target = property.serializedObject.targetObject;
			//获取对应脚本的类型
			if (targetType == null)
				targetType = target.GetType();
			//获取判断的名称
			var showIfAttribute = attribute as ShowIfAttribute;
			MemberInfo[] memberInfos = targetType.GetMember(showIfAttribute.MemberName, mMemberTypes, mBindingFlags);
			for (int i = 0, len = memberInfos.Length; i < len; i++)
			{
				try
				{
					switch (memberInfos[i].MemberType)
					{
						case MemberTypes.Field:
							isShow = (bool)((memberInfos[i] as FieldInfo)?.GetValue(target) ?? isShow);
							break;
						case MemberTypes.Property:
							isShow = (bool)((memberInfos[i] as PropertyInfo)?.GetValue(target) ?? isShow);
							break;
						case MemberTypes.Method:
							isShow = (bool)((memberInfos[i] as MethodInfo)?.Invoke(target, showIfAttribute.Objs) ?? isShow);
							break;
					}
				}
				catch (Exception e)
				{
					Debug.LogError(e);
				}
			}
			if (isShow)
			{
				EditorGUILayout.PropertyField(property, label, true);
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return 0;
		}
	}

	[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
	internal class ReadOnlyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			bool enabled = GUI.enabled;
			GUI.enabled = false;
			EditorGUILayout.PropertyField(property, label);
			GUI.enabled = enabled;
		}

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 0;
        }
    }

	[CustomPropertyDrawer(typeof(ValueDropdownAttribute))]
	internal class ValueDropdownDrawer : PropertyDrawer
	{
		protected BindingFlags mBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.Public | BindingFlags.Static;
		protected static MemberTypes mMemberTypes = MemberTypes.Field | MemberTypes.Property | MemberTypes.Method;
		UnityEngine.Object target;
		Type targetType;
		int selectedIndex = -1;
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			//获取对应的脚本
			if (target == null)
				target = property.serializedObject.targetObject;
			//获取对应脚本的类型
			if (targetType == null)
				targetType = target.GetType();
			//获取源数据数组数据
			IList targetValue = null;
			ValueDropdownAttribute valueDropdownAttribute = attribute as ValueDropdownAttribute;
			MemberInfo[] memberInfos = targetType.GetMember(valueDropdownAttribute.MemberName, mMemberTypes, mBindingFlags);
			for (int i = 0, len = memberInfos.Length; i < len; i++)
			{
				try
				{
					switch (memberInfos[i].MemberType)
					{
						case MemberTypes.Field:
							targetValue = (IList) (memberInfos[i] as FieldInfo)?.GetValue(target);
							break;
						case MemberTypes.Property:
							targetValue = (IList) (memberInfos[i] as PropertyInfo)?.GetValue(target);
							break;
						case MemberTypes.Method:
							targetValue = (IList) (memberInfos[i] as MethodInfo)?.Invoke(target, valueDropdownAttribute.Objs);
							break;
					}
				}
				catch (Exception e)
				{
					Debug.LogError(e);
				}
			}
			if (targetValue == null)
			{
				Debug.LogError(string.Format("{0}使用出错!", attribute.GetType().ToString()), target);
				EditorGUILayout.PropertyField(property, label, true);
				return;
			}

            //目标值
			string targetFieldValue = fieldInfo.GetValue(target).ToString();

			GUIContent[] displayedOptions = new GUIContent[targetValue.Count];
			for (int i = 0; i < displayedOptions.Length; i++)
			{
				displayedOptions[i] = new GUIContent(targetValue[i].ToString());
				if (displayedOptions[i].text == targetFieldValue)
					selectedIndex = i;
			}
            bool needInit = selectedIndex == -1;
            if (needInit)
                selectedIndex = 0;
			EditorGUI.BeginChangeCheck();
			selectedIndex = EditorGUILayout.Popup(label, selectedIndex, displayedOptions);
			if (needInit || EditorGUI.EndChangeCheck())
			{
				try
				{
					fieldInfo.SetValue(target, targetValue[selectedIndex]);
				}
				catch (Exception e)
				{
					Debug.LogError(string.Format("{0}使用出错!", attribute.GetType().ToString()), target);
					Debug.LogError(e.Message, target);
				}
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return 0;
		}
	}
	
	[CustomPropertyDrawer(typeof(RenameAttribute))]
	internal class RenameDrawer : PropertyDrawer
	{
		private GUIContent renameLable;
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			string rename = (attribute as RenameAttribute).Rename;
			if (string.IsNullOrWhiteSpace(rename))
			{
				EditorGUILayout.PropertyField(property, label);
				return;
			}

			if (renameLable == null)
				renameLable = new GUIContent(label) {text = rename};
			EditorGUILayout.PropertyField(property, renameLable);
		}

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 0;
        }
    }
	
	[CustomPropertyDrawer(typeof(MinMaxAttribute))]
	internal class MinMaxDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (!property.type.Equals(nameof(Vector2)))
			{
				EditorGUILayout.PropertyField(property, label, true);
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
	        if (!property.type.Equals(nameof(Vector2))) return 0;
            return EditorGUIUtility.singleLineHeight * 2;
        }
	}
}
