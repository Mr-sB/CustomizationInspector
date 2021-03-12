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
			//Apply to all targets.
			MethodAttribute(targetType, targets);
		}

		public static void MethodAttribute(Type targetType, object[] targets)
		{
			MethodInfo[] methodInfos = targetType.GetMethods(flag);
			foreach (var info in methodInfos)
			{
				object[] attributes = info.GetCustomAttributes(true);
				foreach (Attribute attribute in attributes)
				{
					if (attribute is ButtonAttribute buttonAttribute)
					{
						string desc = buttonAttribute.ShowName ?? info.Name;
						if (GUILayout.Button(desc))
						{
							foreach (var target in targets)
							{
								try
								{
									info.Invoke(target, buttonAttribute.Params);
								}
								catch (Exception e)
								{
									if (target is UnityEngine.Object context)
										Debug.LogError(e, context);
									else
										Debug.LogError(e);
								}
							}
						}
					}
				}
			}
		}
	}
}