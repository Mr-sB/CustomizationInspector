using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CustomizationInspector.Editor
{
	public static class FieldInspector
	{
		private const string EditorPrefsFoldoutKey = "CustomizationInspector.Editor.Foldout.{0}.{1}";
		private const string SavedExpandedSaveKey = "CustomizationInspector.Editor.Foldout.SaveKey";
		private static HashSet<string> foldoutSavedKeys = new HashSet<string>();

		private static GUIContent tmpContent = new GUIContent();
		private static string[] layerNames;

		private static int[] maskValues;

		private static GUIContent TempContent(string text)
		{
			tmpContent.text = text;
			return tmpContent;
		}

		#region Foldout
		[InitializeOnLoadMethod]
		private static void Initialize()
		{
			AssemblyReloadEvents.beforeAssemblyReload -= BeforeAssemblyReload;
			AssemblyReloadEvents.beforeAssemblyReload += BeforeAssemblyReload;
			AssemblyReloadEvents.afterAssemblyReload -= AfterAssemblyReload;
			AssemblyReloadEvents.afterAssemblyReload += AfterAssemblyReload;
			EditorApplication.quitting -= OnApplicationQuite;
			EditorApplication.quitting += OnApplicationQuite;
		}

		private static void BeforeAssemblyReload()
		{
			//Save before reload
			string value = string.Join(',', foldoutSavedKeys);
			foldoutSavedKeys.Clear();
			EditorPrefs.SetString(SavedExpandedSaveKey, value);
		}

		private static void AfterAssemblyReload()
		{
			//Load after reload
			foreach (string key in EditorPrefs.GetString(SavedExpandedSaveKey, "").Split(',', StringSplitOptions.RemoveEmptyEntries))
			{
				if (!foldoutSavedKeys.Contains(key))
					foldoutSavedKeys.Add(key);
			}
			EditorPrefs.DeleteKey(SavedExpandedSaveKey);
		}
		
		private static void OnApplicationQuite()
		{
			//Delete key when application quite
			foreach (var savedKey in foldoutSavedKeys)
				EditorPrefs.DeleteKey(savedKey);
			EditorPrefs.DeleteKey(SavedExpandedSaveKey);
		}

		public static bool DrawFoldout(string content, Object target, bool toggleOnLabelClick = false)
		{
			int hashCode = 0;
			if (target)
				hashCode = target.GetInstanceID();
			return DrawFoldout(content, hashCode, toggleOnLabelClick);
		}
		
		public static bool DrawFoldout(string content, int hashCode, bool toggleOnLabelClick = false)
		{
			return DrawFoldout(LoadFoldoutExpand(GetFoldoutSaveKey(content, hashCode)), content, hashCode, toggleOnLabelClick);
		}
		
		public static bool DrawFoldout(bool expand, string content, Object target, bool toggleOnLabelClick = false)
		{
			int hashCode = 0;
			if (target)
				hashCode = target.GetInstanceID();
			return DrawFoldout(expand, content, hashCode, toggleOnLabelClick);
		}
		
		public static bool DrawFoldout(bool expand, string content, int hashCode, bool toggleOnLabelClick = false)
		{
			string key = GetFoldoutSaveKey(content, hashCode);
			bool value = EditorGUILayout.Foldout(expand, content, toggleOnLabelClick);
			if (value != expand)
				SaveFoldoutExpand(key, value);
			return value;
		}
		
		public static bool DrawFoldout(Rect position, string content, Object target, bool toggleOnLabelClick = false)
		{
			int hashCode = 0;
			if (target)
				hashCode = target.GetInstanceID();
			return DrawFoldout(position, content, hashCode, toggleOnLabelClick);
		}
		
		public static bool DrawFoldout(Rect position, string content, int hashCode, bool toggleOnLabelClick = false)
		{
			return DrawFoldout(position, LoadFoldoutExpand(GetFoldoutSaveKey(content, hashCode)), content, hashCode, toggleOnLabelClick);
		}
		
		public static bool DrawFoldout(Rect position, bool expand, string content, Object target, bool toggleOnLabelClick = false)
		{
			int hashCode = 0;
			if (target)
				hashCode = target.GetInstanceID();
			return DrawFoldout(position, expand, content, hashCode, toggleOnLabelClick);
		}
		
		public static bool DrawFoldout(Rect position, bool expand, string content, int hashCode, bool toggleOnLabelClick = false)
		{
			string key = GetFoldoutSaveKey(content, hashCode);
			bool value = EditorGUI.Foldout(position, expand, content, toggleOnLabelClick);
			if (value != expand)
				SaveFoldoutExpand(key, value);
			return value;
		}
		
		public static bool DrawFoldoutHeader(string content, Object target, Action drawInside)
		{
			int hashCode = 0;
			if (target)
				hashCode = target.GetInstanceID();
			return DrawFoldoutHeader(content, hashCode, drawInside);
		}
		
		public static bool DrawFoldoutHeader(string content, int hashCode, Action drawInside)
		{
			return DrawFoldoutHeader(LoadFoldoutExpand(GetFoldoutSaveKey(content, hashCode)), content, hashCode, drawInside);
		}
		
		public static bool DrawFoldoutHeader(bool expand, string content, Object target, Action drawInside)
		{
			int hashCode = 0;
			if (target)
				hashCode = target.GetInstanceID();
			return DrawFoldoutHeader(expand, content, hashCode, drawInside);
		}
		
		public static bool DrawFoldoutHeader(bool expand, string content, int hashCode, Action drawInside)
		{
			string key = GetFoldoutSaveKey(content, hashCode);
			bool value = EditorGUILayout.BeginFoldoutHeaderGroup(expand, content);
			EditorGUILayout.EndFoldoutHeaderGroup();
			if (value != expand)
				SaveFoldoutExpand(key, value);
			if (value && drawInside != null)
			{
				EditorGUI.indentLevel++;
				drawInside();
				EditorGUI.indentLevel--;
			}
			return value;
		}
		
		public static bool DrawFoldoutHeader(Rect position, string content, Object target, Action drawInside)
		{
			int hashCode = 0;
			if (target)
				hashCode = target.GetInstanceID();
			return DrawFoldoutHeader(position, content, hashCode, drawInside);
		}
		
		public static bool DrawFoldoutHeader(Rect position, string content, int hashCode, Action drawInside)
		{
			return DrawFoldoutHeader(position, LoadFoldoutExpand(GetFoldoutSaveKey(content, hashCode)), content, hashCode, drawInside);
		}
		
		public static bool DrawFoldoutHeader(Rect position, bool expand, string content, Object target, Action drawInside)
		{
			int hashCode = 0;
			if (target)
				hashCode = target.GetInstanceID();
			return DrawFoldoutHeader(position, expand, content, hashCode, drawInside);
		}
		
		public static bool DrawFoldoutHeader(Rect position, bool expand, string content, int hashCode, Action drawInside)
		{
			string key = GetFoldoutSaveKey(content, hashCode);
			bool value = EditorGUI.BeginFoldoutHeaderGroup(position, expand, content);
			EditorGUI.EndFoldoutHeaderGroup();
			if (value != expand)
				SaveFoldoutExpand(key, value);
			if (value && drawInside != null)
			{
				EditorGUI.indentLevel++;
				drawInside();
				EditorGUI.indentLevel--;
			}
			return value;
		}

		public static string GetFoldoutSaveKey(string text, Object target)
		{
			return GetFoldoutSaveKey(text, target.GetInstanceID());
		}
		
		public static string GetFoldoutSaveKey(string text, int hashCode = 0)
		{
			return string.Format(EditorPrefsFoldoutKey, text, hashCode);
		}

		public static void SaveFoldoutExpand(string key, bool expand)
		{
			if (foldoutSavedKeys.Contains(key))
				foldoutSavedKeys.Add(key);
			EditorPrefs.SetBool(key, expand);
		}

		public static bool LoadFoldoutExpand(string key, bool defaultValue = true)
		{
			return EditorPrefs.GetBool(key, defaultValue);
		}
		
		#endregion

		private static object DrawFields(object obj, GUIContent content = null, Object target = null)
		{
			if (obj == null)
			{
				return null;
			}

			List<Type> baseClasses = GetBaseClasses(obj.GetType());
			BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public |
			                           BindingFlags.NonPublic;
			for (int i = baseClasses.Count - 1; i > -1; i--)
			{
				FieldInfo[] fields = baseClasses[i].GetFields(bindingAttr);
				for (int j = 0; j < fields.Length; j++)
				{
					if (!EditorUtil.HasAttribute(fields[j], typeof(NonSerializedAttribute)) &&
					    !EditorUtil.HasAttribute(fields[j], typeof(HideInInspector)) &&
					    (!fields[j].IsPrivate && !fields[j].IsFamily ||
					     EditorUtil.HasAttribute(fields[j], typeof(SerializeField))
					    ))
					{
						if (content == null)
						{
							string name = fields[j].Name;
							TooltipAttribute[] array;
							if ((array =
								    fields[j].GetCustomAttributes(typeof(TooltipAttribute), false) as TooltipAttribute
									    [])?.Length > 0)
							{
								content = new GUIContent(EditorUtil.SplitCamelCase(name), array[0].tooltip);
							}
							else
							{
								content = new GUIContent(EditorUtil.SplitCamelCase(name));
							}
						}

						EditorGUI.BeginChangeCheck();
						object value = DrawFieldLayout(content, fields[j].FieldType, fields[j].GetValue(obj), target);
						if (EditorGUI.EndChangeCheck())
						{
							fields[j].SetValue(obj, value);
							GUI.changed = true;
						}

						content = null;
					}
				}
			}

			return obj;
		}

		public static List<Type> GetBaseClasses(Type t)
		{
			List<Type> list = new List<Type>();
			while (t != null && t != typeof(object))
			{
				list.Add(t);
				t = t.BaseType;
			}

			return list;
		}

//		public static object DrawField(Rect position, GUIContent content, object value)
//		{
//			DrawField(position, content, value.GetType(), value);
//		}
//		
//		public static object DrawFieldLayout(Rect position, GUIContent content, Type fieldType, object value)
//		{
//			if (typeof(IList).IsAssignableFrom(fieldType))
//			{
//				return DrawArrayField(position, content, fieldType, value);
//			}
//			return DrawSingleField(position, content, fieldType, value);
//		}
//		
//		private static object DrawArrayField(Rect position, GUIContent content, Type fieldType, object value)
//		{
//			Type type;
//			if (fieldType.IsArray)
//			{
//				type = fieldType.GetElementType();
//			}
//			else
//			{
//				Type type2 = fieldType;
//				while (!type2.IsGenericType)
//				{
//					type2 = type2.BaseType;
//				}
//
//				type = type2.GetGenericArguments()[0];
//			}
//
//			IList list;
//			if (value == null)
//			{
//				if (fieldType.IsGenericType || fieldType.IsArray)
//				{
//					list = (Activator.CreateInstance(typeof(List<>).MakeGenericType(new Type[]
//					{
//						type
//					}), true) as IList);
//				}
//				else
//				{
//					list = (Activator.CreateInstance(fieldType, true) as IList);
//				}
//
//				if (fieldType.IsArray)
//				{
//					Array array = Array.CreateInstance(type, list.Count);
//					list.CopyTo(array, 0);
//					list = array;
//				}
//
//				GUI.changed = (true);
//			}
//			else
//			{
//				list = (IList) value;
//			}
//
//			if (DrawFoldout(content.text.GetHashCode(), content))
//			{
//				EditorGUI.indentLevel++;
//				bool flag = content.text.GetHashCode() == editingFieldHash;
//				int num = (!flag) ? list.Count : savedArraySize;
//				int num2 = EditorGUI.IntField(position, "Size", num);
//				if (flag && editingArray &&
//				    (GUIUtility.keyboardControl != currentKeyboardControl ||
//				     Event.current.keyCode == KeyCode.Return))
//				{
//					if (num2 != list.Count)
//					{
//						Array array2 = Array.CreateInstance(type, num2);
//						int num3 = -1;
//						for (int i = 0; i < num2; i++)
//						{
//							if (i < list.Count)
//							{
//								num3 = i;
//							}
//
//							if (num3 == -1)
//							{
//								break;
//							}
//
//							object value2 = list[num3];
//							if (i >= list.Count && !typeof(UnityEngine.Object).IsAssignableFrom(type) &&
//							    !typeof(string).IsAssignableFrom(type))
//							{
//								value2 = Activator.CreateInstance(list[num3].GetType(), true);
//							}
//
//							array2.SetValue(value2, i);
//						}
//
//						if (fieldType.IsArray)
//						{
//							list = array2;
//						}
//						else
//						{
//							if (fieldType.IsGenericType)
//							{
//								list = (Activator.CreateInstance(typeof(List<>).MakeGenericType(new Type[]
//								{
//									type
//								}), true) as IList);
//							}
//							else
//							{
//								list = (Activator.CreateInstance(fieldType, true) as IList);
//							}
//
//							for (int j = 0; j < array2.Length; j++)
//							{
//								list.Add(array2.GetValue(j));
//							}
//						}
//					}
//
//					editingArray = false;
//					savedArraySize = -1;
//					editingFieldHash = -1;
//					GUI.changed = (true);
//				}
//				else if (num2 != num)
//				{
//					if (!editingArray)
//					{
//						currentKeyboardControl = GUIUtility.keyboardControl;
//						editingArray = true;
//						editingFieldHash = content.text.GetHashCode();
//					}
//
//					savedArraySize = num2;
//				}
//
//				for (int k = 0; k < list.Count; k++)
//				{
//					content.text = "Element " + k;
//					list[k] = DrawField(content, type, list[k]);
//				}
//
//				EditorGUI.indentLevel--;
//			}
//
//			return list;
//		}

		public static object DrawFieldLayout(GUIContent content, object value, Object target)
		{
			return DrawFieldLayout(content, value.GetType(), value, target);
		}
		
		public static object DrawFieldLayout(GUIContent content, Type fieldType, object value, Object target)
		{
			if (typeof(IList).IsAssignableFrom(fieldType))
			{
				return DrawArrayFieldLayout(content, fieldType, value, target);
			}

			return DrawSingleFieldLayout(content, fieldType, value, target);
		}

		private static object DrawArrayFieldLayout(GUIContent content, Type fieldType, object value, Object target)
		{
			Type type;
			if (fieldType.IsArray)
			{
				type = fieldType.GetElementType();
			}
			else
			{
				Type type2 = fieldType;
				while (!type2.IsGenericType)
				{
					type2 = type2.BaseType;
				}

				type = type2.GetGenericArguments()[0];
			}

			IList list;
			if (value == null)
			{
				if (fieldType.IsGenericType || fieldType.IsArray)
				{
					list = (Activator.CreateInstance(typeof(List<>).MakeGenericType(new Type[]
					{
						type
					}), true) as IList);
				}
				else
				{
					list = (Activator.CreateInstance(fieldType, true) as IList);
				}

				if (fieldType.IsArray)
				{
					Array array = Array.CreateInstance(type, list.Count);
					list.CopyTo(array, 0);
					list = array;
				}

				GUI.changed = (true);
			}
			else
			{
				list = (IList) value;
			}

			EditorGUILayout.BeginVertical();
			int hashCode = value?.GetHashCode() ?? 0;
			if (target)
				hashCode ^= target.GetHashCode();
			bool expand = LoadFoldoutExpand(GetFoldoutSaveKey(content.text, hashCode));
			if (DrawFoldout(expand, content.text, hashCode))
			{
				EditorGUI.indentLevel++;
				int size = list.Count;
				int newSize = EditorGUILayout.DelayedIntField("Size", size);
				if (newSize != list.Count)
				{
					Array newArray = Array.CreateInstance(type, newSize);
					for (int i = 0; i < newSize; i++)
					{
						object element = null;
						if (i < list.Count)
							element = list[i];
						else if(!typeof(Object).IsAssignableFrom(type) && !typeof(string).IsAssignableFrom(type))
							element = Activator.CreateInstance(type, true);
						
						newArray.SetValue(element, i);
					}

					if (fieldType.IsArray)
					{
						list = newArray;
					}
					else
					{
						if (fieldType.IsGenericType)
						{
							list = (Activator.CreateInstance(typeof(List<>).MakeGenericType(new Type[]
							{
								type
							}), true) as IList);
						}
						else
						{
							list = (Activator.CreateInstance(fieldType, true) as IList);
						}

						for (int j = 0; j < newArray.Length; j++)
						{
							list.Add(newArray.GetValue(j));
						}
					}
					GUI.changed = true;
				}

				for (int k = 0; k < list.Count; k++)
				{
					GUILayout.BeginHorizontal();
					list[k] = DrawFieldLayout(TempContent("Element " + k), type, list[k], target);
					GUILayout.Space(6f);
					GUILayout.EndHorizontal();
				}

				EditorGUI.indentLevel--;
			}

			EditorGUILayout.EndVertical();
			return list;
		}

		private static object DrawSingleFieldLayout(GUIContent content, Type fieldType, object value, Object target)
		{
			if (fieldType == typeof(int))
			{
				return EditorGUILayout.IntField(content, (int) value);
			}

			if (fieldType == typeof(float))
			{
				return EditorGUILayout.FloatField(content, (float) value);
			}

			if (fieldType == typeof(double))
			{
				return EditorGUILayout.FloatField(content, Convert.ToSingle((double) value));
			}

			if (fieldType == typeof(long))
			{
				return (long) EditorGUILayout.IntField(content, Convert.ToInt32((long) value));
			}

			if (fieldType == typeof(bool))
			{
				return EditorGUILayout.Toggle(content, (bool) value);
			}

			if (fieldType == typeof(string))
			{
				return EditorGUILayout.TextField(content, (string) value);
			}

			if (fieldType == typeof(byte))
			{
				return Convert.ToByte(EditorGUILayout.IntField(content, Convert.ToInt32(value)));
			}

			if (fieldType == typeof(Vector2))
			{
				return EditorGUILayout.Vector2Field(content, (Vector2) value);
			}

			if (fieldType == typeof(Vector2Int))
			{
				return EditorGUILayout.Vector2IntField(content, (Vector2Int) value);
			}

			if (fieldType == typeof(Vector3))
			{
				return EditorGUILayout.Vector3Field(content, (Vector3) value);
			}

			if (fieldType == typeof(Vector3Int))
			{
				return EditorGUILayout.Vector3IntField(content, (Vector3Int) value);
			}

			if (fieldType == typeof(Vector4))
			{
				return EditorGUILayout.Vector4Field(content.text, (Vector4) value);
			}

			if (fieldType == typeof(Quaternion))
			{
				Quaternion quaternion = (Quaternion) value;
				Vector4 value2 = Vector4.zero;
				value2.Set(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
				value2 = EditorGUILayout.Vector4Field(content.text, value2);
				quaternion.Set(value2.x, value2.y, value2.z, value2.w);
				return quaternion;
			}

			if (fieldType == typeof(Color))
			{
				return EditorGUILayout.ColorField(content, (Color) value);
			}
			
			if (fieldType == typeof(Color32))
			{
				return EditorGUILayout.ColorField(content, (Color32) value);
			}

			if (fieldType == typeof(Rect))
			{
				return EditorGUILayout.RectField(content, (Rect) value);
			}

			if (fieldType == typeof(Matrix4x4))
			{
				GUILayout.BeginVertical();
				int hashCode = value?.GetHashCode() ?? 0;
				if (target)
					hashCode ^= target.GetHashCode();
				bool expand = LoadFoldoutExpand(GetFoldoutSaveKey(content.text, hashCode));
				if (DrawFoldout(expand, content.text, hashCode))
				{
					EditorGUI.indentLevel++;
					Matrix4x4 matrix4x = (Matrix4x4) value;
					for (int i = 0; i < 4; i++)
					{
						for (int j = 0; j < 4; j++)
						{
							EditorGUI.BeginChangeCheck();
							matrix4x[i, j] = EditorGUILayout.FloatField("E" + i.ToString() + j.ToString(),
								matrix4x[i, j]);
							if (EditorGUI.EndChangeCheck())
							{
								GUI.changed = (true);
							}
						}
					}

					value = matrix4x;
					EditorGUI.indentLevel--;
				}

				GUILayout.EndVertical();
				return value;
			}

			if (fieldType == typeof(AnimationCurve))
			{
				if (value == null)
				{
					value = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
					GUI.changed = (true);
				}

				return EditorGUILayout.CurveField(content, (AnimationCurve) value);
			}

			if (fieldType == typeof(LayerMask))
			{
				return DrawLayerMaskLayout(content, (LayerMask) value);
			}

			if (typeof(UnityEngine.Object).IsAssignableFrom(fieldType))
			{
				return EditorGUILayout.ObjectField(content, (UnityEngine.Object) value, fieldType, true);
			}

			if (fieldType.IsEnum)
			{
				if (fieldType.GetCustomAttribute<FlagsAttribute>() == null)
					return EditorGUILayout.EnumPopup(content, (Enum) value);
				return EditorGUILayout.EnumFlagsField(content, (Enum) value);
			}

			if (fieldType.IsClass || fieldType.IsValueType && !fieldType.IsPrimitive)
			{
				if (typeof(Delegate).IsAssignableFrom(fieldType))
				{
					return null;
				}

				try
				{
					GUILayout.BeginVertical();
					if (value == null)
					{
						if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(Nullable<>))
						{
							fieldType = Nullable.GetUnderlyingType(fieldType);
						}

						value = Activator.CreateInstance(fieldType, true);
					}

					if (DrawFoldout(content.text, target))
					{
						EditorGUI.indentLevel++;
						value = DrawFields(value, null, target);
						EditorGUI.indentLevel--;
					}
					GUILayout.EndVertical();
					object result = value;
					return result;
				}
				catch (Exception)
				{
					GUILayout.EndVertical();
					return null;
				}
			}

			EditorGUILayout.LabelField("Unsupported Type: " + fieldType);
			return null;
		}

		private static LayerMask DrawLayerMaskLayout(GUIContent content, LayerMask layerMask)
		{
			if (layerNames == null)
			{
				InitLayers();
			}

			int num = 0;
			for (int i = 0; i < layerNames.Length; i++)
			{
				if ((layerMask.value & maskValues[i]) == maskValues[i])
				{
					num |= 1 << i;
				}
			}

			int num2 = EditorGUILayout.MaskField(content, num, layerNames);
			if (num2 != num)
			{
				num = 0;
				for (int j = 0; j < layerNames.Length; j++)
				{
					if ((num2 & 1 << j) != 0)
					{
						num |= maskValues[j];
					}
				}

				layerMask.value = num;
			}

			return layerMask;
		}

		private static void InitLayers()
		{
			List<string> list = new List<string>();
			List<int> list2 = new List<int>();
			for (int i = 0; i < 32; i++)
			{
				string text = LayerMask.LayerToName(i);
				if (!string.IsNullOrEmpty(text))
				{
					list.Add(text);
					list2.Add(1 << i);
				}
			}

			layerNames = list.ToArray();
			maskValues = list2.ToArray();
		}
	}
}

