using System;
using System.Collections.Generic;
using System.Reflection;
using CustomizationInspector.Runtime;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CustomizationInspector.Editor
{
	public enum FoldoutStyle
	{
		Normal,
		Header,
	}
	
    public class FoldoutDrawer : Drawer
    {
        private class FoldoutInfo
        {
            public readonly string GroupName;
            public readonly string ShowName;
            public readonly List<string> PropertiesPath;
            public readonly List<MethodInfo> MethodInfos;
            public readonly List<FoldoutInfo> Children;
            public readonly FoldoutInfo Parent;
            public bool Expand;

            public FoldoutInfo Root
            {
	            get
	            {
		            var root = this;
		            while (root.Parent != null)
			            root = root.Parent;
		            return root;
	            }
            }
            
            public bool ExpandInInspector
            {
	            get
	            {
		            var foldoutInfo = this;
		            while (foldoutInfo.Expand && foldoutInfo.Parent != null)
			            foldoutInfo = foldoutInfo.Parent;
		            return foldoutInfo.Expand;
	            }
            }

            public FoldoutInfo(string groupName, string showName, bool expand, FoldoutInfo parent)
            {
                GroupName = groupName;
                ShowName = showName;
                PropertiesPath = new List<string>();
                MethodInfos = new List<MethodInfo>();
                Children = new List<FoldoutInfo>();
                Expand = expand;
                Parent = parent;
            }
        }

        public delegate void DrawMethodEventHandler(MethodInfo methodInfo, Object[] targets);
        public const char NameSeparator = '/';
        
        public DrawMethodEventHandler DrawMethod;

        public readonly FoldoutStyle Style;
        private Dictionary<string, string> foldoutMembers; //key: memberName  value: root groupName
        private Dictionary<string, FoldoutInfo> foldoutCache; //key: groupName  value: foldoutInfo
        private List<FoldoutInfo> foldoutRoots;
        private HashSet<string> drawnFoldoutRoots;
        private HashSet<string> drawnMembers;

        public FoldoutDrawer(SerializedObject serializedObject, Object target, Object[] targets, FoldoutStyle style = FoldoutStyle.Header) : base(serializedObject, target, targets)
        {
	        Style = style;
	        foldoutMembers = new Dictionary<string, string>();
	        foldoutCache = new Dictionary<string, FoldoutInfo>();
	        foldoutRoots = new List<FoldoutInfo>();
	        Setup();
	        drawnFoldoutRoots = new HashSet<string>(foldoutRoots.Count);
	        drawnMembers = new HashSet<string>(foldoutMembers.Count);
        }

        public void SetDrawMethodHandler(DrawMethodEventHandler handler)
        {
	        DrawMethod = handler;
        }
        
        /// <summary>
        /// Draw all foldout
        /// </summary>
        public override void Draw()
        {
	        BeginManualDraw();
	        foreach (var foldoutInfo in foldoutRoots)
		        DrawFoldout(foldoutInfo);
        }

        /// <summary>
        /// Call this method before call DrawFoldout and DrawRemainFoldout
        /// </summary>
        public void BeginManualDraw()
        {
	        drawnFoldoutRoots.Clear();
	        drawnMembers.Clear();
        }
        
        /// <summary>
        /// Draw a foldout group. You can check is the serializedProperty drawn by call IsDrawn when you don not want draw duplication.
        /// </summary>
        /// <param name="serializedProperty"></param>
        /// <returns>success</returns>
        public bool DrawFoldout(SerializedProperty serializedProperty)
        {
	        if (!TryGetGroupName(serializedProperty, out var rootGroupName)
	            || !TryGetFoldoutInfo(rootGroupName, out var foldoutInfo))
		        return false;
	        DrawFoldout(foldoutInfo);
	        return true;
        }
        
        public void DrawRemainFoldout()
        {
	        foreach (var foldoutInfo in foldoutRoots)
		        if(!IsDrawn(foldoutInfo))
			        DrawFoldout(foldoutInfo);
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
					AddFoldoutMember(iterator, foldoutInfo.Root.GroupName);
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
				AddFoldoutMember(methodInfo, foldoutInfo.Root.GroupName);
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

        private bool TryGetGroupName(SerializedProperty serializedProperty, out string rootGroupName)
        {
	        return foldoutMembers.TryGetValue(serializedProperty.propertyPath, out rootGroupName);
        }
        
        private bool TryGetGroupName(MethodInfo methodInfo, out string rootGroupName)
        {
	        return foldoutMembers.TryGetValue(GetFoldoutMethodMemberKey(methodInfo), out rootGroupName);
        }
        
        private bool TryGetGroupName(string memberName, out string rootGroupName)
        {
	        return foldoutMembers.TryGetValue(memberName, out rootGroupName);
        }
        
        private bool TryGetFoldoutInfo(string groupName, out FoldoutInfo foldoutInfo)
        {
	        return foldoutCache.TryGetValue(groupName, out foldoutInfo);
        }

        private FoldoutInfo CreateFoldoutInfo(string groupName, string showName, FoldoutInfo parent)
        {
	        FoldoutInfo foldoutInfo = new FoldoutInfo(groupName, showName, FieldInspector.LoadFoldoutExpand(FieldInspector.GetFoldoutSaveKey(groupName, target)), parent);
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
			        child = CreateFoldoutInfo(groupName, names[i], parent);
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

        private void DrawFoldout(FoldoutInfo foldoutInfo)
        {
	        //Add to drawn set
	        if (foldoutInfo.Parent == null && !drawnFoldoutRoots.Contains(foldoutInfo.GroupName))
		        drawnFoldoutRoots.Add(foldoutInfo.GroupName);
	        
	        foreach (var path in foldoutInfo.PropertiesPath)
		        AddDrawnMember(path);
	        if (DrawMethod != null)
	        {
		        foreach (var methodInfo in foldoutInfo.MethodInfos)
			        AddDrawnMember(methodInfo);
	        }

	        if (foldoutInfo.Parent == null || foldoutInfo.Parent.ExpandInInspector)
	        {
		        //Draw self
		        if (Style == FoldoutStyle.Header)
		        {
			        Rect position = GUILayoutUtility.GetRect(EditorGUIUtility.fieldWidth, EditorGUIUtility.fieldWidth,
				        EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight, EditorStyles.foldoutHeader);
			        position = EditorGUI.IndentedRect(position);
			        foldoutInfo.Expand = FieldInspector.DrawFoldoutHeader(position, foldoutInfo.Expand, foldoutInfo.ShowName, target, null);
		        }
		        else
			        foldoutInfo.Expand = FieldInspector.DrawFoldout(foldoutInfo.Expand, foldoutInfo.ShowName, target);
	        }
			//Do not draw when not expandInInspector
			if (foldoutInfo.ExpandInInspector)
			{
				EditorGUI.indentLevel++;
				foreach (var path in foldoutInfo.PropertiesPath)
				{
					if (foldoutInfo.Expand)
						EditorGUILayout.PropertyField(serializedObject.FindProperty(path), true);
				}

				if (DrawMethod != null)
				{
					foreach (var methodInfo in foldoutInfo.MethodInfos)
					{
						if (foldoutInfo.Expand)
							DrawMethod(methodInfo, targets);
					}
				}
			}
	        
	        //Draw children
	        foreach (var child in foldoutInfo.Children)
		        DrawFoldout(child);
	        
	        if (foldoutInfo.ExpandInInspector)
		        EditorGUI.indentLevel--;
        }

        private void AddFoldoutMember(SerializedProperty serializedProperty, string rootGroupName)
        {
	        foldoutMembers.Add(serializedProperty.propertyPath, rootGroupName);
        }
        
        private void AddFoldoutMember(MethodInfo methodInfo, string rootGroupName)
        {
	        foldoutMembers.Add(GetFoldoutMethodMemberKey(methodInfo), rootGroupName);
        }

        private void AddDrawnFoldoutRoot(FoldoutInfo foldoutInfo)
        {
	        var root = foldoutInfo.Root;
	        if (!drawnFoldoutRoots.Contains(root.GroupName))
		        drawnFoldoutRoots.Add(root.GroupName);
        }
        
        private void AddDrawnMember(SerializedProperty serializedProperty)
        {
	        AddDrawnMember(serializedProperty.propertyPath);
        }
        
        private void AddDrawnMember(MethodInfo methodInfo)
        {
	        AddDrawnMember(GetFoldoutMethodMemberKey(methodInfo));
        }
        
        private void AddDrawnMember(string member)
        {
	        if (!drawnMembers.Contains(member))
		        drawnMembers.Add(member);
        }
        
        public bool IsFoldout(SerializedProperty property)
        {
	        return foldoutMembers.ContainsKey(property.propertyPath);
        }
        
        public bool IsFoldout(MethodInfo methodInfo)
        {
	        return foldoutMembers.ContainsKey(GetFoldoutMethodMemberKey(methodInfo));
        }

        private bool IsDrawn(FoldoutInfo foldoutInfo)
        {
	        return drawnFoldoutRoots.Contains(foldoutInfo.Root.GroupName);
        }
        
        public bool IsDrawn(SerializedProperty property)
        {
	        return drawnMembers.Contains(property.propertyPath);
        }
        
        public bool IsDrawn(MethodInfo methodInfo)
        {
	        return drawnMembers.Contains(GetFoldoutMethodMemberKey(methodInfo));
        }
        
        public void SaveExpand()
        {
	        foreach (var foldoutInfo in foldoutCache.Values)
		        foreach (var obj in targets)
			        FieldInspector.SaveFoldoutExpand(FieldInspector.GetFoldoutSaveKey(foldoutInfo.GroupName, obj), foldoutInfo.Expand);
        }
        
        private static string GetFoldoutMethodMemberKey(MethodInfo methodInfo)
        {
	        return $"--Method_{methodInfo}";
        }
    }
}