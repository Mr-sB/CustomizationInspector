using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CustomizationInspector.Editor
{
	public static class EditorExtensionMethods
	{
		private const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

		//根据SerializedProperty查找到其对应的对象引用
		public static object GetObject(this SerializedProperty property)
		{
			//从最外层的property.serializedObject.targetObject(继承自UnityEngine.Object)的对象一层一层的找到目前需要绘制的对象
			string[] array = property.propertyPath.Replace("Array.data", "*Array").Split('.');
			object obj = property.serializedObject.targetObject;
			for (int i = 0; i < array.Length; i++)
			{
				if (!array[i].StartsWith("*Array"))
				{
					var fieldInfo = obj.GetType().GetFieldInfoIncludeBase(array[i]);
					if (fieldInfo == null)
						return null;
					obj = fieldInfo.GetValue(obj);
				}
				else
				{
					int index = int.Parse(array[i].Substring(7, array[i].Length - 8));
					obj = (obj as IList)[index];
				}
			}
			return obj;
		}
		
		//根据SerializedProperty查找到其对应的FieldInfo
		public static FieldInfo GetFieldInfo(this SerializedProperty property)
		{
			//从最外层的property.serializedObject.targetObject(继承自UnityEngine.Object)的对象一层一层的找到目前需要绘制的对象
			string[] array = property.propertyPath.Replace("Array.data", "*Array").Split('.');
			object obj = property.serializedObject.targetObject;
			FieldInfo fieldInfo = null;
			for (int i = 0; i < array.Length; i++)
			{
				if (!array[i].StartsWith("*Array"))
				{
					fieldInfo = obj.GetType().GetFieldInfoIncludeBase(array[i]);
					if (fieldInfo == null)
						return null;
					obj = fieldInfo.GetValue(obj);
				}
				else
				{
					int index = int.Parse(array[i].Substring(7, array[i].Length - 8));
					obj = (obj as IList)[index];
				}
			}
			return fieldInfo;
		}
		
		//根据SerializedProperty查找到List data对应的对象引用
		public static object GetListObjectContext(this SerializedProperty property)
		{
			//从最外层的property.serializedObject.targetObject(继承自UnityEngine.Object)的对象一层一层的找到目前需要绘制的对象
			string[] array = property.propertyPath.Replace("Array.data", "*Array").Split('.');
			object obj = property.serializedObject.targetObject;
			for (int i = 0; i < array.Length - 1; i++)
			{
				if (!array[i].StartsWith("*Array"))
					obj = obj.GetType().GetFieldInfoIncludeBase(array[i]).GetValue(obj);
				else
				{
					int index = int.Parse(array[i].Substring(7, array[i].Length - 8));
					obj = (obj as IList)[index];
				}
			}
			return obj;
		}

		//根据SerializedProperty查找到List data对应的对象引用
		public static SerializedProperty GetListPropertyContext(this SerializedProperty property)
		{
			var path = property.propertyPath;
			try
			{
				path = path.Substring(0, path.LastIndexOf(".Array.data"));
				return property.serializedObject.FindProperty(path);
			}
			catch (Exception e)
			{
				Debug.LogError(e);
				return null;
			}
		}

		//由于Type.GetMethod似乎不会查找到父类的私有方法，所以循环查找一下
		public static MethodInfo GetMethodInfoIncludeBase(this Type type, string methodName, BindingFlags bindingFlags = bindingFlags)
		{
			MethodInfo methodInfo = null;
			while (methodInfo == null && type != null)
			{
				methodInfo = type.GetMethod(methodName, bindingFlags);
				type = type.BaseType;
			}
			return methodInfo;
		}

		//由于Type.GetField似乎不会查找到父类的私有字段，所以循环查找一下
		public static FieldInfo GetFieldInfoIncludeBase(this Type type, string fieldName, BindingFlags bindingFlags = bindingFlags)
		{
			FieldInfo fieldInfo = null;
			while (fieldInfo == null && type != null)
			{
				fieldInfo = type.GetField(fieldName, bindingFlags);
				type = type.BaseType;
			}
			return fieldInfo;
		}
	}
}
