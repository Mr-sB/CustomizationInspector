using System;
using System.Collections;
using System.IO;
using System.Reflection;
using CustomizationInspector.Runtime;
using UnityEditor;
using UnityEngine;

namespace CustomizationInspector.Editor
{
	[CustomPropertyDrawer(typeof(ShowIfAttribute))]
	internal class ShowIfDrawer : PropertyDrawer
	{
		protected BindingFlags mBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic |
		                                       BindingFlags.Public | BindingFlags.Static;
		protected static MemberTypes mMemberTypes = MemberTypes.Field | MemberTypes.Property | MemberTypes.Method;
		private MemberInfo[] memberInfos;
		protected virtual bool defaultValue => true;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (IsShow(property))
				EditorGUI.PropertyField(position, property, label, true);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (IsShow(property))
				return EditorGUI.GetPropertyHeight(property, label, true);
			return 0;
		}

		protected virtual bool IsShow(SerializedProperty property)
		{
			bool isShow = defaultValue;
			//获取对应的对象
			var target = property.GetContextObject();
			var targetType = target.GetType();
			//获取判断的名称
			var showIfAttribute = attribute as ShowIfAttribute;
			if (memberInfos == null)
				memberInfos = targetType.GetMemberInfoIncludeBase(showIfAttribute.MemberName, mMemberTypes, mBindingFlags);
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
	
	[CustomPropertyDrawer(typeof(HideIfAttribute))]
	internal class HideIfDrawer : ShowIfDrawer
	{
		protected override bool defaultValue => false;

		protected override bool IsShow(SerializedProperty property)
		{
			return !base.IsShow(property);
		}
	}

	[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
	internal class ReadOnlyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			bool enabled = GUI.enabled;
			GUI.enabled = false;
			EditorGUI.PropertyField(position, property, label, true);
			GUI.enabled = enabled;
		}

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
	        return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }

	[CustomPropertyDrawer(typeof(ValueDropdownAttribute))]
	internal class ValueDropdownDrawer : PropertyDrawer
	{
		protected const int ErrorBoxHeight = 24;
		protected const int ErrorBoxSpace = 2;
		protected BindingFlags mBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.Public | BindingFlags.Static;
		protected static MemberTypes mMemberTypes = MemberTypes.Field | MemberTypes.Property | MemberTypes.Method;
		private MemberInfo[] memberInfos;
		private GUIContent[] displayedOptions;
		
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			//获取对应的对象
			var target = property.GetContextObject();
			//获取源数据数组数据
			IList targetValue = GetTargetList(property);
			if (targetValue == null)
			{
				Rect errorBoxPosition = EditorGUI.IndentedRect(new Rect(position));
				errorBoxPosition.height = ErrorBoxHeight;
				EditorGUI.HelpBox(errorBoxPosition, string.Format("{0}使用出错!", attribute.GetType().Name), UnityEditor.MessageType.Error);
				position.yMin += ErrorBoxHeight + ErrorBoxSpace;
				EditorGUI.PropertyField(position, property, label, true);
				return;
			}

            //目标值
			string targetFieldValue = (fieldInfo.GetValue(target) ?? string.Empty).ToString();

			int selectedIndex = -1;
			bool isNewDisplayedOptions = false;
			if (displayedOptions == null || displayedOptions.Length != targetValue.Count)
			{
				isNewDisplayedOptions = true;
				displayedOptions = new GUIContent[targetValue.Count];
			}
			for (int i = 0; i < displayedOptions.Length; i++)
			{
				if (isNewDisplayedOptions)
					displayedOptions[i] = new GUIContent(targetValue[i].ToString());
				else
					displayedOptions[i].text = targetValue[i].ToString();
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
					Undo.RecordObject(property.serializedObject.targetObject, property.serializedObject.targetObject.name);
					fieldInfo.SetValue(target, targetValue[selectedIndex]);
				}
				catch (Exception e)
				{
					Debug.LogError(string.Format("{0}使用出错!", attribute.GetType().ToString()), property.serializedObject.targetObject);
					Debug.LogError(e.Message, property.serializedObject.targetObject);
				}
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (GetTargetList(property) == null)
				return EditorGUI.GetPropertyHeight(property, label, true) + ErrorBoxHeight + ErrorBoxSpace;
			return EditorGUIUtility.singleLineHeight;
		}

		private IList GetTargetList(SerializedProperty property)
		{
			//获取对应的对象
			var target = property.GetContextObject();
			var targetType = target.GetType();
			//获取源数据数组数据
			IList targetValue = null;
			ValueDropdownAttribute valueDropdownAttribute = attribute as ValueDropdownAttribute;
			if (memberInfos == null)
				memberInfos = targetType.GetMemberInfoIncludeBase(valueDropdownAttribute.MemberName, mMemberTypes, mBindingFlags);
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
				EditorGUI.PropertyField(position, property, label, true);
				return;
			}

			if (renameLabel == null)
				renameLabel = new GUIContent(label) {text = rename};
			EditorGUI.PropertyField(position, property, renameLabel, true);
		}

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
	        return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }

	internal class PathDrawer : PropertyDrawer
	{
		public const int BrowseButtonWidth = 18;
		public const int OpenButtonWidth = 18;
		public const int Space = 2;
		
		private static readonly GUIContent browseContent = EditorGUIUtility.IconContent("d_Folder Icon");
		private static readonly GUIContent openContent = EditorGUIUtility.IconContent("d_FolderOpened Icon");
		private static readonly GUIStyle buttonStyle = GUI.skin.FindStyle("ColorPickerBox") ??
		                                               EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle("ColorPickerBox") ??
		                                               GUIStyle.none;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (fieldInfo.FieldType != typeof(string))
			{
				EditorGUI.PropertyField(position, property, label, true);
				return;
			}
			DrawPath(position, property, label);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property, label, true);
		}
		
		public void DrawPath(Rect position, SerializedProperty property, GUIContent label)
		{
			if (attribute is not PathAttribute pathAttribute) return;
			Rect propertyPosition = new Rect(position);
			if (pathAttribute.Browse)
				propertyPosition.width -= Space + BrowseButtonWidth;
			if (pathAttribute.Open)
				propertyPosition.width -= Space + OpenButtonWidth;
			EditorGUI.PropertyField(propertyPosition, property, label, true);
			if (pathAttribute.Draggable)
			{
				if (Event.current.type == EventType.DragUpdated && propertyPosition.Contains(Event.current.mousePosition))
					DragAndDrop.visualMode = DragAndDropVisualMode.Link;
				else if (Event.current.type == EventType.DragPerform && DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
				{
					if (propertyPosition.Contains(Event.current.mousePosition) && DragAndDrop.paths[0] != null)
					{
						string path = DragAndDrop.paths[0];
						if (!pathAttribute.IsFile && File.Exists(path))
							path = Path.GetDirectoryName(path);
						property.stringValue = pathAttribute.GetRelativePath(path);
						Event.current.Use();
					}
				}
			}
			
			if (pathAttribute.Browse)
			{
				Rect browsePosition = new Rect(position);
				if (pathAttribute.Open)
				{
					browsePosition.width = BrowseButtonWidth;
					browsePosition.x += propertyPosition.width + Space;
				}
				else
					browsePosition.xMin = browsePosition.xMax - OpenButtonWidth;
				//Avoid ReadOnlyAttribute make button can not click.
				bool enabled = GUI.enabled;
				GUI.enabled = true;
				if (GUI.Button(browsePosition, browseContent, buttonStyle))
				{
					string path;
					if (pathAttribute.IsFile)
						path = EditorUtility.OpenFilePanel(label.text, pathAttribute.GetAbsolutePath(property.stringValue), "");
					else
						path = EditorUtility.OpenFolderPanel(label.text, pathAttribute.GetAbsolutePath(property.stringValue), "");
					if (!string.IsNullOrEmpty(path))
						property.stringValue = pathAttribute.GetRelativePath(path);
				}
				GUI.enabled = enabled;
			}
			if (pathAttribute.Open)
			{
				Rect openPosition = new Rect(position);
				openPosition.xMin = openPosition.xMax - OpenButtonWidth;
				//Avoid ReadOnlyAttribute make button can not click.
				bool enabled = GUI.enabled;
				GUI.enabled = true;
				if (GUI.Button(openPosition, openContent, buttonStyle))
					EditorUtility.RevealInFinder(pathAttribute.GetAbsolutePath(property.stringValue));
				GUI.enabled = enabled;
			}
		}
	}

	[CustomPropertyDrawer(typeof(Runtime.FilePathAttribute))]
	internal class FilePathDrawer : PathDrawer
	{
	}
	
	[CustomPropertyDrawer(typeof(Runtime.FolderPathAttribute))]
	internal class FolderPathDrawer : PathDrawer
	{
	}
	
	[CustomPropertyDrawer(typeof(LabelWidthAttribute))]
	internal class LabelWidthDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var oldLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = attribute is LabelWidthAttribute labelWidthAttribute && labelWidthAttribute.LabelWidth >= 0
				? labelWidthAttribute.LabelWidth
				: EditorGUIUtility.labelWidth;
			EditorGUI.PropertyField(position, property, label, true);
			EditorGUIUtility.labelWidth = oldLabelWidth;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property, label, true);
		}
	}
}
