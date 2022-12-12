using System;
using System.Collections.Generic;
using System.Reflection;
using CustomizationInspector.Runtime;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CustomizationInspector.Editor
{
    public class FoldoutDrawer : Drawer
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

        public delegate void DrawMethodEventHandler(MethodInfo methodInfo, Object[] targets);
        public DrawMethodEventHandler DrawMethod;
        
        private Dictionary<string, FoldoutInfo> foldoutCache;
        private Dictionary<string, string> allFoldoutProperties;
        private List<string> allGroupNames; //sorted by group order
        private HashSet<string> drawnFoldoutMembers;
        private HashSet<string> drawnFoldoutGroups;

        public FoldoutDrawer(SerializedObject serializedObject, Object[] targets) : base(serializedObject, targets)
        {
            Setup(serializedObject.GetIterator());
            drawnFoldoutMembers = new HashSet<string>();
            drawnFoldoutGroups = new HashSet<string>();
        }

        public void SetDrawMethodHandler(DrawMethodEventHandler handler)
        {
	        DrawMethod = handler;
        }
        
        private void Setup(SerializedProperty iterator)
		{
			foldoutCache = new Dictionary<string, FoldoutInfo>();
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
						foldoutInfo = new FoldoutInfo(foldoutAttribute.GroupName, FieldInspector.LoadFoldoutExpand(FieldInspector.GetFoldoutSaveKey(foldoutAttribute.GroupName, target)));
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
			
			MethodInfo[] methodInfos = targetType.GetMethods(BindingFlags);
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
				
				if (!foldoutCache.TryGetValue(foldoutAttribute.GroupName, out var foldoutInfo))
				{
					foldoutInfo = new FoldoutInfo(foldoutAttribute.GroupName, FieldInspector.LoadFoldoutExpand(FieldInspector.GetFoldoutSaveKey(foldoutAttribute.GroupName, target)));
					foldoutCache.Add(foldoutAttribute.GroupName, foldoutInfo);
					allGroupNames.Add(foldoutAttribute.GroupName);
				}
				foldoutInfo.MethodInfos.Add(methodInfo);
			}
		}

        public void Draw()
        {
	        drawnFoldoutMembers.Clear();
	        drawnFoldoutGroups.Clear();
	        SerializedProperty iterator = serializedObject.GetIterator();
	        for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
	        {
		        using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
		        {
			        if (drawnFoldoutMembers.Contains(iterator.propertyPath)) continue;
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
		        if (drawnFoldoutGroups.Contains(groupName) || !foldoutCache.TryGetValue(groupName, out var foldoutInfo)) continue;
		        DrawFoldout(foldoutInfo);
	        }
        }
        
        private void DrawFoldout(FoldoutInfo foldoutInfo)
        {
	        drawnFoldoutGroups.Add(foldoutInfo.GroupName);
	        foldoutInfo.Expanded = FieldInspector.DrawFoldout(foldoutInfo.Expanded, foldoutInfo.GroupName, target);
	        EditorGUI.indentLevel++;
	        foreach (var path in foldoutInfo.PropertiesPath)
	        {
		        drawnFoldoutMembers.Add(path);
		        if (foldoutInfo.Expanded)
			        EditorGUILayout.PropertyField(serializedObject.FindProperty(path), true);
	        }

	        if (DrawMethod != null)
	        {
		        foreach (var methodInfo in foldoutInfo.MethodInfos)
		        {
			        drawnFoldoutMembers.Add(GetFoldoutMethodMemberKey(methodInfo));
			        if (foldoutInfo.Expanded)
				        DrawMethod(methodInfo, targets);
		        }
	        }
	        EditorGUI.indentLevel--;
        }

        public bool DrawnProperty(SerializedProperty property)
        {
	        return drawnFoldoutMembers.Contains(property.propertyPath);
        }
        
        public bool DrawnMethod(MethodInfo methodInfo)
        {
	        return drawnFoldoutMembers.Contains(GetFoldoutMethodMemberKey(methodInfo));
        }
        
        public void SaveExpand()
        {
	        foreach (var foldoutInfo in foldoutCache.Values)
		        foreach (var obj in targets)
			        FieldInspector.SaveFoldoutExpand(FieldInspector.GetFoldoutSaveKey(foldoutInfo.GroupName, obj), foldoutInfo.Expanded);
        }
        
        private static string GetFoldoutMethodMemberKey(MethodInfo methodInfo)
        {
	        return $"--Method_{methodInfo}";
        }
    }
}