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
            public readonly string ShowName;
            public readonly List<string> PropertiesPath;
            public readonly List<MethodInfo> MethodInfos;
            public readonly List<FoldoutInfo> Children;
            public bool Expanded;

            public FoldoutInfo(string groupName, string showName, bool expanded)
            {
                GroupName = groupName;
                ShowName = showName;
                PropertiesPath = new List<string>();
                MethodInfos = new List<MethodInfo>();
                Children = new List<FoldoutInfo>();
                Expanded = expanded;
            }
        }

        public delegate void DrawMethodEventHandler(MethodInfo methodInfo, Object[] targets);
        public const char NameSeparator = '/';
        
        public DrawMethodEventHandler DrawMethod;
        
        private HashSet<string> foldoutMembers;
        private Dictionary<string, FoldoutInfo> foldoutCache;
        private List<FoldoutInfo> foldoutRoots;

        public FoldoutDrawer(SerializedObject serializedObject, Object[] targets) : base(serializedObject, targets)
        {
	        foldoutMembers = new HashSet<string>();
	        foldoutCache = new Dictionary<string, FoldoutInfo>();
	        foldoutRoots = new List<FoldoutInfo>();
	        Setup();
        }

        public void SetDrawMethodHandler(DrawMethodEventHandler handler)
        {
	        DrawMethod = handler;
        }
        
        private void Setup()
        {
			var iterator = serializedObject.GetIterator();
			for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
			{
				try
				{
					var fieldInfo = iterator.GetFieldInfo();
					if (fieldInfo == null) continue;
					var foldoutAttribute = fieldInfo.GetCustomAttribute<FoldoutAttribute>();
					if (foldoutAttribute == null) continue;
					var foldoutInfo = GetOrCreateFoldoutInfo(foldoutAttribute.GroupName);
					foldoutInfo.PropertiesPath.Add(iterator.propertyPath);
					AddFoldoutMember(iterator);
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
				
				var foldoutInfo = GetOrCreateFoldoutInfo(foldoutAttribute.GroupName);
				foldoutInfo.MethodInfos.Add(methodInfo);
				AddFoldoutMember(methodInfo);
			}
		}

        private FoldoutInfo GetOrCreateFoldoutInfo(string groupName)
        {
	        return TryGetFoldoutInfo(groupName, out var foldoutInfo) ? foldoutInfo : CreateFoldoutInfoAndAddToParent(groupName);
        }
        
        private FoldoutInfo CreateFoldoutInfoAndAddToParent(string groupName)
        {
	        string[] names = groupName.Split(NameSeparator);
	        AddToParent(names);
	        TryGetFoldoutInfo(groupName, out var foldoutInfo);
	        return foldoutInfo;
        }

        private bool TryGetFoldoutInfo(string groupName, out FoldoutInfo foldoutInfo)
        {
	        return foldoutCache.TryGetValue(groupName, out foldoutInfo);
        }

        private FoldoutInfo CreateFoldoutInfo(string groupName, string showName)
        {
	        FoldoutInfo foldoutInfo = new FoldoutInfo(groupName, showName, FieldInspector.LoadFoldoutExpand(FieldInspector.GetFoldoutSaveKey(groupName, target)));
	        foldoutCache.Add(groupName, foldoutInfo);
	        return foldoutInfo;
        }

        private void AddToParent(string[] names)
        {
	        string groupName = null;
	        FoldoutInfo parent = null;
	        for (int i = 0; i < names.Length; i++)
	        {
		        bool isRoot = i == 0;
		        if (isRoot)
			        groupName = names[i];
		        else
			        groupName += "/" + names[i];
		        bool create = false;
		        if (!TryGetFoldoutInfo(groupName, out var child))
		        {
			        create = true;
			        child = CreateFoldoutInfo(groupName, names[i]);
		        }

		        if (create)
		        {
			        if (isRoot)
				        foldoutRoots.Add(child);
			        else
				        parent?.Children.Add(child);
		        }
		        parent = child;
	        }
        }
        
        public void Draw()
        {
	        foreach (var foldoutInfo in foldoutRoots)
		        DrawFoldout(foldoutInfo, true);
        }
        
        private void DrawFoldout(FoldoutInfo foldoutInfo, bool parentExpand)
        {
	        if (!parentExpand) return;
	        //Draw self
	        foldoutInfo.Expanded = FieldInspector.DrawFoldout(foldoutInfo.Expanded, foldoutInfo.ShowName, target);
	        EditorGUI.indentLevel++;
	        foreach (var path in foldoutInfo.PropertiesPath)
	        {
		        if (foldoutInfo.Expanded)
			        EditorGUILayout.PropertyField(serializedObject.FindProperty(path), true);
	        }

	        if (DrawMethod != null)
	        {
		        foreach (var methodInfo in foldoutInfo.MethodInfos)
		        {
			        if (foldoutInfo.Expanded)
				        DrawMethod(methodInfo, targets);
		        }
	        }
	        //Draw children
	        foreach (var child in foldoutInfo.Children)
		        DrawFoldout(child, foldoutInfo.Expanded);
	        EditorGUI.indentLevel--;
        }

        private void AddFoldoutMember(SerializedProperty serializedProperty)
        {
	        foldoutMembers.Add(serializedProperty.propertyPath);
        }
        
        private void AddFoldoutMember(MethodInfo methodInfo)
        {
	        foldoutMembers.Add(GetFoldoutMethodMemberKey(methodInfo));
        }
        
        public bool IsFoldout(SerializedProperty property)
        {
	        return foldoutMembers.Contains(property.propertyPath);
        }
        
        public bool IsFoldout(MethodInfo methodInfo)
        {
	        return foldoutMembers.Contains(GetFoldoutMethodMemberKey(methodInfo));
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