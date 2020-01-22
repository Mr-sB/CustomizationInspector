using CustomizationInspector.Runtime;
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CustomizationInspector.Editor
{
	[CustomEditor(typeof(MonoBehaviour), true)]
	[CanEditMultipleObjects]
	public class CustomizationMonoBehaviourEditor : UnityEditor.Editor
	{
		private Type targetType;
		public static BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.Public | BindingFlags.Static;

		private void OnEnable()
		{
			if (targetType == null)
				targetType = target.GetType();
		}

		public override void OnInspectorGUI()
		{
			//利用源码序列化
			//EditorGUI.BeginChangeCheck();
			//serializedObject.Update();

			//// Loop through properties and create one field (including children) for each top level property.
			//SerializedProperty property = serializedObject.GetIterator();
			//bool expanded = true;
			//while (property.NextVisible(expanded))
			//{
			//	using (new EditorGUI.DisabledScope("m_Script" == property.propertyPath))
			//	{
			//		EditorGUILayout.PropertyField(property, true);
			//	}
			//	expanded = false;
			//}

			//MethodAttribute(targetType, target);
			//serializedObject.ApplyModifiedProperties();
			//EditorGUI.EndChangeCheck();
			
			//利用base序列化
			base.OnInspectorGUI();
			MethodAttribute(targetType, target);
		}

		public static void MethodAttribute(Type targetType, object target)
		{
			MethodInfo[] methodInfos = targetType.GetMethods(flag);
			Type attributeType;
			foreach (var info in methodInfos)
			{
				object[] attributes = info.GetCustomAttributes(true);
				foreach (Attribute attribute in attributes)
				{
					attributeType = attribute.GetType();
					switch (attributeType.Name)
					{
						case nameof(ButtonAttribute)://nameof(ButtonAttribute) Equals "ButtonAttribute"
							string desc = (attribute as ButtonAttribute).ShowName;
							if (desc == null)
								desc = info.Name;
							if (GUILayout.Button(desc))
							{
								try
								{
									info.Invoke(target, null);
								}
								catch(Exception e)
								{
									if (target is UnityEngine.Object)
										Debug.LogError(string.Format("不能将ButtonAttribute用于修饰有参数的函数{0}!", info.Name), target as UnityEngine.Object);
									else
										Debug.LogError(string.Format("不能将ButtonAttribute用于修饰有参数的函数{0}!", info.Name));
									Debug.LogError(e);
								}
							}
							break;
					}
				}
			}
		}
	}
}