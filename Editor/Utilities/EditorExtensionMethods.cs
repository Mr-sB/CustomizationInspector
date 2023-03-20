using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

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
			Object targetObject = property.serializedObject.targetObject;
			if (!targetObject) return null;
			object obj = targetObject;
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

		//根据SerializedProperty查找到property对应的上层对象引用
		public static object GetContextObject(this SerializedProperty property)
		{
			//从最外层的property.serializedObject.targetObject(继承自UnityEngine.Object)的对象一层一层的找到目前需要绘制的对象
			string[] array = property.propertyPath.Replace("Array.data", "*Array").Split('.');
			Object targetObject = property.serializedObject.targetObject;
			if (!targetObject) return null;
			object obj = targetObject;
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
		
		//根据SerializedProperty查找到其对应的FieldInfo
		public static FieldInfo GetFieldInfo(this SerializedProperty property)
		{
			//从最外层的property.serializedObject.targetObject(继承自UnityEngine.Object)的对象一层一层的找到目前需要绘制的对象
			string[] array = property.propertyPath.Replace("Array.data", "*Array").Split('.');
			Object targetObject = property.serializedObject.targetObject;
			if (!targetObject) return null;
			object obj = targetObject;
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
		
		//由于Type.GetMember似乎不会查找到父类的私有方法，所以循环查找一下
		public static MemberInfo[] GetMemberInfoIncludeBase(this Type type, string memberName, MemberTypes memberTypes, BindingFlags bindingFlags = bindingFlags)
		{
			MemberInfo[] memberInfo = null;
			while ((memberInfo == null || memberInfo.Length == 0) && type != null)
			{
				memberInfo = type.GetMember(memberName, memberTypes, bindingFlags);
				type = type.BaseType;
			}
			return memberInfo;
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
		
		//由于Type.GetProperty似乎不会查找到父类的私有方法，所以循环查找一下
		public static PropertyInfo GetPropertyInfoIncludeBase(this Type type, string propertyName, BindingFlags bindingFlags = bindingFlags)
		{
			PropertyInfo propertyInfo = null;
			while (propertyInfo == null && type != null)
			{
				propertyInfo = type.GetProperty(propertyName, bindingFlags);
				type = type.BaseType;
			}
			return propertyInfo;
		}
		
		public static Type GetManagedReferenceFieldType(this SerializedProperty serializedProperty)
		{
			return GetManagedReferenceType(serializedProperty.managedReferenceFieldTypename);
		}
		
		/// <param name="typeName">Like: Assembly TypeFullName</param>
		/// <returns></returns>
		public static Type GetManagedReferenceType(string typeName)
		{
			int splitIndex = typeName.IndexOf(' ');
			var assembly = Assembly.Load(typeName.Substring(0,splitIndex));
			return assembly.GetType(typeName.Substring(splitIndex + 1));
		}
	}
}
