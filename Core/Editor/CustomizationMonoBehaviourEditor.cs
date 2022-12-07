using CustomizationInspector.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CustomizationInspector.Editor
{
	[CustomEditor(typeof(MonoBehaviour), true)]
	[CanEditMultipleObjects]
	public class CustomizationMonoBehaviourEditor : UnityEditor.Editor
	{
		private class FoldoutInfo
		{
			public readonly string GroupName;
			public readonly List<string> PropertiesPath;
			public bool Expanded;

			public FoldoutInfo(string groupName, bool expanded)
			{
				GroupName = groupName;
				PropertiesPath = new List<string>();
				Expanded = expanded;
			}
		}
		
		public static BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.Public | BindingFlags.Static;

		private const string SavedExpandedSaveKey = "CustomizationInspector_Foldout_Expanded_Save_Key";
		
		private Type targetType;
		private Dictionary<string, FoldoutInfo> foldoutCache;
		private Dictionary<string, string> allFoldoutProperties;
		private static HashSet<string> savedKeys = new HashSet<string>();

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
			foldoutCache = SetupFoldout(serializedObject.GetIterator(), out allFoldoutProperties);
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
			
			HashSet<string> drewFoldoutProperties = new HashSet<string>();
			
			for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
			{
				using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
				{
					if (drewFoldoutProperties.Contains(iterator.propertyPath)) continue;
					if (allFoldoutProperties.TryGetValue(iterator.propertyPath, out var groupName) &&
					    foldoutCache.TryGetValue(groupName, out var foldoutInfo))
					{
						foldoutInfo.Expanded = EditorGUILayout.Foldout(foldoutInfo.Expanded, groupName, true);
						EditorGUI.indentLevel++;
						foreach (var path in foldoutInfo.PropertiesPath)
						{
							drewFoldoutProperties.Add(path);
							if (foldoutInfo.Expanded)
								EditorGUILayout.PropertyField(serializedObject.FindProperty(path), true);
						}
						EditorGUI.indentLevel--;
					}
					else
						EditorGUILayout.PropertyField(iterator, true);
				}
			}
			serializedObject.ApplyModifiedProperties();
			EditorGUI.EndChangeCheck();
			
			//利用base序列化
			// base.OnInspectorGUI();
			
			//Apply to all targets.
			MethodAttribute(targetType, targets);
		}

		private static Dictionary<string, FoldoutInfo> SetupFoldout(SerializedProperty iterator, out Dictionary<string, string> allFoldoutProperties)
		{
			//Avoid changing origin iterator state.
			iterator = iterator.Copy();
			var target = iterator.serializedObject.targetObject;
			Dictionary<string, FoldoutInfo> foldoutCache = new Dictionary<string, FoldoutInfo>();
			allFoldoutProperties = new Dictionary<string, string>();
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
					}
					foldoutInfo.PropertiesPath.Add(iterator.propertyPath);
					allFoldoutProperties.Add(iterator.propertyPath, foldoutAttribute.GroupName);
				}
				catch (Exception e)
				{
					Debug.LogError(e, iterator.serializedObject.targetObject);
				}
			}
			return foldoutCache;
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
						var parameters = info.GetParameters();
						bool canDraw = buttonAttribute.Params.Count <= parameters.Length;
						if (canDraw)
						{
							for (int i = buttonAttribute.Params.Count, len = parameters.Length; i < len; i++)
							{
								var parameter = parameters[i];
								if (!parameter.HasDefaultValue)
								{
									canDraw = false;
									break;
								}
								//Add default value
								buttonAttribute.Params.Add(parameter.DefaultValue);
							}
						}
						string desc = buttonAttribute.ShowName ?? info.Name;
						if (!canDraw)
						{
							var backgroundColor = GUI.backgroundColor;
							//Red color
							GUI.backgroundColor = new Color(1, 0.3254902f, 0.2901961f);
							if (GUILayout.Button(desc))
								Debug.LogErrorFormat("Parameter count not match! Method name: {0}, button name: {1}.", info.Name, desc);
							GUI.backgroundColor = backgroundColor;
							continue;
						}
						if (GUILayout.Button(desc))
						{
							foreach (var target in targets)
							{
								try
								{
									info.Invoke(target, buttonAttribute.Params.ToArray());
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