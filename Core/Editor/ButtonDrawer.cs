using System;
using System.Reflection;
using CustomizationInspector.Runtime;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CustomizationInspector.Editor
{
    public class ButtonDrawer : Drawer
    {
	    private FoldoutDrawer foldoutDrawer;
	    
	    public ButtonDrawer(SerializedObject serializedObject, Object[] targets) : base(serializedObject, targets)
        {
        }

	    public void SetFoldoutDrawer(FoldoutDrawer drawer)
	    {
		    foldoutDrawer = drawer;
			drawer?.SetDrawMethodHandler(DrawButtons);
	    }
        
	    public void Draw()
	    {
		    MethodInfo[] methodInfos = targetType.GetMethods(BindingFlags);
		    foreach (var methodInfo in methodInfos)
		    {
			    if (foldoutDrawer != null && foldoutDrawer.DrawnMethod(methodInfo)) continue;
			    DrawButtons(methodInfo, targets);
		    }
	    }

	    public static void DrawButtons(MethodInfo methodInfo, Object[] targets)
		{
			object[] attributes = methodInfo.GetCustomAttributes(true);
			foreach (Attribute attribute in attributes)
			{
				if (attribute is ButtonAttribute buttonAttribute)
					DrawButton(methodInfo, buttonAttribute, targets);
			}
		}

		public static void DrawButton(MethodInfo methodInfo, ButtonAttribute buttonAttribute, Object[] targets)
		{
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
						Debug.LogError(e, target);
					}
				}
			}
		}
    }
}