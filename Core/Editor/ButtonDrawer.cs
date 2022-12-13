using System;
using System.Collections.Generic;
using System.Reflection;
using CustomizationInspector.Runtime;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CustomizationInspector.Editor
{
    public class ButtonDrawer : Drawer
    {
	    private static List<object> parameters = new List<object>();
	    private MethodInfo[] methodInfos;
	    private Dictionary<MethodInfo, List<ButtonAttribute>> methodButtonAttributes;
		private FoldoutDrawer foldoutDrawer;
	    
	    public ButtonDrawer(SerializedObject serializedObject, Object[] targets) : base(serializedObject, targets)
        {
	        methodInfos = targetType.GetMethods(BindingFlags);
	        methodButtonAttributes = new Dictionary<MethodInfo, List<ButtonAttribute>>(methodInfos.Length);
	        for (var i = 0; i < methodInfos.Length; i++)
	        {
		        var methodInfo = methodInfos[i];
		        object[] attributes = methodInfo.GetCustomAttributes(true);
		        List<ButtonAttribute> buttonAttributes = new List<ButtonAttribute>();
		        methodButtonAttributes.Add(methodInfo, buttonAttributes);
		        foreach (object attribute in attributes)
		        {
			        if (attribute is ButtonAttribute buttonAttribute)
				        buttonAttributes.Add(buttonAttribute);
		        }
	        }
        }

	    public void SetFoldoutDrawer(FoldoutDrawer drawer)
	    {
		    foldoutDrawer = drawer;
			drawer?.SetDrawMethodHandler(DrawButtons);
	    }
        
	    public override void Draw()
	    {
		    foreach (var methodInfo in methodInfos)
		    {
			    if (foldoutDrawer != null && foldoutDrawer.IsFoldout(methodInfo)) continue;
			    DrawButtons(methodInfo, targets);
		    }
	    }

	    public void DrawButtons(MethodInfo methodInfo, Object[] targets)
		{
			if (methodButtonAttributes.TryGetValue(methodInfo, out var buttonAttributes))
			{
				foreach (var buttonAttribute in buttonAttributes)
					DrawButton(methodInfo, buttonAttribute, targets);
			}
			else
			{
				object[] attributes = methodInfo.GetCustomAttributes(true);
				foreach (Attribute attribute in attributes)
				{
					if (attribute is ButtonAttribute buttonAttribute)
						DrawButton(methodInfo, buttonAttribute, targets);
				}
			}
		}

		public static void DrawButton(MethodInfo methodInfo, ButtonAttribute buttonAttribute, Object[] targets)
		{
			parameters.Clear();
			if (buttonAttribute.Params != null)
				parameters.AddRange(buttonAttribute.Params);
			var parameterInfos = methodInfo.GetParameters();
			bool canDraw = parameters.Count <= parameterInfos.Length;
			if (parameters.Count < parameterInfos.Length)
			{
				for (int i = parameters.Count, len = parameterInfos.Length; i < len; i++)
				{
					var parameter = parameterInfos[i];
					if (parameter.HasDefaultValue)
					{
						//Add default value
						parameters.Add(parameter.DefaultValue);
						continue;
					}
					var defaultValueAttribute = parameter.GetCustomAttribute<System.ComponentModel.DefaultValueAttribute>();
					if (defaultValueAttribute != null)
					{
						//Add default value
						parameters.Add(defaultValueAttribute.Value);
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
						methodInfo.Invoke(target, parameters.ToArray());
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