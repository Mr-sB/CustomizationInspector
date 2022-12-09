using CustomizationInspector.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CustomizationInspector.Editor
{
	[CustomEditor(typeof(Object), true)]
	[CanEditMultipleObjects]
	public class CustomizationMonoBehaviourEditor : UnityEditor.Editor
	{
		private class FoldoutInfo
		{
			public readonly string GroupName;
			public readonly List<string> PropertiesPath;
			public readonly List<MethodInfo> MethodInfos;
			public bool Expanded;

			public FoldoutInfo(string groupName, bool expanded)
			{
				GroupName = groupName;
				PropertiesPath = new List<string>();
				MethodInfos = new List<MethodInfo>();
				Expanded = expanded;
			}
		}
		
		public static readonly BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.Public | BindingFlags.Static;

		private const string SavedExpandedSaveKey = "CustomizationInspector_Foldout_Expanded_Save_Key";
		
		private Type targetType;
		private Dictionary<string, FoldoutInfo> foldoutCache;
		private Dictionary<string, string> allFoldoutProperties;
		private List<string> allGroupNames; //sorted by group order
		private HashSet<string> drewFoldoutMembers;
		private HashSet<string> drewFoldoutGroups;
		private static HashSet<string> savedKeys = new HashSet<string>();

		private static Dictionary<Type, Action<MethodInfo, Attribute, Object[]>> DrawMethodHandlers =
			new Dictionary<Type, Action<MethodInfo, Attribute, Object[]>>
			{
				{typeof(ButtonAttribute), DrawButton},
			};

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
			string value = string.Join(',', savedKeys);
			savedKeys.Clear();
			EditorPrefs.SetString(SavedExpandedSaveKey, value);
		}

		private static void AfterAssemblyReload()
		{
			//Load after reload
			foreach (string key in EditorPrefs.GetString(SavedExpandedSaveKey, "").Split(',', StringSplitOptions.RemoveEmptyEntries))
			{
				if (!savedKeys.Contains(key))
					savedKeys.Add(key);
			}
			EditorPrefs.DeleteKey(SavedExpandedSaveKey);
		}
		
		private static void OnApplicationQuite()
		{
			//Delete key when application quite
			foreach (var savedKey in savedKeys)
				EditorPrefs.DeleteKey(savedKey);
			EditorPrefs.DeleteKey(SavedExpandedSaveKey);
		}
		
		private void OnEnable()
		{
			if (targetType == null)
				targetType = target.GetType();
			foldoutCache = SetupFoldout(serializedObject.GetIterator(), out allGroupNames, out allFoldoutProperties);
			drewFoldoutMembers = new HashSet<string>();
			drewFoldoutGroups = new HashSet<string>();
		}
		
		void OnDisable()
		{
			if (foldoutCache != null && targets != null)
			{
				foreach (var foldoutInfo in foldoutCache.Values)
					foreach (var obj in targets)
					{
						var saveKey = GetExpandedSaveKey(foldoutInfo.GroupName, obj);
						if (!savedKeys.Contains(saveKey))
							savedKeys.Add(saveKey);
						EditorPrefs.SetBool(saveKey, foldoutInfo.Expanded);
					}
			}
		}

		private static string GetExpandedSaveKey(string groupName, Object target)
		{
			return string.Format($"CustomizationInspector_Foldout_Expanded_{target.GetInstanceID()}_{groupName}");
		}

		public override void OnInspectorGUI()
		{
			//利用源码序列化
			EditorGUI.BeginChangeCheck();
			serializedObject.UpdateIfRequiredOrScript();
			SerializedProperty iterator = serializedObject.GetIterator();
			
			drewFoldoutMembers.Clear();
			drewFoldoutGroups.Clear();
			for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
			{
				using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
				{
					if (drewFoldoutMembers.Contains(iterator.propertyPath)) continue;
					if (allFoldoutProperties.TryGetValue(iterator.propertyPath, out var groupName) &&
					    foldoutCache.TryGetValue(groupName, out var foldoutInfo))
					{
						DrawFoldout(foldoutInfo);
					}
					else
						EditorGUILayout.PropertyField(iterator, true);
				}
			}
			
			//Draw remain foldout
			foreach (var groupName in allGroupNames)
			{
				if (drewFoldoutGroups.Contains(groupName) || !foldoutCache.TryGetValue(groupName, out var foldoutInfo)) continue;
				DrawFoldout(foldoutInfo);
			}
			
			serializedObject.ApplyModifiedProperties();
			EditorGUI.EndChangeCheck();
			
			//利用base序列化
			// base.OnInspectorGUI();

			//Apply to all targets.
			DrawMethods();
			
			drewFoldoutMembers.Clear();
			drewFoldoutGroups.Clear();
		}

		private static Dictionary<string, FoldoutInfo> SetupFoldout(SerializedProperty iterator, out List<string> allGroupNames, out Dictionary<string, string> allFoldoutProperties)
		{
			//Avoid changing origin iterator state.
			iterator = iterator.Copy();
			var target = iterator.serializedObject.targetObject;
			var targetType = target.GetType();
			Dictionary<string, FoldoutInfo> foldoutCache = new Dictionary<string, FoldoutInfo>();
			allFoldoutProperties = new Dictionary<string, string>();
			allGroupNames = new List<string>();
			for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
			{
				try
				{
					var fieldInfo = iterator.GetFieldInfo();
					if (fieldInfo == null) continue;
					var foldoutAttribute = fieldInfo.GetCustomAttribute<FoldoutAttribute>();
					if (foldoutAttribute == null) continue;
					if (!foldoutCache.TryGetValue(foldoutAttribute.GroupName, out var foldoutInfo))
					{
						foldoutInfo = new FoldoutInfo(foldoutAttribute.GroupName, EditorPrefs.GetBool(GetExpandedSaveKey(foldoutAttribute.GroupName, target), true));
						foldoutCache.Add(foldoutAttribute.GroupName, foldoutInfo);
						allGroupNames.Add(foldoutAttribute.GroupName);
					}
					foldoutInfo.PropertiesPath.Add(iterator.propertyPath);
					allFoldoutProperties.Add(iterator.propertyPath, foldoutAttribute.GroupName);
				}
				catch (Exception e)
				{
					Debug.LogError(e, target);
				}
			}
			
			MethodInfo[] methodInfos = targetType.GetMethods(flag);
			foreach (var methodInfo in methodInfos)
			{
				object[] attributes = methodInfo.GetCustomAttributes(true);
				FoldoutAttribute foldoutAttribute = null;
				foreach (object attribute in attributes)
				{
					if (attribute is FoldoutAttribute foldout)
					{
						foldoutAttribute = foldout;
						break;
					}
				}
				if (foldoutAttribute == null) continue;

				var canDrawMethod = false;
				foreach (object attribute in attributes)
				{
					//can draw
					if (DrawMethodHandlers.ContainsKey(attribute.GetType()))
					{
						canDrawMethod = true;
						break;
					}
				}

				if (canDrawMethod)
				{
					if (!foldoutCache.TryGetValue(foldoutAttribute.GroupName, out var foldoutInfo))
					{
						foldoutInfo = new FoldoutInfo(foldoutAttribute.GroupName, EditorPrefs.GetBool(GetExpandedSaveKey(foldoutAttribute.GroupName, target), true));
						foldoutCache.Add(foldoutAttribute.GroupName, foldoutInfo);
						allGroupNames.Add(foldoutAttribute.GroupName);
					}
					foldoutInfo.MethodInfos.Add(methodInfo);
				}
			}
			
			return foldoutCache;
		}

		private void DrawFoldout(FoldoutInfo foldoutInfo)
		{
			drewFoldoutGroups.Add(foldoutInfo.GroupName);
			foldoutInfo.Expanded = EditorGUILayout.Foldout(foldoutInfo.Expanded, foldoutInfo.GroupName, true);
			EditorGUI.indentLevel++;
			foreach (var path in foldoutInfo.PropertiesPath)
			{
				drewFoldoutMembers.Add(path);
				if (foldoutInfo.Expanded)
					EditorGUILayout.PropertyField(serializedObject.FindProperty(path), true);
			}
			foreach (var methodInfo in foldoutInfo.MethodInfos)
			{
				drewFoldoutMembers.Add(GetFoldoutMethodMemberKey(methodInfo));
				if (foldoutInfo.Expanded)
					DrawMethod(methodInfo, targets);
			}
			EditorGUI.indentLevel--;
		}
		
		private void DrawMethods()
		{
			MethodInfo[] methodInfos = targetType.GetMethods(flag);
			foreach (var methodInfo in methodInfos)
			{
				if (drewFoldoutMembers!= null && drewFoldoutMembers.Contains(GetFoldoutMethodMemberKey(methodInfo))) continue;
				DrawMethod(methodInfo, targets);
			}
		}

		private static string GetFoldoutMethodMemberKey(MethodInfo methodInfo)
		{
			return $"--Method_{methodInfo}";
		}
		
		public static void DrawMethod(MethodInfo methodInfo, Object[] targets)
		{
			object[] attributes = methodInfo.GetCustomAttributes(true);
			foreach (Attribute attribute in attributes)
			{
				if (DrawMethodHandlers.TryGetValue(attribute.GetType(), out var handler))
					handler?.Invoke(methodInfo, attribute, targets);
			}
		}

		public static void DrawButton(MethodInfo methodInfo, Attribute attribute, Object[] targets)
		{
			if (attribute is not ButtonAttribute buttonAttribute) return;
			var parameters = methodInfo.GetParameters();
			bool canDraw = buttonAttribute.Params.Count <= parameters.Length;
			if (canDraw)
			{
				for (int i = buttonAttribute.Params.Count, len = parameters.Length; i < len; i++)
				{
					var parameter = parameters[i];
					if (parameter.HasDefaultValue)
					{
						//Add default value
						buttonAttribute.Params.Add(parameter.DefaultValue);
						continue;
					}
					var defaultValueAttribute = parameter.GetCustomAttribute<System.ComponentModel.DefaultValueAttribute>();
					if (defaultValueAttribute != null)
					{
						//Add default value
						buttonAttribute.Params.Add(defaultValueAttribute.Value);
						continue;
					}
					canDraw = false;
				}
			}
			GUIContent content = new GUIContent(buttonAttribute.ShowName ?? methodInfo.Name);
			Rect position = GUILayoutUtility.GetRect(content, GUI.skin.button);
			position = EditorGUI.IndentedRect(position);
			if (!canDraw)
			{
				var backgroundColor = GUI.backgroundColor;
				//Red color
				GUI.backgroundColor = new Color(1, 0.3254902f, 0.2901961f);
				if (GUI.Button(position, content))
					Debug.LogErrorFormat("Parameter count not match! Method name: {0}, button name: {1}.", methodInfo.Name, content.text);
				GUI.backgroundColor = backgroundColor;
				return;
			}
			if (GUI.Button(position, content))
			{
				foreach (var target in targets)
				{
					try
					{
						methodInfo.Invoke(target, buttonAttribute.Params.ToArray());
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