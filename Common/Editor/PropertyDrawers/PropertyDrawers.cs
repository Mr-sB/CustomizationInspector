using System;
using System.Collections;
using System.Reflection;
using CustomizationInspector.Runtime;
using UnityEditor;
using UnityEngine;

namespace CustomizationInspector.Editor
{
	[CustomPropertyDrawer(typeof(HideIfAttribute))]
	internal class HideIfDrawer : PropertyDrawer
	{
		protected BindingFlags mBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic |
		                                       BindingFlags.Public | BindingFlags.Static;
		protected static MemberTypes mMemberTypes = MemberTypes.Field | MemberTypes.Property | MemberTypes.Method;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (IsShow(property))
				EditorGUI.PropertyField(position, property, label, true);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (IsShow(property))
				return EditorGUI.GetPropertyHeight(property);
			return 0;
		}

		private bool IsShow(SerializedProperty property)
		{
			bool isShow = true;
			//获取对应的脚本
			var target = property.serializedObject.targetObject;

			//获取对应脚本的类型
			var	targetType = target.GetType();
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
			return isShow;
		}
	}

	[CustomPropertyDrawer(typeof(ShowIfAttribute))]
	internal class ShowIfDrawer : PropertyDrawer
	{
		protected BindingFlags mBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.Public | BindingFlags.Static;
		protected static MemberTypes mMemberTypes = MemberTypes.Field | MemberTypes.Property | MemberTypes.Method;
		
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (IsShow(property))
				EditorGUI.PropertyField(position, property, label, true);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (IsShow(property))
				return EditorGUI.GetPropertyHeight(property);
			return 0;
		}

		private bool IsShow(SerializedProperty property)
		{
			bool isShow = true;
			//获取对应的脚本
			var target = property.serializedObject.targetObject;
			//获取对应脚本的类型
			var targetType = target.GetType();
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

			return isShow;
		}
	}

	[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
	internal class ReadOnlyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			bool enabled = GUI.enabled;
			GUI.enabled = false;
			EditorGUI.PropertyField(position, property, label);
			GUI.enabled = enabled;
		}

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
	        return EditorGUI.GetPropertyHeight(property);
        }
    }

	[CustomPropertyDrawer(typeof(ValueDropdownAttribute))]
	internal class ValueDropdownDrawer : PropertyDrawer
	{
		protected BindingFlags mBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.Public | BindingFlags.Static;
		protected static MemberTypes mMemberTypes = MemberTypes.Field | MemberTypes.Property | MemberTypes.Method;
		
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			//获取对应的脚本
			var target = property.serializedObject.targetObject;
			//获取源数据数组数据
			IList targetValue = GetTargetList(property);
			if (targetValue == null)
			{
				Debug.LogError(string.Format("{0}使用出错!", attribute.GetType().ToString()), target);
				EditorGUI.PropertyField(position, property, label, true);
				return;
			}

            //目标值
			string targetFieldValue = (fieldInfo.GetValue(target) ?? string.Empty).ToString();

			int selectedIndex = -1;
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
			selectedIndex = EditorGUI.Popup(position, label, selectedIndex, displayedOptions);
			if (needInit || EditorGUI.EndChangeCheck())
			{
				try
				{
					Undo.RecordObject(target, target.name);
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
			if (GetTargetList(property) == null)
				return EditorGUI.GetPropertyHeight(property);
			return EditorGUIUtility.singleLineHeight;
		}

		private IList GetTargetList(SerializedProperty property)
		{
			//获取对应的脚本
			var target = property.serializedObject.targetObject;
			//获取对应脚本的类型
			var targetType = target.GetType();
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
			return targetValue;
		}
	}
	
	[CustomPropertyDrawer(typeof(RenameAttribute))]
	internal class RenameDrawer : PropertyDrawer
	{
		private GUIContent renameLabel;
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			string rename = (attribute as RenameAttribute).Rename;
			if (string.IsNullOrWhiteSpace(rename))
			{
				EditorGUI.PropertyField(position, property, label);
				return;
			}

			if (renameLabel == null)
				renameLabel = new GUIContent(label) {text = rename};
			EditorGUI.PropertyField(position, property, renameLabel);
		}

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
	        return EditorGUI.GetPropertyHeight(property);
        }
    }
}
